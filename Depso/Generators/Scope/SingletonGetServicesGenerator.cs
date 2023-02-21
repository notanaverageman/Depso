using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators.Scope;

public class SingletonGetServicesGenerator : Generators.SingletonGetServicesGenerator
{
	protected override void ProcessType(
		GenerationContext generationContext,
		INamedTypeSymbol serviceType)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		string typeName = serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		codeBuilder.AppendLine($"if (serviceType == typeof({typeName})) return _root.{Constants.GetServiceMethodName}(serviceType);");

		generationContext.AddNewLine = true;
	}
}