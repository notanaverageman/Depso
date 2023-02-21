using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace Depso;

public partial class ServiceProviderGenerator
{
	private static bool PopulateServices(GenerationContext generationContext)
	{
		IMethodSymbol registerServicesMethod = generationContext.RegisterServicesMethod;
		SyntaxReference syntaxReference = registerServicesMethod.DeclaringSyntaxReferences[0];

		if (syntaxReference.GetSyntax() is not MethodDeclarationSyntax methodSyntax || methodSyntax.Body == null)
		{
			Diagnostic diagnostic = Diagnostic.Create(
				Diagnostics.RegisterServicesMethodNotFound,
				Location.Create(syntaxReference.SyntaxTree, syntaxReference.Span));

			generationContext.SourceProductionContext.ReportDiagnostic(diagnostic);

			return false;
		}

		foreach (StatementSyntax statement in methodSyntax.Body.Statements)
		{
			if (statement is not ExpressionStatementSyntax expressionStatement)
			{
				ReportIllegalStatementDiagnostic(generationContext, statement);
				continue;
			}

			MemberAccessContext memberAccessContext = new();

			ProcessExpression(generationContext, expressionStatement.Expression, memberAccessContext);
		}

		return true;
	}

	private static void ProcessExpression(
		GenerationContext generationContext,
		ExpressionSyntax expressionSyntax,
		MemberAccessContext memberAccessContext)
	{
		switch (expressionSyntax)
		{
			case GenericNameSyntax generic:
				PopulateFromGeneric(generationContext, generic, memberAccessContext);
				break;
			case IdentifierNameSyntax identifierName:
				PopulateFromIdentifier(generationContext, identifierName, memberAccessContext);
				break;
			case MemberAccessExpressionSyntax memberAccess:
				ProcessMemberAccess(generationContext, memberAccess, memberAccessContext);
				break;
			case InvocationExpressionSyntax invocation:
				ProcessExpression(generationContext, invocation.Expression, memberAccessContext);
				break;
			default:
				ReportIllegalStatementDiagnostic(generationContext, expressionSyntax);
				break;
		}
	}

	private static void ProcessMemberAccess(
		GenerationContext generationContext,
		MemberAccessExpressionSyntax memberAccess,
		MemberAccessContext memberAccessContext)
	{
		SyntaxToken memberName = memberAccess.Name.Identifier;

		if (memberName.ValueText == Constants.AlsoAsSelfMethodName)
		{
			memberAccessContext.RegisterAsSelf = true;
			ProcessExpression(generationContext, memberAccess.Expression, memberAccessContext);

			return;
		}

		if (memberName.ValueText == Constants.AlsoAsMethodName &&
		    memberAccess.Parent is InvocationExpressionSyntax { ArgumentList.Arguments.Count: 1 } invocationExpression &&
		    invocationExpression.ArgumentList.Arguments.First().Expression is TypeOfExpressionSyntax typeOfExpression)
		{
			ProcessAlsoAs(typeOfExpression.Type);
			return;
		}

		if (memberName.ValueText == Constants.AlsoAsMethodName &&
		    memberAccess.Name is GenericNameSyntax { TypeArgumentList.Arguments.Count: 1 } genericName)
		{
			ProcessAlsoAs(genericName.TypeArgumentList.Arguments.First());
			return;
		}

		void ProcessAlsoAs(TypeSyntax typeSyntax)
		{
			TypeInfo typeInfo = generationContext.Compilation
				.GetSemanticModel(typeSyntax.SyntaxTree)
				.GetTypeInfo(typeSyntax);

			if (typeInfo.Type is INamedTypeSymbol typeSymbol)
			{
				memberAccessContext.AddAlsoRegisterAs(typeSymbol);
			}

			ProcessExpression(generationContext, memberAccess.Expression, memberAccessContext);
		}
		
		ReportIllegalStatementDiagnostic(generationContext, memberAccess.Name);
	}

	private static void PopulateFromGeneric(
		GenerationContext generationContext,
		GenericNameSyntax generic,
		MemberAccessContext memberAccessContext)
	{
		string identifier = generic.Identifier.Text;

		Lifetime? lifetime = identifier switch
		{
			Constants.SingletonMethodName => Lifetime.Singleton,
			Constants.ScopedMethodName => Lifetime.Scoped,
			Constants.TransientMethodName => Lifetime.Transient,
			_ => null
		};

		if (lifetime == null)
		{
			ReportIllegalStatementDiagnostic(generationContext, generic);
			return;
		}

		if (generic.Parent is not InvocationExpressionSyntax invocation)
		{
			ReportIllegalStatementDiagnostic(generationContext, generic);
			return;
		}

		if (invocation.ArgumentList.Arguments.Count == 1)
		{
			ArgumentSyntax argument = invocation.ArgumentList.Arguments[0];

			if (HandleFunc(argument.Expression, lifetime.Value, generationContext, memberAccessContext))
			{
				return;
			}
		}

		if (invocation.ArgumentList.Arguments.Count != 0)
		{
			ReportIllegalStatementDiagnostic(generationContext, generic);
			return;
		}

		SeparatedSyntaxList<TypeSyntax> typeArguments = generic.TypeArgumentList.Arguments;

		if (typeArguments.Count == 1)
		{
			INamedTypeSymbol? symbol = GetSymbol(typeArguments[0]);

			if (symbol == null)
			{
				return;
			}

			generationContext.AddServiceDescriptor(new ServiceDescriptor(
				lifetime.Value,
				symbol,
				alsoRegisterAs: memberAccessContext.AlsoRegisterAs));

			return;
		}

		if (typeArguments.Count == 2)
		{
			INamedTypeSymbol? serviceSymbol = GetSymbol(typeArguments[0]);
			INamedTypeSymbol? implementationSymbol = GetSymbol(typeArguments[1]);

			if (serviceSymbol == null || implementationSymbol == null)
			{
				return;
			}

			if (memberAccessContext.RegisterAsSelf)
			{
				memberAccessContext.AddAlsoRegisterAs(implementationSymbol);
			}

			generationContext.AddServiceDescriptor(new ServiceDescriptor(
				lifetime.Value,
				serviceSymbol,
				implementationSymbol,
				alsoRegisterAs: memberAccessContext.AlsoRegisterAs));

			return;
		}
		
		ReportIllegalStatementDiagnostic(generationContext, generic);

		INamedTypeSymbol? GetSymbol(TypeSyntax typeSyntax)
		{
			TypeInfo typeInfo = generationContext.Compilation
				.GetSemanticModel(typeSyntax.SyntaxTree)
				.GetTypeInfo(typeSyntax);

			return typeInfo.Type as INamedTypeSymbol;
		}
	}

	private static void PopulateFromIdentifier(
		GenerationContext generationContext,
		IdentifierNameSyntax identifierName,
		MemberAccessContext memberAccessContext)
	{
		string identifier = identifierName.Identifier.Text;

		Lifetime? lifetime = identifier switch
		{
			Constants.SingletonMethodName => Lifetime.Singleton,
			Constants.ScopedMethodName => Lifetime.Scoped,
			Constants.TransientMethodName => Lifetime.Transient,
			_ => null
		};

		if (lifetime == null)
		{
			ReportIllegalStatementDiagnostic(generationContext, identifierName);
			return;
		}

		if (identifierName.Parent is not InvocationExpressionSyntax invocation)
		{
			ReportIllegalStatementDiagnostic(generationContext, identifierName);
			return;
		}

		SeparatedSyntaxList<ArgumentSyntax> arguments = invocation.ArgumentList.Arguments;

		if (arguments.Count is not (1 or 2))
		{
			ReportIllegalStatementDiagnostic(generationContext, identifierName);
			return;
		}

		ExpressionSyntax firstArgument = arguments[0].Expression;

		if (arguments.Count == 2)
		{
			ExpressionSyntax secondArgument = arguments[1].Expression;

			if (firstArgument is not TypeOfExpressionSyntax typeOfService || secondArgument is not TypeOfExpressionSyntax typeOfImplementation)
			{
				ReportIllegalStatementDiagnostic(generationContext, identifierName);
				return;
			}

			INamedTypeSymbol? serviceSymbol = GetSymbol(typeOfService.Type);
			INamedTypeSymbol? implementationSymbol = GetSymbol(typeOfImplementation.Type);

			if (serviceSymbol == null || implementationSymbol == null)
			{
				return;
			}

			if (memberAccessContext.RegisterAsSelf)
			{
				memberAccessContext.AddAlsoRegisterAs(implementationSymbol);
			}

			generationContext.AddServiceDescriptor(new ServiceDescriptor(
				lifetime.Value,
				serviceSymbol,
				implementationSymbol,
				alsoRegisterAs: memberAccessContext.AlsoRegisterAs));

			return;
		}

		if (firstArgument is TypeOfExpressionSyntax typeOf)
		{
			INamedTypeSymbol? symbol = GetSymbol(typeOf.Type);

			if (symbol == null)
			{
				return;
			}

			generationContext.AddServiceDescriptor(new ServiceDescriptor(
				lifetime.Value,
				symbol,
				alsoRegisterAs: memberAccessContext.AlsoRegisterAs));

			return;
		}

		if (firstArgument is LambdaExpressionSyntax lambda)
		{
			if (lambda is SimpleLambdaExpressionSyntax s)
			{
				SemanticModel semanticModel = generationContext.Compilation.GetSemanticModel(lambda.SyntaxTree);
				SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(s.Parameter);


			}

			if (HandleFunc(lambda, lifetime.Value, generationContext, memberAccessContext))
			{
				return;
			}
		}

		if (firstArgument is IdentifierNameSyntax func)
		{
			if (HandleFunc(func, lifetime.Value, generationContext, memberAccessContext))
			{
				return;
			}
		}
		
		ReportIllegalStatementDiagnostic(generationContext, identifierName);

		INamedTypeSymbol? GetSymbol(TypeSyntax typeSyntax)
		{
			TypeInfo typeInfo = generationContext.Compilation
				.GetSemanticModel(typeSyntax.SyntaxTree)
				.GetTypeInfo(typeSyntax);

			return typeInfo.Type as INamedTypeSymbol;
		}
	}

	private static bool HandleFunc(
		SyntaxNode syntaxNode,
		Lifetime lifetime,
		GenerationContext generationContext,
		MemberAccessContext memberAccessContext)
	{
		SymbolInfo symbolInfo = generationContext.Compilation
			.GetSemanticModel(syntaxNode.SyntaxTree)
			.GetSymbolInfo(syntaxNode);

		ISymbol? symbol = symbolInfo.Symbol;

		if (HandleFuncMethod(syntaxNode, symbol, lifetime, generationContext, memberAccessContext))
		{
			return true;
		}

		if (symbol is IPropertySymbol { Type: INamedTypeSymbol propertyType })
		{
			return HandleFuncPropertyOrField(syntaxNode, propertyType, lifetime, generationContext, memberAccessContext);
		}
		
		if (symbol is IFieldSymbol { Type: INamedTypeSymbol fieldType })
		{
			return HandleFuncPropertyOrField(syntaxNode, fieldType, lifetime, generationContext, memberAccessContext);
		}
		
		return false;
	}

	private static bool HandleFuncMethod(
		SyntaxNode syntaxNode,
		ISymbol? symbol,
		Lifetime lifetime,
		GenerationContext generationContext,
		MemberAccessContext memberAccessContext)
	{
		if (symbol is not IMethodSymbol methodSymbol)
		{
			return false;
		}

		if (methodSymbol.TypeParameters.Length > 0)
		{
			return false;
		}

		if (methodSymbol.Parameters.Length != 1)
		{
			return false;
		}

		IParameterSymbol parameter = methodSymbol.Parameters[0];

		if (!parameter.Type.SymbolEquals(generationContext.KnownTypes.IServiceProvider))
		{
			return false;
		}

		if (methodSymbol.ReturnType is not INamedTypeSymbol returnType)
		{
			return false;
		}

		generationContext.AddServiceDescriptor(new ServiceDescriptor(
			lifetime,
			returnType,
			factory: syntaxNode,
			alsoRegisterAs: memberAccessContext.AlsoRegisterAs));

		return true;
	}

	private static bool HandleFuncPropertyOrField(
		SyntaxNode syntaxNode,
		INamedTypeSymbol type,
		Lifetime lifetime,
		GenerationContext generationContext,
		MemberAccessContext memberAccessContext)
	{
		if (!type.IsGenericType)
		{
			return false;
		}

		if (!type.ConstructUnboundGenericType().SymbolEquals(generationContext.KnownTypes.Func))
		{
			return false;
		}

		if (type.TypeArguments.Length != 2)
		{
			return false;
		}

		ITypeSymbol firstArgument = type.TypeArguments[0];
		ITypeSymbol secondArgument = type.TypeArguments[1];

		if (!firstArgument.SymbolEquals(generationContext.KnownTypes.IServiceProvider))
		{
			return false;
		}

		if (secondArgument is not INamedTypeSymbol namedType)
		{
			return false;
		}

		generationContext.AddServiceDescriptor(new ServiceDescriptor(
			lifetime,
			namedType,
			factory: syntaxNode,
			alsoRegisterAs: memberAccessContext.AlsoRegisterAs));

		return true;
	}

	private static void ReportIllegalStatementDiagnostic(GenerationContext generationContext, SyntaxNode node)
	{
		Location location = Location.Create(node.SyntaxTree, node.Span);

		if (node.Parent is InvocationExpressionSyntax i1)
		{
			TextSpan span = location.SourceSpan;
			TextSpan argumentSpan = i1.ArgumentList.Span;

			location = Location.Create(
				location.SourceTree!,
				new TextSpan(span.Start, span.Length + argumentSpan.Length));
		}
		else if (node.Parent?.Parent is InvocationExpressionSyntax i2)
		{
			TextSpan span = location.SourceSpan;
			TextSpan argumentSpan = i2.ArgumentList.Span;

			location = Location.Create(
				location.SourceTree!,
				new TextSpan(span.Start, span.Length + argumentSpan.Length));
		}

		// TODO: List all available statements.
		Diagnostic diagnostic = Diagnostic.Create(
			Diagnostics.IllegalStatement,
			location);

		generationContext.SourceProductionContext.ReportDiagnostic(diagnostic);
	}

	private class MemberAccessContext
	{
		private List<INamedTypeSymbol>? _alsoRegisterAs;
		
		public IReadOnlyList<INamedTypeSymbol>? AlsoRegisterAs => _alsoRegisterAs;
		public bool RegisterAsSelf { get; set; }
		
		public void AddAlsoRegisterAs(INamedTypeSymbol type)
		{
			_alsoRegisterAs ??= new List<INamedTypeSymbol>();
			_alsoRegisterAs.Add(type);
		}
	}
}