using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class EnumerableFieldsGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateFields);
	}

	private static void GenerateFields(GenerationContext generationContext)
	{
		IndexManager indexManager = generationContext.IndexManager;
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		KnownTypes knownTypes = generationContext.KnownTypes;

		bool addedFields = false;

		foreach (INamedTypeSymbol type in generationContext.EnumerableTypes)
		{
			Lifetime? lifetime = generationContext.GetEnumerableLifetime(type);

			bool isScoped = generationContext.IsScopeClass && lifetime == Lifetime.Scoped;
			bool isSingleton = !generationContext.IsScopeClass && lifetime == Lifetime.Singleton;

			if (!isSingleton && !isScoped)
			{
				continue;
			}
			
			generationContext.AddNewLineIfNecessary();

			INamedTypeSymbol fieldType = knownTypes.IEnumerable.Construct(type);
			ServiceDescriptor serviceDescriptor = new(lifetime!.Value, type);

			indexManager.Add(serviceDescriptor);
			
			string methodName = $"CreateEnumerable{type.Name.ToPascalCase()}";
			string fieldName = $"_enumerable{type.Name.ToPascalCase()}";
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