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
	private readonly Dictionary<ITypeSymbol, Index> _indexes;

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

		_indexes = new Dictionary<ITypeSymbol, Index>(SymbolEqualityComparer.Default);
	}

	public void SetIndexes(Index index)
	{
		if (ImplementationType != null)
		{
			_indexes[ImplementationType] = index;
			_indexes[ServiceType] = index;
		}
		else
		{
			_indexes[ServiceType] = index;
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
				_indexes[alsoRegisterAs] = index;
			}
			else
			{
				_indexes[alsoRegisterAs] = index with { CreateMethod = -1 };
			}
		}
	}
	
	public string GetFieldName()
	{
		ITypeSymbol symbol = ConcreteType;

		string genericSuffix = "";

		if (symbol is INamedTypeSymbol { IsGenericType: true } namedType)
		{
			genericSuffix = namedType.Arity.ToString();
		}
		
		string result = $"_{symbol.Name.ToCamelCase()}{genericSuffix}";
		
		int index = Lifetime == Lifetime.Transient
			? _indexes[symbol].CreateMethod
			: _indexes[symbol].Field;

		return $"{result}_{index}";
	}

	public string GetCreateMethodName()
	{
		if (Factory != null)
		{
			throw new InvalidOperationException("Trying to create Create method when Factory is not null.");
		}

		ITypeSymbol symbol = ConcreteType;

		string genericSuffix = "";

		if (symbol is INamedTypeSymbol { IsGenericType: true } namedType)
		{
			genericSuffix = namedType.Arity.ToString();
		}

		return $"Create{symbol.Name}{genericSuffix}_{_indexes[symbol].CreateMethod}";
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