using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using QuickGraph;
using QuickGraph.Algorithms.Search;
using QuickGraph.Collections;

namespace Depso;

public class DependencyGraph
{
	public BidirectionalGraph<INamedTypeSymbol, IEdge<INamedTypeSymbol>> Graph { get; }

	public DependencyGraph(IEqualityComparer<INamedTypeSymbol>? equalityComparer = null)
	{
		Func<int, IVertexEdgeDictionary<INamedTypeSymbol, IEdge<INamedTypeSymbol>>>? dictionaryFactory = null;

		if (equalityComparer != null)
		{
			dictionaryFactory = capacity => new ComparableVertexEdgeDictionary(capacity, equalityComparer);
		}

		Graph = new BidirectionalGraph<INamedTypeSymbol, IEdge<INamedTypeSymbol>>(
			allowParallelEdges: false,
			capacity: 0,
			vertexEdgesDictionaryFactory: dictionaryFactory);
	}

	public void AddNode(
		INamedTypeSymbol node,
		IReadOnlyCollection<INamedTypeSymbol> dependsOnConcrete)
	{
		Graph.AddVertex(node);

		Graph.AddVertexRange(dependsOnConcrete);
		Graph.AddEdgeRange(dependsOnConcrete.Select(x => (IEdge<INamedTypeSymbol>)new SEquatableEdge<INamedTypeSymbol>(x, node)));
	}

	public bool Contains(INamedTypeSymbol node)
	{
		return Graph.ContainsVertex(node);
	}
	
	public bool IsDirectedAcyclicGraph()
	{
		bool isDirectedAcyclicGraph = true;

		DepthFirstSearchAlgorithm<INamedTypeSymbol, IEdge<INamedTypeSymbol>> dfs = new(
			Graph,
			new Dictionary<INamedTypeSymbol, GraphColor>(SymbolEqualityComparer.Default));

		void OnDfsOnBackEdge(IEdge<INamedTypeSymbol> _) => isDirectedAcyclicGraph = false;

		try
		{
			dfs.BackEdge += OnDfsOnBackEdge;
			dfs.Compute();
		}
		finally
		{
			dfs.BackEdge -= OnDfsOnBackEdge;
		}

		return isDirectedAcyclicGraph;
	}

	public static string GraphToString(BidirectionalGraph<INamedTypeSymbol, IEdge<INamedTypeSymbol>> graph)
	{
		HashSet<INamedTypeSymbol> visitedTypes = new(SymbolEqualityComparer.Default);
		INamedTypeSymbol? recursiveType = null;

		foreach (IEdge<INamedTypeSymbol> edge in graph.Edges)
		{
			visitedTypes.Add(edge.Source);

			if (visitedTypes.Contains(edge.Target))
			{
				recursiveType = edge.Target;
				break;
			}
		}

		if (recursiveType == null)
		{
			throw new InvalidOperationException("Internal error: Starting type not found on recursive graph.");
		}

		DepthFirstSearchAlgorithm<INamedTypeSymbol, IEdge<INamedTypeSymbol>> dfs = new(
			graph,
			new Dictionary<INamedTypeSymbol, GraphColor>(SymbolEqualityComparer.Default));

		int indentation = 0;
		StringBuilder stringBuilder = new();
		stringBuilder.AppendLine(recursiveType.ToDisplayString());

		dfs.DiscoverVertex += type =>
		{
			if (!type.SymbolEquals(recursiveType))
			{
				// ReSharper disable once AccessToModifiedClosure
				indentation++;
				stringBuilder.Append(' ', indentation * 2);
				stringBuilder.AppendLine($"-> {type.ToDisplayString()}");
			}
		};

		dfs.Compute(recursiveType);

		indentation++;
		stringBuilder.Append(' ', indentation * 2);
		stringBuilder.AppendLine($"-> {recursiveType.ToDisplayString()}");
		stringBuilder.AppendLine();

		return stringBuilder.ToString();
	}

	private class ComparableVertexEdgeDictionary
		:
		Dictionary<INamedTypeSymbol, IEdgeList<INamedTypeSymbol, IEdge<INamedTypeSymbol>>>,
		IVertexEdgeDictionary<INamedTypeSymbol, IEdge<INamedTypeSymbol>>
	{
		public ComparableVertexEdgeDictionary(int capacity, IEqualityComparer<INamedTypeSymbol> comparer) : base(capacity, comparer)
		{
		}

		private ComparableVertexEdgeDictionary Clone()
		{
			ComparableVertexEdgeDictionary clone = new(Count, Comparer);
			
			foreach (KeyValuePair<INamedTypeSymbol, IEdgeList<INamedTypeSymbol, IEdge<INamedTypeSymbol>>> kv in this)
			{
				clone.Add(kv.Key, kv.Value.Clone());
			}

			return clone;
		}

		IVertexEdgeDictionary<INamedTypeSymbol, IEdge<INamedTypeSymbol>> IVertexEdgeDictionary<INamedTypeSymbol, IEdge<INamedTypeSymbol>>.Clone()
		{
			return Clone();
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}