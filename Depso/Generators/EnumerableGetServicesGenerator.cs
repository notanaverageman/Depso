using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class EnumerableGetServicesGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.GetServicesActions.Add(new GetServicesAction(GenerateLines, GetServicesAction.OrderEnumerables));
	}

	private static void GenerateLines(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		foreach (INamedTypeSymbol type in generationContext.EnumerableTypes)
		{
			INamedTypeSymbol constructedType = generationContext.KnownTypes.IEnumerable.Construct(type);
			Lifetime? lifetime = generationContext.GetEnumerableLifetime(type);

			string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			string enumerableTypeName = constructedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			if (generationContext.IsScopeClass)
			{
				if (lifetime != Lifetime.Scoped)
				{
					codeBuilder.AppendLine($"if (serviceType == typeof({enumerableTypeName})) return _root.GetService(serviceType);");
				}
				else
				{
					ReturnProperty();
				}

				continue;
			}

			if (lifetime == null)
			{
				codeBuilder.AppendLine($"if (serviceType == typeof({enumerableTypeName})) return global::System.Array.Empty<{typeName}>();");
				continue;
			}
			
			if (lifetime == Lifetime.Singleton)
			{
				ReturnProperty();
			}
			else if (lifetime == Lifetime.Scoped)
			{
				codeBuilder.AppendLine($"if (serviceType == typeof({enumerableTypeName})) return {Constants.RootScopePropertyName}.GetService(serviceType);");
			}
			else if (lifetime == Lifetime.Transient)
			{
				string methodName = $"CreateEnumerable{type.Name.ToPascalCase()}";
				codeBuilder.AppendLine($"if (serviceType == typeof({enumerableTypeName})) return {methodName}();");
			}

			void ReturnProperty()
			{
				string fieldName = $"_enumerable{type.Name.ToPascalCase()}";
				string propertyName = fieldName.ToPropertyName();

				codeBuilder.AppendLine($"if (serviceType == typeof({enumerableTypeName})) return {propertyName};");
			}
		}
	}
}