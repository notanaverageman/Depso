using Depso.CSharp;

namespace Depso.Generators;

public class AsyncDisposableFieldsGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		if (!generationContext.HasTransientAsyncDisposable)
		{
			return;
		}
		
		generationContext.Actions.Add(GenerateFields);
	}

	private static void GenerateFields(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		string fieldType = $"{Constants.ListMetadataName}<{Constants.IAsyncDisposableMetadataName.WithGlobalPrefix()}>?".WithGlobalPrefix();
		string fieldName = Constants.AsyncDisposablesFieldName;

		codeBuilder.Field(fieldType, fieldName).Private();
	}
}