using System.Collections.Generic;
using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class ServiceProviderIsServiceGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateMethod);
	}

	private static void GenerateMethod(GenerationContext generationContext)
	{
		INamedTypeSymbol? serviceProviderIsService = generationContext.KnownTypes.IServiceProviderIsService;

		if (serviceProviderIsService == null)
		{
			return;
		}

		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		generationContext.AddNewLineIfNecessary();
		generationContext.AddNewLine = true;

		using MethodBuilder methodBuilder = codeBuilder.Method("bool", "IsService");
		methodBuilder.AddParameter($"global::{Constants.TypeMetadataName}", "serviceType");

		SortedSet<string> types = new();

		foreach (ServiceDescriptor serviceDescriptor in generationContext.ServiceDescriptors)
		{
			string serviceType = serviceDescriptor.ServiceType
				.WithNullableAnnotation(NullableAnnotation.None)
				.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			types.Add(serviceType);

			if (serviceDescriptor.AlsoRegisterAs == null)
			{
				continue;
			}

			foreach (INamedTypeSymbol alsoRegisterAs in serviceDescriptor.AlsoRegisterAs)
			{
				string alsoRegisterAsType = alsoRegisterAs
					.WithNullableAnnotation(NullableAnnotation.None)
					.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

				types.Add(alsoRegisterAsType);
			}
		}
		
		if (types.Count == 0)
		{
			codeBuilder.AppendLine("return false;");
			return;
		}

		using (codeBuilder.If($"serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(global::{Constants.IEnumerableMetadataName}<>)"))
		{
			codeBuilder.AppendLine("serviceType = serviceType.GetGenericArguments()[0];");
		}

		codeBuilder.AppendLine();
		codeBuilder.AppendLine("return false");
		codeBuilder.Indent();

		int i = 0;

		foreach (string type in types)
		{
			codeBuilder.Append($"|| serviceType == typeof({type})");

			if (i == types.Count - 1)
			{
				codeBuilder.AppendLine(";", indent: false);
			}
			else
			{
				codeBuilder.AppendLine();
			}

			i++;
		}

		codeBuilder.Unindent();
	}
}