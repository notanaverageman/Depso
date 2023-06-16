using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class ServiceScopeFactoryGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateProperty);
	}

	private static void GenerateProperty(GenerationContext generationContext)
	{
		INamedTypeSymbol? serviceScope = generationContext.KnownTypes.IServiceScope;
		INamedTypeSymbol? serviceScopeFactory = generationContext.KnownTypes.IServiceScopeFactory;

		if (serviceScope == null || serviceScopeFactory == null)
		{
			return;
		}

		generationContext.AddNewLineIfNecessary();

		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		
		string returnType = serviceScope.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		string interfaceType = serviceScopeFactory.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		codeBuilder.AppendLine($"{returnType} {interfaceType}.CreateScope() => this.{Constants.CreateScopeMethodName}({Constants.LockFieldName});");

		generationContext.AddNewLine = true;
	}
}