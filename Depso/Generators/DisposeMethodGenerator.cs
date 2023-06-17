using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class DisposeMethodGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateMethod);
	}

	private void GenerateMethod(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		codeBuilder.AppendLine();

		using MethodBuilder methodBuilder = codeBuilder.Method("void", Constants.DisposeMethodName).Public();

		AddDisposeCheck(generationContext);

		if (generationContext.HasTransientDisposable)
		{
			DisposeTransients(generationContext);
		}

		if (!generationContext.IsScopeClass)
		{
			DisposeRootScope(generationContext);
		}

		DisposeServices(generationContext);

		generationContext.AddNewLine = true;
	}

	public static void AddDisposeCheck(GenerationContext generationContext, bool returnDefault = false)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		using (codeBuilder.Lock(Constants.LockFieldName))
		{
			using (codeBuilder.If(Constants.IsDisposedFieldName))
			{
				codeBuilder.AppendLine(returnDefault ? "return default;" : "return;");
			}

			codeBuilder.AppendLine();
			codeBuilder.AppendLine($"{Constants.IsDisposedFieldName} = true;");
		}

		generationContext.AddNewLine = true;
	}

	public static void DisposeTransients(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		generationContext.AddNewLineIfNecessary();

		using (codeBuilder.If($"{Constants.DisposablesFieldName} != null"))
		{
			using (codeBuilder.ForReverse(initialValue: $"{Constants.DisposablesFieldName}.Count - 1"))
			{
				codeBuilder.AppendLine($"{Constants.DisposablesFieldName}[i].{Constants.DisposeMethodName}();");
			}
		}

		generationContext.AddNewLine = true;
	}

	private static void DisposeRootScope(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		generationContext.AddNewLineIfNecessary();

		codeBuilder.AppendLine($"if ({Constants.RootScopeFieldName} != null) {Constants.RootScopeFieldName}.{Constants.DisposeMethodName}();");
		
		generationContext.AddNewLine = true;
	}

	private void DisposeServices(GenerationContext generationContext)
	{
		KnownTypes knownTypes = generationContext.KnownTypes;
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		
		bool addedLines = false;

		foreach (ServiceDescriptor serviceDescriptor in generationContext.GetDependencySortedDescriptors())
		{
			// Transients are stored in disposable lists and they are handled above.
			if (serviceDescriptor.Lifetime == Lifetime.Transient)
			{
				continue;
			}

			if (serviceDescriptor.RedirectToThis)
			{
				continue;
			}

			if (generationContext.IsScopeClass && serviceDescriptor.Lifetime == Lifetime.Singleton)
			{
				continue;
			}

			if (!generationContext.IsScopeClass && serviceDescriptor.Lifetime == Lifetime.Scoped)
			{
				continue;
			}

			ITypeSymbol concreteType = serviceDescriptor.ImplementationType ?? serviceDescriptor.ServiceType;
			ITypeSymbol fieldType = concreteType.WithNullableAnnotation(NullableAnnotation.None);
			
			string fieldName = serviceDescriptor.GetFieldName();

			if (fieldType.IsDisposable(knownTypes))
			{
				generationContext.AddNewLineIfNecessary();

				codeBuilder.AppendLine($"if ({fieldName} != null) {fieldName}.{Constants.DisposeMethodName}();");
				addedLines = true;
			}
		}

		if (addedLines)
		{
			generationContext.AddNewLine = true;
		}
	}
}