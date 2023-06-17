using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using QuickGraph.Algorithms.ConnectedComponents;
using QuickGraph;
using QuickGraph.Algorithms;
using Depso.CSharp;

namespace Depso;

public class GenerationContext
{
	private readonly Dictionary<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>> _constructorParameters;
	private readonly List<ServiceDescriptor> _serviceDescriptors;
	private readonly HashSet<INamedTypeSymbol> _enumerableTypes;
	private readonly ServiceDescriptorCache _serviceDescriptorCache;

	private readonly DependencyGraph _dependencyGraph;

	public IReadOnlyList<ServiceDescriptor> ServiceDescriptors => _serviceDescriptors;
	public IReadOnlyCollection<INamedTypeSymbol> EnumerableTypes => _enumerableTypes;

	public SourceProductionContext SourceProductionContext { get; }
	public Compilation Compilation { get; }
	public KnownTypes KnownTypes { get; }
	public ClassDeclarationSyntax ClassSyntax { get; }
	public INamedTypeSymbol ClassSymbol { get; }
	public IMethodSymbol RegisterServicesMethod { get; }
	public IndexManager IndexManager { get; }
	public CodeBuilder CodeBuilder { get; }

	public bool HasSingleton { get; private set; }
	public bool HasScoped { get; private set; }
	public bool HasTransient { get; private set; }
	
	public bool HasDisposable { get; private set; }
	public bool HasAsyncDisposable { get; private set; }
	public bool HasScopedDisposable { get; private set; }
	public bool HasScopedAsyncDisposable { get; private set; }
	public bool HasTransientDisposable { get; private set; }
	public bool HasTransientAsyncDisposable { get; private set; }

	public bool HasNonSingletonDisposable =>
		HasScopedDisposable ||
		HasScopedAsyncDisposable ||
		HasTransientDisposable ||
		HasTransientAsyncDisposable;

	public List<Action<GenerationContext>> Actions { get; }
	public List<Action<GenerationContext>> GetServicesActions { get; }

	public HashSet<ITypeSymbol> GetServicesProcessedTypes { get; }

	public bool IsModule { get; }
	public bool IsScopeClass { get; set; }
	public bool AddNewLine { get; set; }

	public GenerationContext(
		SourceProductionContext sourceProductionContext,
		Compilation compilation,
		KnownTypes knownTypes,
		ClassDeclarationSyntax classSyntax,
		INamedTypeSymbol classSymbol,
		IMethodSymbol registerServicesMethod,
		bool isModule)
	{
		SourceProductionContext = sourceProductionContext;
		Compilation = compilation;
		KnownTypes = knownTypes;
		ClassSyntax = classSyntax;
		ClassSymbol = classSymbol;
		RegisterServicesMethod = registerServicesMethod;
		IsModule = isModule;

		IndexManager = new IndexManager();
		CodeBuilder = new CodeBuilder();

		_constructorParameters = new Dictionary<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>>(SymbolEqualityComparer.Default);
		_serviceDescriptors = new List<ServiceDescriptor>();
		_enumerableTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
		_serviceDescriptorCache = new ServiceDescriptorCache();
		
		_dependencyGraph = new DependencyGraph(SymbolEqualityComparer.Default);

		Actions = new List<Action<GenerationContext>>();
		GetServicesActions = new List<Action<GenerationContext>>();

		GetServicesProcessedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
	}

	public void AddServiceDescriptor(ServiceDescriptor serviceDescriptor)
	{
		INamedTypeSymbol service = serviceDescriptor.ServiceType;
		INamedTypeSymbol? implementation = serviceDescriptor.ImplementationType;
		INamedTypeSymbol concreteType = implementation ?? service;

		_serviceDescriptors.Add(serviceDescriptor);
		_serviceDescriptorCache.Add(serviceDescriptor);

		if (serviceDescriptor.Lifetime == Lifetime.Singleton)
		{
			HasSingleton = true;
		}
		else if (serviceDescriptor.Lifetime == Lifetime.Scoped)
		{
			HasScoped = true;
		}
		else if (serviceDescriptor.Lifetime == Lifetime.Transient)
		{
			HasTransient = true;
		}
		
		if (concreteType.IsAsyncDisposable(KnownTypes))
		{
			HasAsyncDisposable = true;

			if (serviceDescriptor.Lifetime == Lifetime.Transient)
			{
				HasTransientAsyncDisposable = true;
			}
			else if (serviceDescriptor.Lifetime == Lifetime.Scoped)
			{
				HasScopedAsyncDisposable = true;
			}
		}
		else if (concreteType.IsDisposable(KnownTypes))
		{
			HasDisposable = true;

			if (serviceDescriptor.Lifetime == Lifetime.Transient)
			{
				HasTransientDisposable = true;
			}
			else if (serviceDescriptor.Lifetime == Lifetime.Scoped)
			{
				HasScopedDisposable = true;
			}
		}
	}

	public ServiceDescriptor GetEffectiveServiceDescriptorForType(ITypeSymbol serviceType, Lifetime lifetime)
	{
		IReadOnlyList<ServiceDescriptor> descriptors = _serviceDescriptorCache.GetDescriptorsForService(serviceType);
		ServiceDescriptor serviceDescriptor = descriptors.Last(x => x.Lifetime == lifetime);

		return serviceDescriptor;
	}
	
	public string GetFactoryInvocation(SyntaxNode factory, string serviceProviderParameter)
	{
		FactoryRewriter rewriter = new(this);
		factory = rewriter.Visit(factory);

		if (factory is not AnonymousFunctionExpressionSyntax anonymousFunction)
		{
			return $"return {factory}({serviceProviderParameter});";
		}
		
		if (anonymousFunction.Block != null)
		{
			return anonymousFunction.Block.Statements.ToString();
		}

		return $"return {anonymousFunction.ExpressionBody};";
	}

	public IReadOnlyList<ITypeSymbol>? GetConstructorParameters(INamedTypeSymbol type)
	{
		_constructorParameters.TryGetValue(type, out IReadOnlyList<ITypeSymbol>? result);
		return result;
	}

	public IReadOnlyList<ServiceDescriptor> GetEnumerableDescriptors(ITypeSymbol type)
	{
		return _serviceDescriptorCache.GetEnumerableDescriptors(type);
	}

	public Lifetime? GetEnumerableLifetime(ITypeSymbol type)
	{
		Lifetime? lifetime = null;

		foreach (ServiceDescriptor descriptor in GetEnumerableDescriptors(type))
		{
			if (lifetime == null || descriptor.Lifetime < lifetime)
			{
				lifetime = descriptor.Lifetime;
			}
		}

		return lifetime;
	}

	public void ComputeDependencyGraph()
	{
		HashSet<INamedTypeSymbol> processedTypes = new(SymbolEqualityComparer.Default);

		foreach (ServiceDescriptor serviceDescriptor in _serviceDescriptors)
		{
			INamedTypeSymbol typeSymbol = serviceDescriptor.ImplementationType ?? serviceDescriptor.ServiceType;

			if (!processedTypes.Add(typeSymbol))
			{
				continue;
			}

			if (serviceDescriptor.Factory != null)
			{
				_dependencyGraph.AddNode(typeSymbol, Array.Empty<INamedTypeSymbol>());
				continue;
			}
			
			ConstructorSelectionResult selectionResult = SelectConstructor(serviceDescriptor, typeSymbol);

			// Don't report diagnostics on modules. Modules may not have all the necessary
			// services registered yet.
			if (selectionResult.Error != ConstructorSelectionError.None && !IsModule)
			{
				selectionResult.ReportDiagnostic(typeSymbol);
				continue;
			}

			ImmutableArray<IParameterSymbol> constructorParameters =
				selectionResult.SelectedConstructor?.Parameters ?? ImmutableArray<IParameterSymbol>.Empty;

			List<ITypeSymbol> parameters = new();
			List<INamedTypeSymbol> concreteDependencies = new();

			foreach (IParameterSymbol parameter in constructorParameters)
			{
				bool typeIsRegistered = _serviceDescriptorCache.TryGet(
					parameter.Type,
					out IReadOnlyList<ServiceDescriptor>? serviceDescriptors);

				if (typeIsRegistered)
				{
					// If the requested type is registered directly, the last registration will determine the dependency.
					ServiceDescriptor dependency = serviceDescriptors!.Last();
					INamedTypeSymbol dependencyType = dependency.ImplementationType ?? dependency.ServiceType;

					parameters.Add(parameter.Type);
					concreteDependencies.Add(dependencyType);

					continue;
				}

				INamedTypeSymbol? enumerableTypeArgument = parameter.Type.GetEnumerableType(KnownTypes);

				if (enumerableTypeArgument != null)
				{
					// If the requested type is IEnumerable, we will use all service descriptors.
					foreach (ServiceDescriptor descriptor in _serviceDescriptorCache.GetEnumerableDescriptors(enumerableTypeArgument))
					{
						INamedTypeSymbol dependencyType = descriptor.ImplementationType ?? descriptor.ServiceType;
						concreteDependencies.Add(dependencyType);
					}

					parameters.Add(parameter.Type);
					_enumerableTypes.Add(enumerableTypeArgument);

					continue;
				}
				
				if (parameter.IsOptional)
				{
					// Don't try to realize the services that are optional and not registered.
					continue;
				}

				throw new InvalidOperationException("Should not reach here");
			}

			_constructorParameters[typeSymbol] = parameters;
			_dependencyGraph.AddNode(typeSymbol, concreteDependencies);
		}
	}

	private ConstructorSelectionResult SelectConstructor(ServiceDescriptor serviceDescriptor, INamedTypeSymbol symbol)
	{
		ConstructorSelectionResult result = new(this);

		if (serviceDescriptor.RedirectToThis)
		{
			return result;
		}

		// No need to search for a constructor on non constructible types.
		if (symbol.IsAbstract || symbol.IsStatic || symbol.TypeKind == TypeKind.Interface)
		{
			result.SetError(ConstructorSelectionError.NotConstructible);
			return result;
		}
		
		foreach (IMethodSymbol constructor in symbol.Constructors)
		{
			if (constructor.DeclaredAccessibility != Accessibility.Public)
			{
				// This is an error only if there is no selected constructor yet.
				if (result.SelectedConstructor == null)
				{
					// Setting the error here will not override the error if its value is greater
					// than this error, e.g. missing parameters.
					result.SetError(ConstructorSelectionError.NoPublicConstructor);
				}

				continue;
			}

			int parameterCount = constructor.Parameters.Length;
			int selectedParameterCount = result.SelectedConstructor?.Parameters.Length ?? -1;

			// If the selected constructor has more parameters than this one, we can ignore current
			// constructor.
			if (parameterCount < selectedParameterCount)
			{
				continue;
			}

			bool currentConstructorHasMissingParameters = false;

			foreach (IParameterSymbol parameter in constructor.Parameters)
			{
				bool hasParameter = _serviceDescriptorCache.Contains(parameter.Type);

				if (hasParameter || parameter.IsOptional || parameter.Type.IsEnumerable(KnownTypes))
				{
					continue;
				}
				
				// This constructor is better than the currently selected one, if there is one,
				// but misses some parameters. In this case we may report a warning.
				// Otherwise, if there is no selected constructor, missing parameters will be
				// reported as an error.
				result.AddMissingParameter(parameter);
				currentConstructorHasMissingParameters = true;
			}

			if (currentConstructorHasMissingParameters)
			{
				// This constructor was the best candidate so far, but it misses some parameters.
				result.SetBestCandidateMissingParameters(constructor);

				// This is an error only if there is no selected constructor.
				if (result.SelectedConstructor == null)
				{
					result.SetError(ConstructorSelectionError.MissingParameters);
				}

				continue;
			}

			if (selectedParameterCount == parameterCount)
			{
				// There are more than one constructors that can be satisfied with the same parameter
				// count.
				result.AddAmibiguousConstructor(constructor);
				result.AddAmibiguousConstructor(result.SelectedConstructor!);
				
				result.SetError(ConstructorSelectionError.AmbiguousMatch);

				continue;
			}

			// This is the current best constructor.
			result.SetError(ConstructorSelectionError.None);
			result.SetSelectedConstructor(constructor);
		}

		return result;
	}

	public IEnumerable<ServiceDescriptor> GetDependencySortedDescriptors()
	{
		return _dependencyGraph.Graph
			.SourceFirstTopologicalSort()
			.Reverse()
			.SelectMany(_serviceDescriptorCache.GetDescriptorsForImplementation);
	}

	public bool IsDependencyGraphValid()
	{
		if (_dependencyGraph.IsDirectedAcyclicGraph())
		{
			return true;
		}

		Dictionary<INamedTypeSymbol, int> components = new(SymbolEqualityComparer.Default);

		StronglyConnectedComponentsAlgorithm<INamedTypeSymbol, IEdge<INamedTypeSymbol>> componentsAlgorithm = new(
			_dependencyGraph.Graph,
			components);

		componentsAlgorithm.Compute();

		List<BidirectionalGraph<INamedTypeSymbol, IEdge<INamedTypeSymbol>>> cyclicGraphs = componentsAlgorithm.Graphs
			.Where(x => x.VertexCount > 1 || (x.VertexCount == 1 && x.EdgeCount == 1))
			.ToList();

		List<string> recursiveChains = cyclicGraphs
			.Select(DependencyGraph.GraphToString)
			.ToList();

		SourceProductionContext.ReportDiagnostic(Diagnostic.Create(
			Diagnostics.RecursiveDependency,
			Location.None,
			string.Join("\n", recursiveChains.Select(x => x.Trim()))));

		return false;
	}
	
	public void AddNewLineIfNecessary()
	{
		if (!AddNewLine)
		{
			return;
		}

		CodeBuilder.AppendLine();
		AddNewLine = false;
	}

	public void Reset()
	{
		Actions.Clear();
		GetServicesActions.Clear();
		GetServicesProcessedTypes.Clear();
		IndexManager.Clear();
		IsScopeClass = true;
		AddNewLine = false;
	}

	private class ConstructorSelectionResult
	{
		private readonly GenerationContext _generationContext;

		private List<IMethodSymbol>? _ambiguousConstructors;
		private List<IParameterSymbol>? _missingParameters;

		public ConstructorSelectionError Error { get; private set; }
		public IMethodSymbol? SelectedConstructor { get; private set; }

		private IMethodSymbol? BestCandidateMissingParameters { get; set; }

		private IReadOnlyList<IMethodSymbol> AmbiguousConstructors =>
			_ambiguousConstructors ?? (IReadOnlyList<IMethodSymbol>)Array.Empty<IMethodSymbol>();

		private IReadOnlyList<IParameterSymbol> MissingParameters =>
			_missingParameters ?? (IReadOnlyList<IParameterSymbol>)Array.Empty<IParameterSymbol>();

		public ConstructorSelectionResult(GenerationContext generationContext)
		{
			_generationContext = generationContext;
		}
		
		public void SetError(ConstructorSelectionError error)
		{
			if (error == ConstructorSelectionError.None)
			{
				Error = ConstructorSelectionError.None;
				_missingParameters = null;
				_ambiguousConstructors = null;

				return;
			}

			if (error > Error)
			{
				Error = error;
			}
		}

		public void AddAmibiguousConstructor(IMethodSymbol constructor)
		{
			_ambiguousConstructors ??= new List<IMethodSymbol>();
			_ambiguousConstructors.Add(constructor);
		}

		public void AddMissingParameter(IParameterSymbol parameter)
		{
			_missingParameters ??= new List<IParameterSymbol>();
			_missingParameters.Add(parameter);
		}

		public void SetSelectedConstructor(IMethodSymbol? constructor)
		{
			SelectedConstructor = constructor;
		}

		public void SetBestCandidateMissingParameters(IMethodSymbol? constructor)
		{
			BestCandidateMissingParameters = constructor;
		}

		public void ReportDiagnostic(INamedTypeSymbol symbol)
		{
			Location? location =
				(symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ClassDeclarationSyntax)?.Identifier.GetLocation();

			Diagnostic diagnostic = Error switch
			{
				ConstructorSelectionError.NotConstructible =>
					Diagnostic.Create(Diagnostics.ClassNotConstructible, location),
				ConstructorSelectionError.NoPublicConstructor =>
					Diagnostic.Create(Diagnostics.NoPublicConstructors, location),
				ConstructorSelectionError.MissingParameters =>
					Diagnostic.Create(
						Diagnostics.MissingDependencies,
						(BestCandidateMissingParameters?.DeclaringSyntaxReferences
							.FirstOrDefault()
							?.GetSyntax() as ConstructorDeclarationSyntax)
							?.Identifier
							.GetLocation(),
						string.Join(", ", MissingParameters.Select(x => x.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)))),
				ConstructorSelectionError.AmbiguousMatch =>
					Diagnostic.Create(
						Diagnostics.AmbiguousConstructors,
						(AmbiguousConstructors.First().DeclaringSyntaxReferences
							.FirstOrDefault()
							?.GetSyntax() as ConstructorDeclarationSyntax)
						?.Identifier
						.GetLocation(),
						string.Join(", ", AmbiguousConstructors.Select(x => x.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)))),
				_ => throw new ArgumentOutOfRangeException(nameof(Error)),
			};
			_generationContext.SourceProductionContext.ReportDiagnostic(diagnostic);
		}
	}

	private enum ConstructorSelectionError
	{
		None,
		NoPublicConstructor,
		MissingParameters,
		AmbiguousMatch,
		NotConstructible
	}
}