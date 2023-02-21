using System;
using Depso.CSharp;

namespace Depso.Generators;

public class GetServiceMethodGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateMethod);
	}

	private static void GenerateMethod(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		generationContext.AddNewLineIfNecessary();

		using MethodBuilder method = codeBuilder.Method("object?", Constants.GetServiceMethodName);
		method.AddParameter(Constants.TypeMetadataName.WithGlobalPrefix(), "serviceType");

		foreach (Action<GenerationContext> action in generationContext.GetServicesActions)
		{
			action(generationContext);
		}

		generationContext.AddNewLineIfNecessary();
		generationContext.AddNewLine = true;

		codeBuilder.AppendLine("return null;");
	}
}