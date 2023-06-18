using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Depso;

public class ServiceDescriptorCache
{
	private readonly Dictionary<ITypeSymbol, List<ServiceDescriptor>> _serviceCache;
	private readonly Dictionary<ITypeSymbol, List<ServiceDescriptor>> _implementationCache;
	
	public ServiceDescriptorCache()
	{
		_serviceCache = new Dictionary<ITypeSymbol, List<ServiceDescriptor>>(SymbolEqualityComparer.Default);
		_implementationCache = new Dictionary<ITypeSymbol, List<ServiceDescriptor>>(SymbolEqualityComparer.Default);
	}

	public void Add(ServiceDescriptor serviceDescriptor)
	{
		INamedTypeSymbol serviceType = serviceDescriptor.ServiceType;

		if (!_serviceCache.TryGetValue(serviceType, out List<ServiceDescriptor>? descriptors))
		{
			descriptors = new List<ServiceDescriptor>();
			_serviceCache[serviceType] = descriptors;
		}

		descriptors.Add(serviceDescriptor);

		if (serviceDescriptor.AlsoRegisterAs != null)
		{
			foreach (INamedTypeSymbol alsoRegisterAs in serviceDescriptor.AlsoRegisterAs)
			{
				if (!_serviceCache.TryGetValue(alsoRegisterAs, out descriptors))
				{
					descriptors = new List<ServiceDescriptor>();
					_serviceCache[alsoRegisterAs] = descriptors;
				}

				descriptors.Add(serviceDescriptor);
			}
		}

		INamedTypeSymbol implementationType = serviceDescriptor.ImplementationType ?? serviceType;

		if (!_implementationCache.TryGetValue(implementationType, out List<ServiceDescriptor>? implementationDescriptors))
		{
			implementationDescriptors = new List<ServiceDescriptor>();
			_implementationCache[implementationType] = implementationDescriptors;
		}

		implementationDescriptors.Add(serviceDescriptor);
	}
	
	public IReadOnlyList<ServiceDescriptor> GetDescriptorsForService(ITypeSymbol type)
	{
		if (_serviceCache.TryGetValue(type, out List<ServiceDescriptor>? descriptors))
		{
			return descriptors;
		}

		return Array.Empty<ServiceDescriptor>();
	}

	public IReadOnlyList<ServiceDescriptor> GetDescriptorsForImplementation(ITypeSymbol type)
	{
		return _implementationCache[type];
	}

	public bool TryGet(ITypeSymbol type, out IReadOnlyList<ServiceDescriptor>? serviceDescriptors)
	{
		bool result = _serviceCache.TryGetValue(type, out List<ServiceDescriptor>? s);
		serviceDescriptors = s;

		return result;
	}

	public IReadOnlyList<ServiceDescriptor> GetEnumerableDescriptors(ITypeSymbol type)
	{
		_serviceCache.TryGetValue(type, out List<ServiceDescriptor>? s);
		return (IReadOnlyList<ServiceDescriptor>?)s ?? Array.Empty<ServiceDescriptor>();
	}

	public bool Contains(ITypeSymbol type)
	{
		return _serviceCache.ContainsKey(type);
	}
}