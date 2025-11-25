using Depso.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Depso.Generators.Scope;

public class ScopedFieldsFactoryGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateFields);
	}

	private static void GenerateFields(GenerationContext generationContext)
	{
		IReadOnlyList<ServiceDescriptor> serviceDescriptors = generationContext.ServiceDescriptors;
		IndexManager indexManager = generationContext.IndexManager;
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
		{
			if (serviceDescriptor.Lifetime != Lifetime.Scoped || serviceDescriptor.Factory == null)
			{
				continue;
			}

			ITypeSymbol fieldType = serviceDescriptor.ImplementationType ?? serviceDescriptor.ServiceType;
			fieldType = fieldType.WithNullableAnnotation(NullableAnnotation.None);

			indexManager.Add(serviceDescriptor);

			string fieldName = serviceDescriptor.GetFieldName();
			string propertyName = fieldName.ToPropertyName();
			string fieldTypeName = fieldType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			string factoryMethodName = propertyName.ToFactoryMethodName();

			generationContext.AddNewLineIfNecessary();

			codeBuilder.Field($"{fieldTypeName}?", fieldName).Private();
			codeBuilder.AppendLine($"private {fieldTypeName} {propertyName} => {fieldName} ??= {factoryMethodName}();");

			generationContext.AddNewLine = true;
		}
	}
}