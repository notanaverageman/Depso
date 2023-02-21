using Depso.CSharp;

namespace Depso.Generators;

public class DisposableFieldsGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		if (!generationContext.HasTransientDisposable)
		{
			return;
		}

		generationContext.Actions.Add(GenerateFields);
	}

	private static void GenerateFields(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		string fieldType = $"{Constants.ListMetadataName}<{Constants.IDisposableMetadataName.WithGlobalPrefix()}>?".WithGlobalPrefix();
		string fieldName = Constants.DisposablesFieldName;

		codeBuilder.Field(fieldType, fieldName).Private();
	}
}