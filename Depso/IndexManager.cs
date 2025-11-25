using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Depso;

public class IndexManager
{
	private readonly Dictionary<ITypeSymbol, int> _fieldIndexes;
	private readonly Dictionary<ITypeSymbol, int> _createMethodIndexes;
	private readonly Dictionary<ITypeSymbol, int> _createMethodIndexesNonFactory;
	
	public IndexManager()
	{
		_fieldIndexes = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
		_createMethodIndexes = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
		_createMethodIndexesNonFactory = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
	}
	
	public void Add(ServiceDescriptor serviceDescriptor)
	{
		// Singleton and scoped services need a field index, and they also increment the create method indexes.
		// Transient services only increment the create method indexes as they don't have fields.

		// Create method index is determined by the service type as it will be the return type of the method.
		// Field index is determined by the implementation type (or service type if implementation type is null)
		// as it will be the type of the field.

		// Field index of the also register as types is the same as the field index determined above
		// if the lifetime is singleton or scoped. The create method index is not defined for those since
		// the creation will be handled by the concrete type.
		// 
		// For transients field index is not defined as they don't have fields. Create method index will be the
		// same as the service type's.

		bool isTransient = serviceDescriptor.Lifetime == Lifetime.Transient;

		INamedTypeSymbol serviceType = serviceDescriptor.ServiceType;
		INamedTypeSymbol? implementationType = serviceDescriptor.ImplementationType;

		int fieldIndex = -1;
		int createMethodIndex = AddCreateMethodIndex(implementationType ?? serviceType, serviceDescriptor.Factory != null);

		if (!isTransient)
		{
			fieldIndex = AddFieldIndex(implementationType ?? serviceType);
		}

		serviceDescriptor.SetIndexes(new Index(fieldIndex, createMethodIndex));
	}
	
	private int AddCreateMethodIndex(ITypeSymbol type, bool isFactory)
	{
		type = NormalizeType(type);

		if (!isFactory && _createMethodIndexesNonFactory.TryGetValue(type, out int indexNonFactory))
		{
			// Use the same index if the create method is not a factory, i.e. we create the service
			// via generated code. This ensures that the same method will be used by multiple non-factory
			// registrations.
			return indexNonFactory;
		}

		if (!_createMethodIndexes.TryGetValue(type, out int index))
		{
			index = -1;
		}

		index++;

		_createMethodIndexes[type] = index;

		if (!_createMethodIndexesNonFactory.ContainsKey(type))
		{
			_createMethodIndexesNonFactory[type] = index;
		}

		return index;
	}

	private int AddFieldIndex(ITypeSymbol type)
	{
		type = NormalizeType(type);

		if (!_fieldIndexes.TryGetValue(type, out int index))
		{
			index = -1;
		}

		index++;

		_fieldIndexes[type] = index;

		return index;
	}

	private ITypeSymbol NormalizeType(ITypeSymbol type)
	{
		if (type is INamedTypeSymbol { IsGenericType: true } namedType)
		{
			return namedType.ConstructedFrom;
		}

		return type;
	}
	
	public void Clear()
	{
		_createMethodIndexes.Clear();
		_fieldIndexes.Clear();
	}
}