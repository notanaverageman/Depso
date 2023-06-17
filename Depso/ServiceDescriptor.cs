using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Depso;

public record struct Index(int Field, int CreateMethod);

[DebuggerDisplay("{GetDebuggerDisplay()}")]
public class ServiceDescriptor
{
	private readonly Dictionary<ITypeSymbol, Index> _indices;

	public Lifetime Lifetime { get; init; }
	public INamedTypeSymbol ServiceType { get; init; }
	public INamedTypeSymbol? ImplementationType { get; init; }
	public IReadOnlyList<INamedTypeSymbol>? AlsoRegisterAs { get; init; }
	public SyntaxNode? Factory { get; init; }
	public bool RedirectToThis { get; init; }

	public INamedTypeSymbol ConcreteType => ImplementationType ?? ServiceType;

	public ServiceDescriptor(
		Lifetime lifetime,
		INamedTypeSymbol serviceType,
		INamedTypeSymbol? implementationType = null,
		SyntaxNode? factory = null,
		IReadOnlyList<INamedTypeSymbol>? alsoRegisterAs = null)
	{
		Lifetime = lifetime;
		ServiceType = serviceType;
		ImplementationType = implementationType;
		Factory = factory;
		AlsoRegisterAs = alsoRegisterAs;

		_indices = new Dictionary<ITypeSymbol, Index>(SymbolEqualityComparer.Default);
	}

	public void SetIndexes(Index index)
	{
		if (ImplementationType != null)
		{
			_indices[ImplementationType] = index;
			_indices[ServiceType] = index with { CreateMethod = -1 };
		}
		else
		{
			_indices[ServiceType] = index;
		}

		if (AlsoRegisterAs == null)
		{
			return;
		}

		foreach (INamedTypeSymbol alsoRegisterAs in AlsoRegisterAs)
		{
			if (alsoRegisterAs.SymbolEquals(ServiceType) || alsoRegisterAs.SymbolEquals(ImplementationType))
			{
				continue;
			}

			if (Lifetime == Lifetime.Transient)
			{
				_indices[alsoRegisterAs] = index;
			}
			else
			{
				_indices[alsoRegisterAs] = index with { CreateMethod = -1 };
			}
		}
	}
	
	public string GetFieldName()
	{
		ITypeSymbol symbol = ImplementationType ?? ServiceType;

		string genericSuffix = "";

		if (symbol is INamedTypeSymbol { IsGenericType: true } namedType)
		{
			genericSuffix = namedType.Arity.ToString();
		}
		
		string result = $"_{symbol.Name.ToCamelCase()}{genericSuffix}";
		
		int index = Lifetime == Lifetime.Transient
			? _indices[symbol].CreateMethod
			: _indices[symbol].Field;

		return $"{result}_{index}";
	}

	public string GetCreateMethodName()
	{
		if (Factory != null)
		{
			throw new InvalidOperationException("Trying to create createX method when Factory is not null.");
		}
		
		string propertyName = GetFieldName().ToPropertyName();
		return $"Create{propertyName}";
	}

	public string GetFactoryMethodName()
	{
		if (Factory == null)
		{
			throw new InvalidOperationException("Trying to create factory method when Factory is null.");
		}

		string propertyName = GetFieldName().ToPropertyName();
		return $"Factory{propertyName}";
	}

	private string GetDebuggerDisplay()
	{
		string? alsoRegisterAs = AlsoRegisterAs == null
			? null
			: string.Join(", ", AlsoRegisterAs.Select(x => x.ToDisplayString()));

		return $"""
			S: {ServiceType.ToDisplayString()} - I: {ImplementationType?.ToDisplayString()} - F: {Factory} - A: {alsoRegisterAs}
			""";
	}
}