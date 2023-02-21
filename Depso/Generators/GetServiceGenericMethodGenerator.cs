using Depso.CSharp;

namespace Depso.Generators;

public class GetServiceGenericMethodGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateMethod);
	}

	private static void GenerateMethod(GenerationContext generationContext)
	{
		generationContext.AddNewLineIfNecessary();

		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		using MethodBuilder methodBuilder = codeBuilder.Method("T", Constants.GetServiceMethodName).Private();
		methodBuilder.AddTypeParameter("T");

		codeBuilder.AppendLine($"return (T){Constants.GetServiceMethodName}(typeof(T))!;");

		generationContext.AddNewLine = true;
	}
}