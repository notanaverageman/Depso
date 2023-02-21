using Depso.CSharp;

namespace Depso.Generators;

public class AddDisposableMethodGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		if (!generationContext.HasTransientDisposable)
		{
			return;
		}

		generationContext.Actions.Add(GenerateMethod);
	}

	private static void GenerateMethod(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		codeBuilder.AppendLine();

		using MethodBuilder methodBuilder = codeBuilder.Method("void", Constants.AddDisposableMethodName).Private();
		methodBuilder.AddParameter(Constants.IDisposableMetadataName.WithGlobalPrefix(), "disposable");

		using (codeBuilder.Lock(Constants.LockFieldName))
		{
			codeBuilder.AppendLine($"{Constants.ThrowIfDisposedMethodName}();");
			codeBuilder.AppendLine();

			using (codeBuilder.If($"{Constants.DisposablesFieldName} == null"))
			{
				codeBuilder.AppendLine($"{Constants.DisposablesFieldName} = new {Constants.ListMetadataName.WithGlobalPrefix()}<{Constants.IDisposableMetadataName.WithGlobalPrefix()}>();");
			}

			codeBuilder.AppendLine();

			codeBuilder.AppendLine($"{Constants.DisposablesFieldName}.Add(disposable);");
		}
	}
}