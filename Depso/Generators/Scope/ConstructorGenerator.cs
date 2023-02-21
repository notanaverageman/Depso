using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators.Scope;

public class ConstructorGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateConstructor);
	}

	private static void GenerateConstructor(GenerationContext generationContext)
	{
		generationContext.AddNewLineIfNecessary();

		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		string rootType = generationContext.ClassSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		using ConstructorBuilder constructorBuilder = codeBuilder.Constructor(Constants.ScopeClassName);
		constructorBuilder.AddParameter(rootType, "root");
		constructorBuilder.AddParameter("object?", "sync");

		codeBuilder.AppendLine("_root = root;");
		codeBuilder.AppendLine();

		using (codeBuilder.If("sync != null"))
		{
			codeBuilder.AppendLine($"{Constants.LockFieldName} = sync;");
		}

		generationContext.AddNewLine = true;
	}
}