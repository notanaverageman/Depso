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

		int i = 0;

		foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
		{
			if (serviceDescriptor.Lifetime != Lifetime.Scoped)
			{
				continue;
			}

			INamedTypeSymbol serviceType = serviceDescriptor.ServiceType;

			if (serviceDescriptor.RedirectToThis && generationContext.IsScopeClass)
			{
				int order = serviceDescriptor.Order ?? i;
				i++;
				
				generationContext.GetServicesActions.Add(new GetServicesAction(
					x => ProcessType(x, serviceDescriptor, serviceType),
					GetServicesAction.OrderScoped + order));
				
				continue;
			}

			if (processedTypes.Add(serviceType))
			{
				int order = serviceDescriptor.Order ?? i;
				i++;

				generationContext.GetServicesActions.Add(new GetServicesAction(
					x => ProcessType(x, serviceDescriptor, serviceType),
					GetServicesAction.OrderScoped + order));
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
					x => ProcessType(x, serviceDescriptor, alsoRegisterAs),
					GetServicesAction.OrderScoped + order));
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