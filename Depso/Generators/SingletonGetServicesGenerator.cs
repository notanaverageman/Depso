using System.Collections.Generic;
using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class SingletonGetServicesGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		IReadOnlyList<ServiceDescriptor> serviceDescriptors = generationContext.ServiceDescriptors;
		HashSet<ITypeSymbol> processedTypes = generationContext.GetServicesProcessedTypes;

		foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
		{
			if (serviceDescriptor.Lifetime != Lifetime.Singleton)
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
				if (!processedTypes.Add(alsoRegisterAs))
				{
					continue;
				}

				generationContext.GetServicesActions.Add(x => ProcessType(x, alsoRegisterAs));
			}
		}
	}

	protected virtual void ProcessType(
		GenerationContext generationContext,
		INamedTypeSymbol serviceType)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		
		ServiceDescriptor serviceDescriptor = generationContext.GetEffectiveServiceDescriptorForType(serviceType);

		string typeName = serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		if (serviceDescriptor.RedirectToThis)
		{
			codeBuilder.AppendLine($"if (serviceType == typeof({typeName})) return this;");
		}
		else
		{
		string fieldName = serviceDescriptor.GetFieldName();
		string propertyName = fieldName.ToPropertyName();
		
		codeBuilder.AppendLine($"if (serviceType == typeof({typeName})) return {propertyName};");
		}

		generationContext.AddNewLine = true;
	}
}