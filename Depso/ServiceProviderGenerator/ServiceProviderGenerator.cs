using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
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
			i.AddSource($"{Constants.GeneratorNamespace}.Attributes.g.cs", ServiceProvider.SourceCode);
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

		if (symbol.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == ServiceProvider.FullName))
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

			string registrationMethods = CreateRegistrationMethods(classSymbol);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(registrationMethods);

			context.AddSource($"{Constants.GeneratorNamespace}.{classSymbol.ToDisplayString()}.RegistrationMethods.g.cs", registrationMethods);

			compilation = compilation.AddSyntaxTrees(syntaxTree);
			classSymbol = compilation.GetSemanticModel(@class.SyntaxTree).GetDeclaredSymbol(@class)!;

			IMethodSymbol? registerServicesMethod = classSymbol.GetMembers()
				.OfType<IMethodSymbol>()
				.FirstOrDefault(x => x.IsRegisterServicesMethod());

			if (registerServicesMethod == null)
			{
				Diagnostic diagnostic = Diagnostic.Create(
					Diagnostics.RegisterServicesMethodNotFound,
					Location.Create(@class.SyntaxTree, @class.Identifier.Span),
					classSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

				context.ReportDiagnostic(diagnostic);

				continue;
			}

			GenerationContext generationContext = new(
				context,
				compilation,
				knownTypes,
				@class,
				classSymbol,
				registerServicesMethod);

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
			string? scopeClassSource = ProcessScopeClass(generationContext);
			
			context.AddSource(
				$"{Constants.GeneratorNamespace}.{classSymbol.ToDisplayString()}.g.cs",
				classSource);

			if (scopeClassSource != null)
			{
				context.AddSource(
					$"{Constants.GeneratorNamespace}.{classSymbol.ToDisplayString()}.Scoped.g.cs",
					scopeClassSource);
			}
		}
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

	private static string ProcessClass(GenerationContext generationContext)
	{
		INamedTypeSymbol classSymbol = generationContext.ClassSymbol;

		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		
		codeBuilder.AppendLine("// <auto-generated/>");
		codeBuilder.AppendLine();

		codeBuilder.AppendLine("#nullable enable");
		codeBuilder.AppendLine();

		using (AddNamespace(classSymbol, codeBuilder))
		using (ClassBuilder classBuilder = AddClass(classSymbol, codeBuilder))
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

				new GetServiceMethodGenerator(),
				new GetServiceGenericMethodGenerator(),

				new SingletonCreateMethodsGenerator(),
				new TransientCreateMethodsGenerator(),

				new CreateScopeMethodGenerator(),

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

		return codeBuilder.ToString();
	}

	private static string? ProcessScopeClass(GenerationContext generationContext)
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
}