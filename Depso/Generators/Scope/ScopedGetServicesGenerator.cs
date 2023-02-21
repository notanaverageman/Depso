using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators.Scope;

public class ScopedGetServicesGenerator : Generators.ScopedGetServicesGenerator
{
	protected override void ProcessType(
		GenerationContext generationContext,
		ServiceDescriptor serviceDescriptor,
		INamedTypeSymbol serviceType)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		if (serviceDescriptor.Factory == null)
		{
			serviceDescriptor = generationContext.GetEffectiveServiceDescriptorForType(serviceType);
		}

		string typeName = serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		string fieldName = serviceDescriptor.GetFieldName();
		string propertyName = fieldName.ToPropertyName();
		
		codeBuilder.AppendLine($"if (serviceType == typeof({typeName})) return {propertyName};");

		generationContext.AddNewLine = true;
	}
}