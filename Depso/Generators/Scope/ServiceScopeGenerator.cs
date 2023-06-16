using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators.Scope;

public class ServiceScopeGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateProperty);
	}

	private static void GenerateProperty(GenerationContext generationContext)
	{
		INamedTypeSymbol? serviceScope = generationContext.KnownTypes.IServiceScope;
		INamedTypeSymbol serviceProvider = generationContext.KnownTypes.IServiceProvider;

		if (serviceScope == null)
		{
			return;
		}

		generationContext.AddNewLineIfNecessary();

		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		
		string returnType = serviceProvider.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		string interfaceType = serviceScope.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		codeBuilder.AppendLine($"{returnType} {interfaceType}.ServiceProvider => this;");

		generationContext.AddNewLine = true;
	}
}