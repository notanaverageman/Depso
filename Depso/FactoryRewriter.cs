using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Depso;

public class FactoryRewriter : CSharpSyntaxRewriter
{
	private readonly GenerationContext _generationContext;
	private HashSet<SyntaxNode>? _nodesToReplace;

	private Compilation Compilation => _generationContext.Compilation;

	public FactoryRewriter(GenerationContext generationContext)
	{
		_generationContext = generationContext;
	}

	public override SyntaxNode? VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
	{
		// We need the symbol to find the parameter and replace all references to it in the body.
		ISymbol? symbol = Compilation.GetDeclaredSymbol(node.Parameter);

		if (symbol == null)
		{
			return base.VisitSimpleLambdaExpression(node);
		}

		SemanticModel semanticModel = Compilation.GetSemanticModel(node.SyntaxTree);

		IOperation? operation = semanticModel.GetOperation(node.Body);
		ParameterReferenceFinder parameterReferenceFinder = new(symbol);

		parameterReferenceFinder.Visit(operation);

		// No references to the parameter in the body.
		if (parameterReferenceFinder.ReferenceNodes == null)
		{
			return base.VisitSimpleLambdaExpression(node);
		}

		// Found the references. Replace them in subnodes.
		_nodesToReplace = parameterReferenceFinder.ReferenceNodes;
		SyntaxNode? visitedExpression = base.VisitSimpleLambdaExpression(node);
		
		// Reset the reference for next visit.
		_nodesToReplace = null;

		return visitedExpression;
	}

	public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
	{
		// Replace extension methods with static method calls.
		if (node.Expression is not MemberAccessExpressionSyntax memberAccess)
		{
			return base.VisitInvocationExpression(node);
		}

		if (!Compilation.ContainsSyntaxTree(node.SyntaxTree))
		{
			return base.VisitInvocationExpression(node);
		}

		ISymbol? symbol = GetSymbol(node);

		if (symbol is not IMethodSymbol methodSymbol)
		{
			return base.VisitInvocationExpression(node);
		}

		if (!methodSymbol.IsExtensionMethod)
		{
			return base.VisitInvocationExpression(node);
		}
		
		ExpressionSyntax expression = (ExpressionSyntax)Visit(memberAccess.Expression);
		ArgumentSyntax[] arguments = node.ArgumentList.Arguments
			.Select(VisitArgument)
			.Cast<ArgumentSyntax>()
			.ToArray();

		ArgumentListSyntax newArguments = ArgumentList();
		newArguments = newArguments.AddArguments(Argument(expression));
		newArguments = newArguments.AddArguments(arguments);

		NameSyntax typeName = ParseName(methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
		SimpleNameSyntax methodName = (SimpleNameSyntax)Visit(memberAccess.Name);

		MemberAccessExpressionSyntax newMemberAccessExpression = MemberAccessExpression(
			SyntaxKind.SimpleMemberAccessExpression,
			typeName,
			methodName);
		
		return node
			.WithExpression(newMemberAccessExpression)
			.WithArgumentList(newArguments);
	}

	public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
	{
		if (GetSymbol(node) is not INamedTypeSymbol namedTypeSymbol)
		{
			return base.VisitQualifiedName(node);
		}

		return ParseName(namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
	}

	public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
	{
		ISymbol? symbol = GetSymbol(node);

		if (symbol is INamedTypeSymbol namedTypeSymbol)
		{
			return IdentifierName(namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).WithTriviaFrom(node);
		}

		if (symbol is IFieldSymbol or IPropertySymbol or IMethodSymbol)
		{
			if (symbol.IsStatic)
			{
				string containingType = symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

				return MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					IdentifierName(containingType),
					node);
			}

			if (_generationContext is { IsModule: false, IsScopeClass: true })
			{
				return MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					IdentifierName("_root"),
					node);
			}
		}

		if (!_generationContext.IsModule && _nodesToReplace?.Contains(node) == true)
		{
			return ThisExpression();
		}

		return base.VisitIdentifierName(node);

	}

	public override SyntaxNode? VisitGenericName(GenericNameSyntax node)
	{
		if (GetSymbol(node) is not INamedTypeSymbol namedTypeSymbol)
		{
			return base.VisitGenericName(node);
		}

		SeparatedSyntaxList<TypeSyntax> typeArguments = node.TypeArgumentList.Arguments;

		if (typeArguments.Count > 0)
		{
			TypeSyntax[] newTypeArguments = new TypeSyntax[typeArguments.Count];

			for (int i = 0; i < typeArguments.Count; i++)
			{
				TypeSyntax typeArgument = typeArguments[i];

				if (GetSymbol(typeArgument) is not INamedTypeSymbol typeSymbol)
				{
					newTypeArguments[i] = typeArgument;
					continue;
				}

				newTypeArguments[i] = ParseTypeName(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
			}

			typeArguments = SeparatedList(newTypeArguments);
		}

		string name = namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		name = name.Substring(0, name.IndexOf('<'));

		return GenericName(
			Identifier(name),
			TypeArgumentList(typeArguments));
	}

	public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
	{
		ISymbol? symbol = GetSymbol(node);

		if (symbol is not (IFieldSymbol or IPropertySymbol or IMethodSymbol))
		{
			return base.VisitMemberAccessExpression(node);
		}

		SyntaxNode visitedExpression = Visit(node.Expression);
		SyntaxNode visitedName = Visit(node.Name);

		if (visitedExpression is ThisExpressionSyntax es && visitedName is MemberAccessExpressionSyntax)
		{
			return node.WithExpression(es);
		}

		if (symbol.IsStatic)
		{
			if (visitedName is MemberAccessExpressionSyntax m)
			{
				return m;
			}

			string containingType = symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			return MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				IdentifierName(containingType),
				(SimpleNameSyntax)visitedName);
		}

		if (_generationContext is { IsScopeClass: true, IsModule: false })
		{
			if (visitedName is MemberAccessExpressionSyntax m)
			{
				return m;
			}

			return MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				IdentifierName("this"),
				(SimpleNameSyntax)visitedName);
		}
		
		if (visitedExpression is ExpressionSyntax expressionSyntax)
		{
			if (visitedName is MemberAccessExpressionSyntax m)
			{
				return m;
			}

			return MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				expressionSyntax,
				(SimpleNameSyntax)visitedName);
		}

		return base.VisitMemberAccessExpression(node);
	}

	private ISymbol? GetSymbol(SyntaxNode node)
	{
		SymbolInfo symbolInfo = Compilation.GetSymbolInfo(node);
		return symbolInfo.Symbol;
	}

	private class ParameterReferenceFinder : OperationWalker
	{
		private readonly ISymbol _parameterSymbol;

		public HashSet<SyntaxNode>? ReferenceNodes { get; private set; }

		public ParameterReferenceFinder(ISymbol parameterSymbol)
		{
			_parameterSymbol = parameterSymbol;
		}

		public override void VisitParameterReference(IParameterReferenceOperation operation)
		{
			if (!operation.Parameter.SymbolEquals(_parameterSymbol))
			{
				return;
			}

			ReferenceNodes ??= new HashSet<SyntaxNode>();
			ReferenceNodes.Add(operation.Syntax);
		}
	}
}