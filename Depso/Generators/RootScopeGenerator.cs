using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class RootScopeGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateField);
	}

	private static void GenerateField(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		generationContext.AddNewLineIfNecessary();

		string className = generationContext.ClassSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		string scopeType = $"{className}.{Constants.ScopeClassName}";
		string fieldName = Constants.RootScopeFieldName;
		string propertyName = Constants.RootScopePropertyName;
		string methodName = Constants.CreateScopeMethodName;
		string lockField = Constants.LockFieldName;

		codeBuilder.Field($"{scopeType}?", Constants.RootScopeFieldName).Private();
		codeBuilder.AppendLine($"private {scopeType} {propertyName} => {fieldName} ??= {methodName}({lockField});");
		
		generationContext.AddNewLine = true;
	}
}