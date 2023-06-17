using System.Collections.Generic;
using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class TransientGetServicesGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		IReadOnlyList<ServiceDescriptor> serviceDescriptors = generationContext.ServiceDescriptors;
		HashSet<ITypeSymbol> processedTypes = generationContext.GetServicesProcessedTypes;
		IndexManager indexManager = generationContext.IndexManager;

		foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
		{
			if (serviceDescriptor.Lifetime != Lifetime.Transient)
			{
				continue;
			}

			indexManager.Add(serviceDescriptor);
		}

		foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
		{
			if (serviceDescriptor.Lifetime != Lifetime.Transient)
			{
				continue;
			}

			INamedTypeSymbol serviceType = serviceDescriptor.ServiceType;

			if (processedTypes.Add(serviceType))
			{
				generationContext.GetServicesActions.Add(x => ProcessType(x, serviceType));
			}

			if (serviceDescriptor.AlsoRegisterAs == null)
			{
				continue;
			}

			foreach (INamedTypeSymbol alsoRegisterAs in serviceDescriptor.AlsoRegisterAs)
			{
				if (processedTypes.Add(alsoRegisterAs))
				{
					generationContext.GetServicesActions.Add(x => ProcessType(x, alsoRegisterAs));
				}
			}
		}
	}

	private static void ProcessType(
		GenerationContext generationContext,
		INamedTypeSymbol serviceType)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		string typeName = serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		ServiceDescriptor serviceDescriptor = generationContext.GetEffectiveServiceDescriptorForType(
			serviceType,
			Lifetime.Transient);

		string methodName = serviceDescriptor.Factory == null
			? serviceDescriptor.GetCreateMethodName()
			: serviceDescriptor.GetFactoryMethodName();

		string rootPrefix = generationContext.IsScopeClass
			? "_root."
			: "";

		codeBuilder.AppendLine(serviceDescriptor.ConcreteType.IsDisposableOrAsyncDisposable(generationContext.KnownTypes)
			? $"if (serviceType == typeof({typeName})) return {methodName}AddDisposable();"
			: $"if (serviceType == typeof({typeName})) return {rootPrefix}{methodName}();");

		generationContext.AddNewLine = true;
	}
}