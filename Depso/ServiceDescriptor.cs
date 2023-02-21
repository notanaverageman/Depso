using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Depso;

public record struct Index(int Field, int CreateMethod);

public class ServiceDescriptor
{
	private readonly Dictionary<ITypeSymbol, Index> _indices;

	public Lifetime Lifetime { get; init; }
	public INamedTypeSymbol ServiceType { get; init; }
	public INamedTypeSymbol? ImplementationType { get; init; }
	public IReadOnlyList<INamedTypeSymbol>? AlsoRegisterAs { get; init; }

	public SyntaxNode? Factory { get; init; }
	public string? LambdaFactory { get; private set; }

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

	public void SetLambdaFactory(string lambdaFactory)
	{
		LambdaFactory = lambdaFactory;
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

		ITypeSymbol symbol = ImplementationType ?? ServiceType;
		return $"Create{symbol.Name.ToPascalCase()}";
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
}