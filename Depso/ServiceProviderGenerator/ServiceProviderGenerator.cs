using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Text;
using Depso.CSharp;
using Depso.Generators;
using Depso.Generators.Scope;
using static Depso.Attributes;
using ScopedGetServicesGenerator = Depso.Generators.ScopedGetServicesGenerator;
using SingletonGetServicesGenerator = Depso.Generators.SingletonGetServicesGenerator;

namespace Depso;

[Generator]
public partial class ServiceProviderGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(i =>
		{
			i.AddSource(
				$"{Constants.GeneratorNamespace}.Attributes.ServiceProvider.g.cs",
				ServiceProvider.SourceCode);

			i.AddSource(
				$"{Constants.GeneratorNamespace}.Attributes.ServiceProviderModule.g.cs",
				ServiceProviderModule.SourceCode);
		});

		IncrementalValuesProvider<ClassDeclarationSyntax?> classDeclarations = context.SyntaxProvider
			.CreateSyntaxProvider(
				static (x, _) => IsSyntaxTargetForGeneration(x),
				static (x, _) => GetSemanticTargetForGeneration(x))
			.Where(static x => x is not null);

		context.RegisterSourceOutput(
			context.CompilationProvider.Combine(classDeclarations.Collect()),
			static (compilation, source) => Execute(source.Left, source.Right, compilation));
	}

	private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
	{
		return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
	}

	private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
	{
		if (context.Node is not ClassDeclarationSyntax classSyntax)
		{
			return null;
		}

		INamedTypeSymbol? symbol = context.SemanticModel.GetDeclaredSymbol(classSyntax);

		if (symbol == null)
		{
			return null;
		}

		Func<AttributeData, bool> attributeChecker = x =>
		{
			string? displayString = x.AttributeClass?.ToDisplayString();
			return displayString == ServiceProvider.FullName || displayString == ServiceProviderModule.FullName;
		};

		if (symbol.GetAttributes().Any(attributeChecker))
		{
			return classSyntax;
		}

		return null;
	}

	public static void Execute(
		Compilation compilation,
		ImmutableArray<ClassDeclarationSyntax?> classes,
		SourceProductionContext context)
	{
		KnownTypes knownTypes = new(compilation);

		foreach (ClassDeclarationSyntax? @class in classes.Distinct())
		{
			if (context.CancellationToken.IsCancellationRequested)
			{
				return;
			}

			if (@class == null)
			{
				continue;
			}

			INamedTypeSymbol? classSymbol = compilation.GetSemanticModel(@class.SyntaxTree).GetDeclaredSymbol(@class);

			if (classSymbol == null)
			{
				continue;
			}

			if (!@class.Modifiers.Any(SyntaxKind.PartialKeyword))
			{
				string typeName = classSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

				Diagnostic diagnostic = Diagnostic.Create(
					Diagnostics.ClassNotPartial,
					Location.Create(@class.SyntaxTree, @class.Identifier.Span),
					typeName);

				context.ReportDiagnostic(diagnostic);

				continue;
			}

			bool isModule = classSymbol
				.GetAttributes()
				.Any(x => x.AttributeClass?.ToDisplayString() == ServiceProviderModule.FullName);

			ProcessServiceProvider(compilation, context, knownTypes, @class, classSymbol, isModule);
		}
	}

	private static void ProcessServiceProvider(
		Compilation compilation,
		SourceProductionContext context,
		KnownTypes knownTypes,
		ClassDeclarationSyntax @class,
		INamedTypeSymbol classSymbol,
		bool isModule)
	{
		string registrationMethods = CreateRegistrationMethods(classSymbol, isStatic: isModule);
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(registrationMethods);

		context.AddSource($"{Constants.GeneratorNamespace}.{classSymbol.ToDisplayString()}.RegistrationMethods.g.cs", registrationMethods);

		compilation = compilation.AddSyntaxTrees(syntaxTree);
		classSymbol = compilation.GetSemanticModel(@class.SyntaxTree).GetDeclaredSymbol(@class)!;

		IMethodSymbol? registerServicesMethod = classSymbol.GetMembers()
			.OfType<IMethodSymbol>()
			.FirstOrDefault(x => x.IsRegisterServicesMethod(isStatic: isModule));

		if (registerServicesMethod == null)
		{
			DiagnosticDescriptor descriptor = isModule
				? Diagnostics.RegisterServicesStaticMethodNotFound
				: Diagnostics.RegisterServicesMethodNotFound;

			Diagnostic diagnostic = Diagnostic.Create(
				descriptor,
				Location.Create(@class.SyntaxTree, @class.Identifier.Span),
				classSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

			context.ReportDiagnostic(diagnostic);

			return;
		}

		GenerationContext generationContext = new(
			context,
			compilation,
			knownTypes,
			@class,
			classSymbol,
			registerServicesMethod,
			isModule);

		if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
		{
			// TODO: Handle nested classes.
			return;
		}

		if (!PopulateServices(generationContext))
		{
			return;
		}

		generationContext.ComputeDependencyGraph();

		if (!generationContext.IsDependencyGraphValid())
		{
			// TODO: Add the source even if it is invalid.
			return;
		}
		
		string classSource = ProcessClass(generationContext);
		string scopeClassSource = ProcessScopeClass(generationContext);

		context.AddSource(
			$"{Constants.GeneratorNamespace}.{classSymbol.ToDisplayString()}.g.cs",
			classSource);

		context.AddSource(
			$"{Constants.GeneratorNamespace}.{classSymbol.ToDisplayString()}.Scoped.g.cs",
			scopeClassSource);
	}

	private static string ProcessClass(GenerationContext generationContext)
	{
		INamedTypeSymbol classSymbol = generationContext.ClassSymbol;

		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		
		codeBuilder.AppendLine("// <auto-generated/>");
		codeBuilder.AppendLine();

		codeBuilder.AppendLine("#nullable enable");
		codeBuilder.AppendLine();

		using (AddNamespace(classSymbol, codeBuilder))
		{
			if (generationContext.IsModule)
			{
				foreach (ServiceDescriptor serviceDescriptor in generationContext.ServiceDescriptors)
				{
					generationContext.IndexManager.Add(serviceDescriptor);
				}
				
				AddModuleAttributes(codeBuilder);

				foreach (ServiceDescriptor serviceDescriptor in generationContext.ServiceDescriptors)
				{
					AddRegistrationAttributeToModuleClass(generationContext, codeBuilder, serviceDescriptor);
				}

				codeBuilder.AppendLine($"[global::{Constants.GeneratedModuleAttributeClassName}]");
			}

			using (ClassBuilder classBuilder = AddClass(classSymbol, codeBuilder))
			{
				if (generationContext.IsModule)
				{
					CreateModuleFactoryMethods(generationContext);
				}
				else
				{
					AddClassInterfaces(generationContext.KnownTypes, classBuilder);

					List<IGenerator> generators = new()
					{
						new SingletonGetServicesGenerator(),
						new ScopedGetServicesGenerator(),
						new TransientGetServicesGenerator(),
						new EnumerableGetServicesGenerator(),

						new LockGenerator(),
						new RootScopeGenerator(),
						new CommonDisposableFieldsGenerator(),
						new DisposableFieldsGenerator(),
						new AsyncDisposableFieldsGenerator(),
						new SingletonFieldsGenerator(),
						new SingletonFieldsFactoryGenerator(),
						new EnumerableFieldsGenerator(),

						new ServiceScopeFactoryGenerator(),

						new GetServiceMethodGenerator(),
						new GetServiceGenericMethodGenerator(),

						new SingletonCreateMethodsGenerator(),
						new TransientCreateMethodsGenerator(),

						new CreateScopeMethodGenerator(),

						new ServiceProviderIsServiceGenerator(),

						new DisposeMethodGenerator(),
						new DisposeAsyncMethodGenerator(),
						new ThrowIfDisposedMethodGenerator(),

						new AddDisposableMethodGenerator(),
						new AddAsyncDisposableMethodGenerator(),
					};

					foreach (IGenerator generator in generators)
					{
						generator.Generate(generationContext);
					}

					foreach (Action<GenerationContext> action in generationContext.Actions)
					{
						action(generationContext);
					}
				}
			}
		}

		return codeBuilder.ToString();
	}

	private static string ProcessScopeClass(GenerationContext generationContext)
	{
		generationContext.Reset();

		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		codeBuilder.Clear();

		codeBuilder.AppendLine("// <auto-generated/>");
		codeBuilder.AppendLine();

		codeBuilder.AppendLine("#nullable enable");
		codeBuilder.AppendLine();

		INamedTypeSymbol classSymbol = generationContext.ClassSymbol;

		using (AddNamespace(classSymbol, codeBuilder))
		using (AddClass(classSymbol, codeBuilder))
		{
			using (ClassBuilder classBuilder = codeBuilder.Class(Constants.ScopeClassName).Public())
			{
				AddClassInterfaces(generationContext.KnownTypes, classBuilder);

				List<IGenerator> generators = new()
				{
					new Generators.Scope.SingletonGetServicesGenerator(),
					new Generators.Scope.ScopedGetServicesGenerator(),
					new TransientGetServicesGenerator(),
					new EnumerableGetServicesGenerator(),

					new LockGenerator(),
					new RootFieldGenerator(),
					new CommonDisposableFieldsGenerator(),
					new DisposableFieldsGenerator(),
					new AsyncDisposableFieldsGenerator(),
					new ScopedFieldsGenerator(),
					new ScopedFieldsFactoryGenerator(),
					new EnumerableFieldsGenerator(),

					new ServiceScopeGenerator(),

					new ConstructorGenerator(),

					new GetServiceMethodGenerator(),
					new GetServiceGenericMethodGenerator(),

					new ScopedCreateMethodsGenerator(),
					new TransientCreateMethodsGenerator(),
					
					new DisposeMethodGenerator(),
					new DisposeAsyncMethodGenerator(),
					new ThrowIfDisposedMethodGenerator(),

					new AddDisposableMethodGenerator(),
					new AddAsyncDisposableMethodGenerator(),
				};

				foreach (IGenerator generator in generators)
				{
					generator.Generate(generationContext);
				}

				foreach (Action<GenerationContext> action in generationContext.Actions)
				{
					action(generationContext);
				}
			}
		}

		return codeBuilder.ToString();
	}
	
	private static void AddModuleAttributes(CodeBuilder codeBuilder)
	{
		const string generatedModuleAttribute = $$"""
			[global::System.AttributeUsage(global::System.AttributeTargets.Class)]
			file class {{Constants.GeneratedModuleAttributeClassName}} : global::System.Attribute
			{
			}
			""";

		codeBuilder.AppendLine(generatedModuleAttribute);
		codeBuilder.AppendLine();

		AddAttributeClass("Singleton");
		AddAttributeClass("Scoped");
		AddAttributeClass("Transient");

		void AddAttributeClass(string name)
		{
			string attributeClass = $$"""
				[global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true)]
				file class {{name}}Attribute : global::System.Attribute
				{
				    public {{name}}Attribute(
				        global::System.Type serviceType,
				        global::System.Type? implementationType = null,
				        string? factory = null,
				        params global::System.Type[] registerAlsoAs)
				    {
				    }
				}
				""";

			codeBuilder.AppendLine(attributeClass);
			codeBuilder.AppendLine();
		}
	}

	private static void CreateModuleFactoryMethods(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		foreach (ServiceDescriptor serviceDescriptor in generationContext.ServiceDescriptors)
		{
			SyntaxNode? factory = serviceDescriptor.Factory;

			if (factory == null)
			{
				continue;
			}

			ParameterSyntax? parameter = null;

			if (factory is AnonymousMethodExpressionSyntax anonymousMethod)
			{
				parameter = anonymousMethod.ParameterList?.Parameters.FirstOrDefault();
			}
			else if (factory is SimpleLambdaExpressionSyntax simpleLambdaExpression)
			{
				parameter = simpleLambdaExpression.Parameter;
			}
			else if (factory is ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpression)
			{
				parameter = parenthesizedLambdaExpression.ParameterList.Parameters.FirstOrDefault();
			}

			string serviceProviderParameter = parameter == null
				? "serviceProvider"
				: parameter.Identifier.ToString();

			string factoryMethodName = serviceDescriptor.GetFactoryMethodName();
			string factoryInvocation = generationContext.GetFactoryInvocation(
				factory,
				serviceProviderParameter);

			INamedTypeSymbol serviceType = serviceDescriptor.ServiceType;

			string fieldTypeName = serviceType
				.WithNullableAnnotation(NullableAnnotation.None)
				.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			generationContext.AddNewLineIfNecessary();

			using (MethodBuilder method = codeBuilder.Method(returnType: fieldTypeName, name: factoryMethodName).Public().Static())
			{
				method.AddParameter(
					generationContext.KnownTypes.IServiceProvider.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
					serviceProviderParameter);

				using System.IO.StringReader reader = new(factoryInvocation);

				while (reader.ReadLine() is { } line)
				{
					codeBuilder.AppendLine(line.TrimStart());
				}
			}

			generationContext.AddNewLine = true;
		}
	}

	private static void AddRegistrationAttributeToModuleClass(
		GenerationContext generationContext,
		CodeBuilder codeBuilder,
		ServiceDescriptor serviceDescriptor)
	{
		INamedTypeSymbol service = serviceDescriptor.ServiceType;
		INamedTypeSymbol? implementation = serviceDescriptor.ImplementationType;

		StringBuilder stringBuilder = new();

		stringBuilder.Append($"typeof({service.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})");

		if (implementation != null)
		{
			stringBuilder.Append(", ");
			stringBuilder.Append($"typeof({implementation.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})");
		}
		else
		{
			stringBuilder.Append(", null");
		}

		if (serviceDescriptor.Factory != null)
		{
			string factoryMethodName = serviceDescriptor.GetFactoryMethodName();
			string className = generationContext.ClassSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			stringBuilder.Append(", ");
			stringBuilder.Append($"\"{className}.{factoryMethodName}\"");
		}
		else
		{
			stringBuilder.Append(", null");
		}

		if (serviceDescriptor.AlsoRegisterAs != null)
		{
			foreach (INamedTypeSymbol alsoRegisterAs in serviceDescriptor.AlsoRegisterAs)
			{
				stringBuilder.Append(", ");
				stringBuilder.Append($"typeof({alsoRegisterAs.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})");
			}
		}

		codeBuilder.AppendLine($"[global::{serviceDescriptor.Lifetime}({stringBuilder})]");
	}

	private static void AddClassInterfaces(KnownTypes knownTypes, ClassBuilder classBuilder)
	{
		void AddInterfaceIfPossible(INamedTypeSymbol? symbol, string interfaceName)
		{
			if (symbol != null)
			{
				classBuilder.Base(interfaceName.WithGlobalPrefix());
			}
		}

		AddInterfaceIfPossible(knownTypes.IDisposable, Constants.IDisposableMetadataName);
		AddInterfaceIfPossible(knownTypes.IAsyncDisposable, Constants.IAsyncDisposableMetadataName);
		AddInterfaceIfPossible(knownTypes.IServiceProvider, Constants.IServiceProviderMetadataName);
		AddInterfaceIfPossible(knownTypes.IServiceScope, Constants.IServiceScopeMetadataName);
		AddInterfaceIfPossible(knownTypes.IServiceScopeFactory, Constants.IServiceScopeFactoryMetadataName);
		AddInterfaceIfPossible(knownTypes.IServiceProviderIsService, Constants.IServiceProviderIsServiceMetadataName);
	}

	private static IDisposable AddNamespace(INamedTypeSymbol classSymbol, CodeBuilder builder)
	{
		return classSymbol.ContainingNamespace.IsGlobalNamespace
			? Disposable.Empty
			: builder.Namespace(classSymbol.ContainingNamespace.ToDisplayString());
	}

	private static ClassBuilder AddClass(INamedTypeSymbol classSymbol, CodeBuilder builder)
	{
		string visibility = SyntaxFacts.GetText(classSymbol.DeclaredAccessibility);

		ClassBuilder classBuilder = builder.Class(classSymbol.Name).Partial();
		classBuilder.Visibility(visibility);

		return classBuilder;
	}
}