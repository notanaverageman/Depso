using System.Linq;
using Microsoft.CodeAnalysis;

namespace Depso;

public static class SymbolExtensions
{
	public static SymbolInfo GetSymbolInfo(this Compilation compilation, SyntaxNode syntaxNode)
	{
		return compilation.ContainsSyntaxTree(syntaxNode.SyntaxTree)
			? compilation
				.GetSemanticModel(syntaxNode.SyntaxTree)
				.GetSymbolInfo(syntaxNode)
			: default;
	}

	public static TypeInfo GetTypeInfo(this Compilation compilation, SyntaxNode syntaxNode)
	{
		return compilation.ContainsSyntaxTree(syntaxNode.SyntaxTree)
			? compilation
				.GetSemanticModel(syntaxNode.SyntaxTree)
				.GetTypeInfo(syntaxNode)
			: default;
	}

	public static ISymbol? GetDeclaredSymbol(this Compilation compilation, SyntaxNode syntaxNode)
	{
		return compilation.ContainsSyntaxTree(syntaxNode.SyntaxTree)
			? compilation
				.GetSemanticModel(syntaxNode.SyntaxTree)
				.GetDeclaredSymbol(syntaxNode)
			: null;
	}

	public static bool SymbolEquals(this ISymbol? symbol, ISymbol? other)
	{
		return SymbolEqualityComparer.Default.Equals(symbol, other);
	}

	public static bool IsDisposable(this ITypeSymbol symbol, KnownTypes knownTypes)
	{
		return symbol.AllInterfaces.Contains(knownTypes.IDisposable);
	}

	public static bool IsAsyncDisposable(this ITypeSymbol symbol, KnownTypes knownTypes)
	{
		return knownTypes.IAsyncDisposable != null && symbol.AllInterfaces.Contains(knownTypes.IAsyncDisposable);
	}

	public static bool IsDisposableOrAsyncDisposable(this ITypeSymbol symbol, KnownTypes knownTypes)
	{
		return symbol.IsDisposable(knownTypes) || symbol.IsAsyncDisposable(knownTypes);
	}

	public static bool IsEnumerable(this ITypeSymbol symbol, KnownTypes knownTypes)
	{
		return symbol.GetEnumerableType(knownTypes) != null;
	}

	public static INamedTypeSymbol? GetEnumerableType(this ITypeSymbol symbol, KnownTypes knownTypes)
	{
		if (symbol is not INamedTypeSymbol { IsGenericType: true } e)
		{
			return null;
		}

		if (!e.ConstructedFrom.SymbolEquals(knownTypes.IEnumerable))
		{
			return null;
		}

		return e.TypeArguments.First() as INamedTypeSymbol;
	}

	public static bool IsRegisterServicesMethod(this IMethodSymbol methodSymbol, bool isStatic)
	{
		if (!methodSymbol.ReturnsVoid)
		{
			return false;
		}

		if (methodSymbol.IsAbstract || methodSymbol.IsGenericMethod || methodSymbol.IsVirtual)
		{
			return false;
		}

		if (isStatic && !methodSymbol.IsStatic)
		{
			return false;
		}

		if (!isStatic && methodSymbol.IsStatic)
		{
			return false;
		}

		if (methodSymbol.Name != Constants.RegisterServicesMethodName)
		{
			return false;
		}

		if (methodSymbol.DeclaredAccessibility != Accessibility.Private)
		{
			return false;
		}

		if (methodSymbol.Parameters.Length > 0)
		{
			return false;
		}

		if (methodSymbol.DeclaringSyntaxReferences.Length != 1)
		{
			return false;
		}

		return true;
	}
}