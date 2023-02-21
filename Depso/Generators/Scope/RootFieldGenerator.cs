using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators.Scope;

public class RootFieldGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateFields);
	}

	private static void GenerateFields(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		
		string rootType = generationContext.ClassSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		codeBuilder.Field(rootType, "_root").Private().ReadOnly();

		generationContext.AddNewLine = true;
	}
}