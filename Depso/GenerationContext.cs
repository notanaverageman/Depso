using System;
using System.Collections.Generic;
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

	public ServiceDescriptor GetEffectiveServiceDescriptorForType(ITypeSymbol serviceType)
	{
		IReadOnlyList<ServiceDescriptor> descriptors = _serviceDescriptorCache.GetDescriptorsForService(serviceType);
		ServiceDescriptor serviceDescriptor = descriptors.Last();

		return serviceDescriptor;
	}
	
	public string GetFactoryInvocation(
		SyntaxNode factory,
		string serviceProviderParameter,
		bool replaceServiceProviderToThis)
	{
		FactoryRewriter rewriter = new(Compilation, IsScopeClass, replaceServiceProviderToThis);
		factory = rewriter.Visit(factory)!;

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

	public IReadOnlyList<ITypeSymbol> GetConstructorParameters(INamedTypeSymbol type)
	{
		return _constructorParameters[type];
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

			// TODO: Check that this symbol is a concrete type.

			IMethodSymbol? constructor = SelectConstructor(typeSymbol);

			if (constructor == null)
			{
				// TODO: Diagnostic
				throw new ArgumentException("Constructor null!");
			}

			List<ITypeSymbol> parameters = new();
			List<INamedTypeSymbol> concreteDependencies = new();

			foreach (IParameterSymbol parameter in constructor.Parameters)
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

	private IMethodSymbol? SelectConstructor(INamedTypeSymbol symbol)
	{
		IMethodSymbol? bestCandidate = null;

		foreach (IMethodSymbol constructor in symbol.Constructors)
		{
			// TODO: It can be internal if the constructor is accessible from this assembly.
			if (constructor.DeclaredAccessibility != Accessibility.Public)
			{
				continue;
			}

			int parameterCount = constructor.Parameters.Length;
			int bestParameterCount = bestCandidate?.Parameters.Length ?? -1;

			if (parameterCount < bestParameterCount)
			{
				continue;
			}

			bool canUse = true;

			foreach (IParameterSymbol parameter in constructor.Parameters)
			{
				bool hasParameter = _serviceDescriptorCache.Contains(parameter.Type);

				if (hasParameter || parameter.IsOptional || parameter.Type.IsEnumerable(KnownTypes))
				{
					continue;
				}

				canUse = false;
				break;
			}

			if (canUse && bestParameterCount == parameterCount)
			{
				// TODO: Report ambiguous match.
			}

			if (canUse)
			{
				bestCandidate = constructor;
			}
		}

		return bestCandidate;
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
}