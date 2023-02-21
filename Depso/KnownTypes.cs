// ReSharper disable InconsistentNaming
using Microsoft.CodeAnalysis;
using System;

namespace Depso;

public class KnownTypes
{
	public record KnownAttributes(
		INamedTypeSymbol Singleton,
		INamedTypeSymbol Scoped,
		INamedTypeSymbol Transient);
	
	public INamedTypeSymbol Func { get; }
	public INamedTypeSymbol IEnumerable { get; }

	public INamedTypeSymbol IDisposable { get; }
	public INamedTypeSymbol? IAsyncDisposable { get; }

	public INamedTypeSymbol IServiceProvider { get; }
	public INamedTypeSymbol? IServiceScope { get; }
	public INamedTypeSymbol? IServiceScopeFactory { get; }
	public INamedTypeSymbol? IServiceProviderIsService { get; }
	
	public KnownTypes(Compilation compilation)
	{
		Func = GetRequiredType(compilation, $"{Constants.FuncMetadataName}`2").ConstructUnboundGenericType();
		IEnumerable = GetRequiredType(compilation, $"{Constants.IEnumerableMetadataName}`1");

		IDisposable = GetRequiredType(compilation, Constants.IDisposableMetadataName);
	    IAsyncDisposable = compilation.GetTypeByMetadataName(Constants.IAsyncDisposableMetadataName);

	    IServiceProvider = GetRequiredType(compilation, Constants.IServiceProviderMetadataName);
	    IServiceScope = compilation.GetTypeByMetadataName(Constants.IServiceScopeMetadataName);
	    IServiceScopeFactory = compilation.GetTypeByMetadataName(Constants.IServiceScopeFactoryMetadataName);
	    IServiceProviderIsService = compilation.GetTypeByMetadataName(Constants.IServiceProviderIsServiceMetadataName);
	}
	
	private static INamedTypeSymbol GetRequiredType(
		Compilation compilation,
		string fullyQualifiedMetadataName)
	{
		INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);

		if (symbol == null)
		{
			// TODO: Diagnostic.
			throw new ArgumentException($"Type with metadata '{fullyQualifiedMetadataName}' not found.");
		}

		return symbol;
	}
}