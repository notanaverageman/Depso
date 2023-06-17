using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Depso;

public partial class ServiceProviderGenerator
{
	private static bool PopulateServices(GenerationContext generationContext)
	{
		// Add service provider interfaces to be able to resolve them via this instance.
		void AddIfNotNull(INamedTypeSymbol? symbol, Lifetime lifetime)
		{
			if (symbol != null)
			{
				ServiceDescriptor serviceDescriptor = new(lifetime, symbol)
				{
					RedirectToThis = true
				};

				generationContext.AddServiceDescriptor(serviceDescriptor);
			}
		}

		AddIfNotNull(generationContext.KnownTypes.IServiceProvider, Lifetime.Singleton);
		AddIfNotNull(generationContext.KnownTypes.IServiceProvider, Lifetime.Scoped);
		AddIfNotNull(generationContext.KnownTypes.IServiceScope, Lifetime.Scoped);
		AddIfNotNull(generationContext.KnownTypes.IServiceScopeFactory, Lifetime.Singleton);
		AddIfNotNull(generationContext.KnownTypes.IServiceProviderIsService, Lifetime.Singleton);

		return PopulateServices(generationContext, generationContext.RegisterServicesMethod);
	}

	private static bool PopulateServices(GenerationContext generationContext, IMethodSymbol registerServicesMethod)
	{
		MethodDeclarationSyntax? methodSyntax = registerServicesMethod.DeclaringSyntaxReferences
			.Select(x => x.GetSyntax())
			.OfType<MethodDeclarationSyntax>()
			.SingleOrDefault(x => x.Body != null);

		if (methodSyntax == null)
		{
			SyntaxNode syntax = registerServicesMethod.DeclaringSyntaxReferences.First().GetSyntax();

			DiagnosticDescriptor diagnosticDescriptor = Diagnostics.RegisterServicesMethodNotFound;

			if (generationContext.IsModule)
			{
				diagnosticDescriptor = Diagnostics.RegisterServicesStaticMethodNotFound;
			}

			Diagnostic diagnostic = Diagnostic.Create(
				diagnosticDescriptor,
				Location.Create(syntax.SyntaxTree, syntax.Span),
				registerServicesMethod.ContainingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

			generationContext.SourceProductionContext.ReportDiagnostic(diagnostic);

			return false;
		}

		foreach (StatementSyntax statement in methodSyntax.Body!.Statements)
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

	private static void ImportModule(GenerationContext generationContext, GenericNameSyntax generic)
	{
		if (generic.TypeArgumentList.Arguments.Count != 1)
		{
			ReportIllegalStatementDiagnostic(generationContext, generic);
			return;
		}

		TypeSyntax moduleTypeSyntax = generic.TypeArgumentList.Arguments[0];
		ISymbol? moduleTypeSymbol = generationContext.Compilation.GetSymbolInfo(moduleTypeSyntax).Symbol;

		if (moduleTypeSymbol is not INamedTypeSymbol moduleType)
		{
			ReportIllegalStatementDiagnostic(generationContext, moduleTypeSyntax);
			return;
		}

		ImportModule(generationContext, moduleTypeSyntax, moduleType);
	}

	private static void ImportModule(GenerationContext generationContext, TypeSyntax typeSyntax)
	{
		ITypeSymbol? moduleTypeSymbol = generationContext.Compilation.GetTypeInfo(typeSyntax).Type;

		if (moduleTypeSymbol is not INamedTypeSymbol moduleType)
		{
			ReportIllegalStatementDiagnostic(generationContext, typeSyntax);
			return;
		}

		ImportModule(generationContext, typeSyntax, moduleType);
	}

	private static void ImportModule(
		GenerationContext generationContext,
		TypeSyntax moduleSyntax,
		INamedTypeSymbol moduleType)
	{
		// Get RegisterServices method expressions from module class if it is defined
		// in the same assembly.
		// Otherwise, the services are extracted from the attributes on module class.

		IMethodSymbol? registerServicesMethod = moduleType.GetMembers()
			.OfType<IMethodSymbol>()
			.FirstOrDefault(x => x.IsRegisterServicesMethod(isStatic: true));

		AttributeData? generatedModuleAttribute = moduleType
			.GetAttributes()
			.FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == Constants.GeneratedModuleAttributeClassName);

		if (registerServicesMethod == null && generatedModuleAttribute == null)
		{
			Diagnostic diagnostic = Diagnostic.Create(
				Diagnostics.RegisterServicesStaticMethodNotFound,
				Location.Create(moduleSyntax.SyntaxTree, moduleSyntax.Span),
				moduleType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

			generationContext.SourceProductionContext.ReportDiagnostic(diagnostic);

			return;
		}

		if (registerServicesMethod != null && registerServicesMethod.DeclaringSyntaxReferences.Length != 0)
		{
			// Parse the registration expressions inside the method body.
			PopulateServices(generationContext, registerServicesMethod);
		}
		else if (generatedModuleAttribute != null)
		{
			// Get the registrations from attributes.
			foreach (AttributeData attributeData in moduleType.GetAttributes())
			{
				ProcessModuleAttribute(generationContext, moduleSyntax, moduleType, attributeData);
			}
		}
	}

	private static void ProcessModuleAttribute(
		GenerationContext generationContext,
		TypeSyntax moduleSyntax,
		INamedTypeSymbol moduleSymbol,
		AttributeData attribute)
	{
		string? attributeName = attribute.AttributeClass?.ToDisplayString();
		ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;

		if (attributeName == Constants.GeneratedModuleAttributeClassName)
		{
			return;
		}

		if (arguments.Length == 0)
		{
			ReportIllegalStatementDiagnostic(generationContext, moduleSyntax);
			return;
		}

		Lifetime? lifetime = attributeName switch
		{
			$"{nameof(Lifetime.Singleton)}Attribute" => Lifetime.Singleton,
			$"{nameof(Lifetime.Scoped)}Attribute" => Lifetime.Scoped,
			$"{nameof(Lifetime.Transient)}Attribute" => Lifetime.Transient,
			_ => null
		};

		if (lifetime == null)
		{
			ReportIllegalStatementDiagnostic(generationContext, moduleSyntax);
			return;
		}

		if (arguments[0].Value is not INamedTypeSymbol serviceType)
		{
			ReportIllegalStatementDiagnostic(generationContext, moduleSyntax);
			return;
		}

		MemberAccessContext memberAccessContext = new();

		INamedTypeSymbol? implementationType = arguments.Length > 1 ? arguments[1].Value as INamedTypeSymbol : null;
		string? factoryMethod = arguments.Length > 2 ? arguments[2].Value as string : null;
		
		if (arguments.Length > 3)
		{
			ImmutableArray<TypedConstant> array = arguments[3].Values;

			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Value is INamedTypeSymbol argument)
				{
					memberAccessContext.AddAlsoRegisterAs(argument);
				}
			}
		}

		SyntaxNode? factorySyntax = factoryMethod == null
			? null
			: CSharpSyntaxTree.ParseText(factoryMethod).GetRoot();

		generationContext.AddServiceDescriptor(new ServiceDescriptor(
			lifetime.Value,
			serviceType,
			implementationType,
			factorySyntax,
			alsoRegisterAs: memberAccessContext.AlsoRegisterAs));
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
			TypeInfo typeInfo = generationContext.Compilation.GetTypeInfo(typeSyntax);

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

		if (identifier == Constants.ImportModuleMethodName)
		{
			ImportModule(generationContext, generic);
			return;
		}

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
			TypeInfo typeInfo = generationContext.Compilation.GetTypeInfo(typeSyntax);
			return typeInfo.Type as INamedTypeSymbol;
		}
	}

	private static void PopulateFromIdentifier(
		GenerationContext generationContext,
		IdentifierNameSyntax identifierName,
		MemberAccessContext memberAccessContext)
	{
		if (identifierName.Parent is not InvocationExpressionSyntax invocation)
		{
			ReportIllegalStatementDiagnostic(generationContext, identifierName);
			return;
		}

		SeparatedSyntaxList<ArgumentSyntax> arguments = invocation.ArgumentList.Arguments;
		string identifier = identifierName.Identifier.Text;

		if (identifier == Constants.ImportModuleMethodName)
		{
			if (arguments.Count != 1 || arguments.First().Expression is not TypeOfExpressionSyntax typeOfExpression)
			{
				ReportIllegalStatementDiagnostic(generationContext, identifierName);
				return;
			}

			ImportModule(generationContext, typeOfExpression.Type);
			return;
		}

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
			TypeInfo typeInfo = generationContext.Compilation.GetTypeInfo(typeSyntax);
			return typeInfo.Type as INamedTypeSymbol;
		}
	}

	private static bool HandleFunc(
		SyntaxNode syntaxNode,
		Lifetime lifetime,
		GenerationContext generationContext,
		MemberAccessContext memberAccessContext)
	{
		ISymbol? symbol = generationContext.Compilation.GetSymbolInfo(syntaxNode).Symbol;

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

		bool isServiceProvider = parameter.Type.SymbolEquals(generationContext.KnownTypes.IServiceProvider);
		bool isErrorType = parameter.Type is IErrorTypeSymbol;

		// Roslyn doesn't fill the parameter type if generic argument is not specified on call site.
		// Assume that it is the correct type.
		if (!(isServiceProvider || isErrorType))
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
		Location? location = Location.Create(node.SyntaxTree, node.Span);

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

		if(location.SourceTree != null && !generationContext.Compilation.ContainsSyntaxTree(location.SourceTree))
		{
			location = null;
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