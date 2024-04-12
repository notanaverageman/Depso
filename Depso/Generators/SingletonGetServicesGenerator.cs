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
		
		int i = 0;

		foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
		{
			if (serviceDescriptor.Lifetime != Lifetime.Singleton)
			{
				continue;
			}

			INamedTypeSymbol serviceType = serviceDescriptor.ServiceType;

			if (processedTypes.Add(serviceType))
			{
				int order = serviceDescriptor.Order ?? i;
				i++;

				generationContext.GetServicesActions.Add(new GetServicesAction(
					x => ProcessType(x, serviceType),
					GetServicesAction.OrderSingleton + order));
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

				int order = serviceDescriptor.Order ?? i;
				i++;

				generationContext.GetServicesActions.Add(new GetServicesAction(
					x => ProcessType(x, alsoRegisterAs),
					GetServicesAction.OrderSingleton + order));
			}
		}
	}

	protected virtual void ProcessType(
		GenerationContext generationContext,
		INamedTypeSymbol serviceType)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		
		ServiceDescriptor serviceDescriptor = generationContext.GetEffectiveServiceDescriptorForType(
			serviceType,
			Lifetime.Singleton);

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