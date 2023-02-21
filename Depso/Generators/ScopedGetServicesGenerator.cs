using System.Collections.Generic;
using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class ScopedGetServicesGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		IReadOnlyList<ServiceDescriptor> serviceDescriptors = generationContext.ServiceDescriptors;
		HashSet<ITypeSymbol> processedTypes = generationContext.GetServicesProcessedTypes;

		foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
		{
			if (serviceDescriptor.Lifetime != Lifetime.Scoped)
			{
				continue;
			}

			INamedTypeSymbol serviceType = serviceDescriptor.ServiceType;

			if (processedTypes.Add(serviceType))
			{
				generationContext.GetServicesActions.Add(x => ProcessType(x, serviceDescriptor, serviceType));
			}

			if (serviceDescriptor.AlsoRegisterAs == null)
			{
				continue;
			}

			foreach (INamedTypeSymbol alsoRegisterAs in serviceDescriptor.AlsoRegisterAs)
			{
				if (!processedTypes.Add(alsoRegisterAs))
				{
					continue;
				}
				
				generationContext.GetServicesActions.Add(x => ProcessType(x, serviceDescriptor, alsoRegisterAs));
			}
		}
	}

	protected virtual void ProcessType(
		GenerationContext generationContext,
		ServiceDescriptor serviceDescriptor,
		INamedTypeSymbol serviceType)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		string typeName = serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		string rootScope = Constants.RootScopePropertyName;
		string getService = Constants.GetServiceMethodName;

		codeBuilder.AppendLine($"if (serviceType == typeof({typeName})) return {rootScope}.{getService}(serviceType);");
		
		generationContext.AddNewLine = true;
	}
}