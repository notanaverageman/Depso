using Depso.CSharp;

namespace Depso.Generators;

public class ThrowIfDisposedMethodGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateMethod);
	}

	private void GenerateMethod(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		codeBuilder.AppendLine();

		using (codeBuilder.Method("void", Constants.ThrowIfDisposedMethodName).Private())
		{
			using (codeBuilder.If(Constants.IsDisposedFieldName))
			{
				string className = generationContext.ClassSymbol.ToDisplayString();

				if (generationContext.IsScopeClass)
				{
					className += $".{Constants.ScopeClassName}";
				}

				codeBuilder.AppendLine($"throw new {Constants.ObjectDisposedException.WithGlobalPrefix()}(\"{className}\");");
			}
		}
		
		generationContext.AddNewLine = true;
	}
}