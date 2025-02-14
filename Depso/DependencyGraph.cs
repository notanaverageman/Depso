using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using QuikGraph;
using QuikGraph.Algorithms.Search;

namespace Depso;

public class DependencyGraph
{
	public BidirectionalGraph<INamedTypeSymbol, IEdge<INamedTypeSymbol>> Graph { get; }

	public DependencyGraph()
	{
		Graph = new BidirectionalGraph<INamedTypeSymbol, IEdge<INamedTypeSymbol>>(allowParallelEdges: false);
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
}