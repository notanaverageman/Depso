using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class DisposeAsyncMethodGenerator : IGenerator
{
	public void Generate(GenerationContext generationContext)
	{
		generationContext.Actions.Add(GenerateMethod);
	}

	private void GenerateMethod(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		generationContext.AddNewLineIfNecessary();

		string valueTaskType = Constants.ValueTaskMetadataName;
		
		bool isAsyncMethod = 
			!generationContext.IsScopeClass ||
			generationContext.HasScopedAsyncDisposable ||
			generationContext.HasTransientAsyncDisposable;

		using MethodBuilder methodBuilder = codeBuilder.Method(valueTaskType, Constants.DisposeAsyncMethodName).Public();

		DisposeMethodGenerator.AddDisposeCheck(generationContext, returnDefault: !isAsyncMethod);

		if (generationContext.HasTransientDisposable)
		{
			DisposeMethodGenerator.DisposeTransients(generationContext);
		}

		if (generationContext.HasTransientAsyncDisposable)
		{
			DisposeAsyncTransients(generationContext);
		}

		if (!generationContext.IsScopeClass)
		{
			DisposeAsyncRootScope(generationContext);
		}

		DisposeAsyncServices(generationContext);

		if (isAsyncMethod)
		{
			methodBuilder.SetAsync();
		}
		else
		{
			generationContext.AddNewLineIfNecessary();
			codeBuilder.AppendLine("return default;");
		}
	}
	
	private static void DisposeAsyncTransients(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		generationContext.AddNewLineIfNecessary();

		using (codeBuilder.If($"{Constants.AsyncDisposablesFieldName} != null"))
		{
			using (codeBuilder.ForReverse(initialValue: $"{Constants.AsyncDisposablesFieldName}.Count - 1"))
			{
				codeBuilder.AppendLine($"await {Constants.AsyncDisposablesFieldName}[i].{Constants.DisposeAsyncMethodName}();");
			}
		}

		generationContext.AddNewLine = true;
	}
	
	private static void DisposeAsyncRootScope(GenerationContext generationContext)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		generationContext.AddNewLineIfNecessary();

		codeBuilder.AppendLine($"if ({Constants.RootScopeFieldName} != null) await {Constants.RootScopeFieldName}.{Constants.DisposeAsyncMethodName}();");

		generationContext.AddNewLine = true;
	}
	
	private void DisposeAsyncServices(GenerationContext generationContext)
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

			if (fieldType.IsAsyncDisposable(knownTypes))
			{
				generationContext.AddNewLineIfNecessary();

				codeBuilder.AppendLine($"if ({fieldName} != null) await {fieldName}.{Constants.DisposeAsyncMethodName}();");
				addedLines = true;
			}
			else if (fieldType.IsDisposable(knownTypes))
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