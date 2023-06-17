using System.Collections.Generic;
using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class SingletonFieldsGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		if (!generationContext.HasSingleton)
		{
			return;
		}

		generationContext.Actions.Add(GenerateFields);
	}

	private static void GenerateFields(GenerationContext generationContext)
	{
		IReadOnlyList<ServiceDescriptor> serviceDescriptors = generationContext.ServiceDescriptors;
		IndexManager indexManager = generationContext.IndexManager;
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		bool addedFields = false;

		foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
		{
			if (serviceDescriptor.Lifetime != Lifetime.Singleton)
			{
				continue;
			}

			if (serviceDescriptor.RedirectToThis)
			{
				continue;
			}

			if (serviceDescriptor.Factory != null)
			{
				continue;
			}

			generationContext.AddNewLineIfNecessary();

			ITypeSymbol concreteType = serviceDescriptor.ImplementationType ?? serviceDescriptor.ServiceType;
			ITypeSymbol fieldType = concreteType.WithNullableAnnotation(NullableAnnotation.None);

			indexManager.Add(serviceDescriptor);

			string methodName = serviceDescriptor.GetCreateMethodName();
			string fieldName = serviceDescriptor.GetFieldName();
			string propertyName = fieldName.ToPropertyName();
			string fieldTypeName = $"{fieldType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}";

			if (addedFields)
			{
				codeBuilder.AppendLine();
			}

			codeBuilder.AppendLine($"private {fieldTypeName}? {fieldName};");
			codeBuilder.AppendLine($"private {fieldTypeName} {propertyName} => {fieldName} ??= {methodName}();");

			addedFields = true;
		}

		if (addedFields)
		{
			generationContext.AddNewLine = true;
		}
	}
}