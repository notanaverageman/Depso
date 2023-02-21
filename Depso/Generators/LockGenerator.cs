using Depso.CSharp;

namespace Depso.Generators;

public class LockGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateField);
	}

	private static void GenerateField(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		
		generationContext.AddNewLineIfNecessary();

		string lockField = $"private readonly object {Constants.LockFieldName} = new object();";
		codeBuilder.AppendLine(lockField);
		
		generationContext.AddNewLine = true;
	}
}