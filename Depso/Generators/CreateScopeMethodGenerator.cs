using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class CreateScopeMethodGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateMethod);
	}

	private static void GenerateMethod(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		INamedTypeSymbol classSymbol = generationContext.ClassSymbol;

		codeBuilder.AppendLine();

		string scopeType = $"{classSymbol.ToDisplayString()}.{Constants.ScopeClassName}".WithGlobalPrefix();

		using MethodBuilder methodBuilder = codeBuilder.Method(scopeType, Constants.CreateScopeMethodName).Private();
		methodBuilder.AddParameter("object?", "sync");

		codeBuilder.AppendLine($"{Constants.ThrowIfDisposedMethodName}();");
		codeBuilder.AppendLine($"return new {scopeType}(this, sync);");
	}
}