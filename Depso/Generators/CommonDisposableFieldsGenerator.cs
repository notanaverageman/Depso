using Depso.CSharp;

namespace Depso.Generators;

public class CommonDisposableFieldsGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateFields);
	}

	private static void GenerateFields(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		generationContext.AddNewLineIfNecessary();
		
		codeBuilder.AppendLine($"private bool {Constants.IsDisposedFieldName};");
		
		generationContext.AddNewLine = true;
	}
}