using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Depso;

public class FactoryRewriter : CSharpSyntaxRewriter
{
	private readonly Compilation _compilation;
	private readonly bool _isScopeClass;
	private readonly bool _replaceServiceProviderToThis;

	private HashSet<SyntaxNode>? _nodesToReplace;

	public FactoryRewriter(Compilation compilation, bool isScopeClass, bool replaceServiceProviderToThis)
	{
		_compilation = compilation;
		_isScopeClass = isScopeClass;
		_replaceServiceProviderToThis = replaceServiceProviderToThis;
	}

	public override SyntaxNode? Visit(SyntaxNode? node)
	{
		if (ReplaceNode(node, out SyntaxNode? replacement))
		{
			return replacement;
		}

		return base.Visit(node);
	}

	public override SyntaxNode? VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
	{
		SemanticModel semanticModel = _compilation.GetSemanticModel(node.SyntaxTree);
		ISymbol? symbol = semanticModel.GetDeclaredSymbol(node.Parameter);

		if (symbol == null)
		{
			return base.VisitSimpleLambdaExpression(node);
		}

		IOperation? operation = semanticModel.GetOperation(node.Body);
		ParameterReferenceFinder parameterReferenceFinder = new(symbol);

		parameterReferenceFinder.Visit(operation);

		if (parameterReferenceFinder.ReferenceNodes == null)
		{
			return base.VisitSimpleLambdaExpression(node);
		}

		_nodesToReplace = parameterReferenceFinder.ReferenceNodes;

		return base.VisitSimpleLambdaExpression(node);
	}
	public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
	{
		return ProcessNode(node, base.VisitQualifiedName);
	}

	public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
	{
		return ProcessNode(node, base.VisitIdentifierName);
	}

	public override SyntaxNode? VisitGenericName(GenericNameSyntax node)
	{
		return ProcessNode(node, base.VisitGenericName);
	}

	public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
	{
		return ProcessNode(node, base.VisitMemberAccessExpression);
	}

	private SyntaxNode? ProcessNode<T>(T node, Func<T, SyntaxNode?> visitFunction) where T : SyntaxNode
	{
		if (ReplaceNode(node, out SyntaxNode? replacement))
		{
			return replacement;
		}

		SyntaxNode fullyQualifiedName = ToFullyQualifiedName(node);

		if (fullyQualifiedName != node)
		{
			return fullyQualifiedName;
		}

		return visitFunction(node);
	}
	
	private SyntaxNode ToFullyQualifiedName(SyntaxNode node)
	{
		if (_compilation.ContainsSyntaxTree(node.SyntaxTree))
		{
			
		}

		SymbolInfo symbolInfo = _compilation.ContainsSyntaxTree(node.SyntaxTree)
			? _compilation
				.GetSemanticModel(node.SyntaxTree)
				.GetSymbolInfo(node)
			: new SymbolInfo();

		if (symbolInfo.Symbol is INamedTypeSymbol namedTypeSymbol)
		{
			return ParseName(namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
		}

		if (symbolInfo.Symbol is IPropertySymbol propertySymbol)
		{
			if (!propertySymbol.IsStatic)
			{
				return ParseMember(node);
			}
			
			string property = propertySymbol.Name;
			string containingType = propertySymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			return ParseName($"{ParseName(containingType)}.{property}");
		}

		if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
		{
			if (!fieldSymbol.IsStatic)
			{
				return ParseMember(node);
			}
			
			string field = fieldSymbol.Name;
			string containingType = fieldSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			return ParseName($"{ParseName(containingType)}.{field}");
		}

		if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
		{
			if (!methodSymbol.IsStatic)
			{
				return ParseMember(node);
			}

			string method = methodSymbol.Name;
			string containingType = methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			
			return ParseName($"{ParseName(containingType)}.{method}");
		}
		
		return node;
	}

	private SyntaxNode ParseMember(SyntaxNode syntaxNode)
	{
		if (!_isScopeClass)
		{
			return syntaxNode;
		}

		return MemberAccessExpression(
			SyntaxKind.SimpleMemberAccessExpression,
			IdentifierName("_root"),
			IdentifierName(syntaxNode.ToString()));
	}

	private bool ReplaceNode(SyntaxNode? node, out SyntaxNode? replacement)
	{
		if (_replaceServiceProviderToThis && node != null && _nodesToReplace?.Contains(node) == true)
		{
			replacement = ThisExpression();
			return true;
		}

		replacement = null;
		return false;
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