using Depso.CSharp;

namespace Depso.Generators;

public class AddAsyncDisposableMethodGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		if (!generationContext.HasTransientAsyncDisposable)
		{
			return;
		}

		generationContext.Actions.Add(GenerateMethod);
	}

	private static void GenerateMethod(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		codeBuilder.AppendLine();

		using MethodBuilder methodBuilder = codeBuilder.Method("void", Constants.AddAsyncDisposableMethodName).Private();
		methodBuilder.AddParameter(Constants.IAsyncDisposableMetadataName.WithGlobalPrefix(), "disposable");

		using (codeBuilder.Lock(Constants.LockFieldName))
		{
			codeBuilder.AppendLine($"{Constants.ThrowIfDisposedMethodName}();");
			codeBuilder.AppendLine();

			using (codeBuilder.If($"{Constants.AsyncDisposablesFieldName} == null"))
			{
				codeBuilder.AppendLine($"{Constants.AsyncDisposablesFieldName} = new {Constants.ListMetadataName.WithGlobalPrefix()}<{Constants.IAsyncDisposableMetadataName.WithGlobalPrefix()}>();");
			}

			codeBuilder.AppendLine();

			codeBuilder.AppendLine($"{Constants.AsyncDisposablesFieldName}.Add(disposable);");
		}
	}
}