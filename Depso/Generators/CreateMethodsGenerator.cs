using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Depso.CSharp;

namespace Depso.Generators;

public abstract class CreateMethodsGenerator : IGenerator
{
	public abstract void Generate(GenerationContext generationContext);

	protected void Generate(GenerationContext generationContext, Lifetime lifetime)
	{
		IReadOnlyList<ServiceDescriptor> serviceDescriptors = generationContext.ServiceDescriptors;
		IReadOnlyCollection<INamedTypeSymbol> enumerableTypes = generationContext.EnumerableTypes;
		HashSet<ITypeSymbol> addedTypes = new(SymbolEqualityComparer.Default);

		foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
		{
			if (serviceDescriptor.Lifetime != lifetime)
			{
				continue;
			}
			
			if (serviceDescriptor.Factory == null)
			{
				INamedTypeSymbol concreteType = serviceDescriptor.ImplementationType ?? serviceDescriptor.ServiceType;

				if (!addedTypes.Add(concreteType))
				{
					continue;
				}
				
				string methodName = serviceDescriptor.GetCreateMethodName();

				GenerateCreateMethod(generationContext, concreteType, methodName, isEnumerable: false);
				ProcessServiceDescriptor(generationContext, serviceDescriptor, methodName);
			}
			else
			{
				string factoryMethodName = serviceDescriptor.GetFactoryMethodName();
				
				GenerateFactoryMethod(generationContext, serviceDescriptor, lifetime, factoryMethodName);
				ProcessServiceDescriptor(generationContext, serviceDescriptor, factoryMethodName);
			}
		}

		foreach (INamedTypeSymbol type in enumerableTypes)
		{
			Lifetime? enumerableLifetime = generationContext.GetEnumerableLifetime(type);

			if (enumerableLifetime != lifetime)
			{
				continue;
			}

			INamedTypeSymbol enumerableType = generationContext.KnownTypes.IEnumerable.Construct(type);

			if (!addedTypes.Add(enumerableType))
			{
				continue;
			}

			string methodName = $"CreateEnumerable{type.Name.ToPascalCase()}";

			GenerateCreateMethod(generationContext, type, methodName, isEnumerable: true);
		}
	}

	protected virtual void GenerateCreateMethod(
		GenerationContext generationContext,
		INamedTypeSymbol concreteType,
		string methodName,
		bool isEnumerable)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;

		string fieldTypeName = concreteType
			.WithNullableAnnotation(NullableAnnotation.None)
			.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		string returnType = isEnumerable
			? $"{fieldTypeName}[]"
			: fieldTypeName;

		codeBuilder.AppendLine();

		using (codeBuilder.Method(returnType, methodName).Private())
		{
			using (codeBuilder.Lock(Constants.LockFieldName))
			{
				codeBuilder.AppendLine($"{Constants.ThrowIfDisposedMethodName}();");

				if (isEnumerable)
				{
					GenerateEnumerableFieldCreator(generationContext, concreteType, fieldTypeName);
				}
				else
				{
					GenerateFieldCreator(generationContext, concreteType, fieldTypeName);
				}
			}
		}
	}

	protected virtual void GenerateFactoryMethod(
		GenerationContext generationContext,
		ServiceDescriptor serviceDescriptor,
		Lifetime lifetime,
		string methodName)
	{
		generationContext.AddNewLineIfNecessary();

		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		INamedTypeSymbol serviceType = serviceDescriptor.ServiceType;

		string fieldTypeName = serviceType
			.WithNullableAnnotation(NullableAnnotation.None)
			.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		using (codeBuilder.Method(returnType: fieldTypeName, name: methodName).Private())
		{
			string factoryInvocation = generationContext.GetFactoryInvocation(
				serviceDescriptor.Factory!,
				serviceProviderParameter: "this");

			using (codeBuilder.Lock(Constants.LockFieldName))
			{
				codeBuilder.AppendLine($"{Constants.ThrowIfDisposedMethodName}();");
				int currentOffset = codeBuilder.CurrentOffset;

				using System.IO.StringReader reader = new(factoryInvocation);
				int lineCount = 0;

				while (reader.ReadLine() is { } line)
				{
					codeBuilder.AppendLine(line.TrimStart());
					lineCount++;
				}

				if (lineCount > 1)
				{
					codeBuilder.Insert(CodeBuilder.NewLine, currentOffset);
				}
			}
		}

		generationContext.AddNewLine = true;
	}

	protected virtual void ProcessServiceDescriptor(
		GenerationContext generationContext,
		ServiceDescriptor serviceDescriptor,
		string methodName)
	{
	}

	private void GenerateFieldCreator(
		GenerationContext generationContext,
		INamedTypeSymbol typeSymbol,
		string fieldTypeName)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		IReadOnlyList<ITypeSymbol>? dependencies = generationContext.GetConstructorParameters(typeSymbol);

		if (dependencies == null)
		{
			// This will be reported while creating the dependency graph.
			return;
		}
		
		if (dependencies.Count == 0)
		{
			codeBuilder.AppendLine($"return new {fieldTypeName}();");
			return;
		}

		if (dependencies.Count == 1)
		{
			ITypeSymbol dependency = dependencies[0];
			string dependencyTypeName = dependency.ToDisplayString().WithGlobalPrefix();

			codeBuilder.AppendLine($"return new {fieldTypeName}({Constants.GetServiceMethodName}<{dependencyTypeName}>());");
			return;
		}

		codeBuilder.AppendLine();
		codeBuilder.AppendLine($"return new {fieldTypeName}(");
		codeBuilder.Indent();

		for (int i = 0; i < dependencies.Count; i++)
		{
			string comma = i < dependencies.Count - 1 ? "," : "";

			ITypeSymbol dependency = dependencies[i];

			string dependencyTypeName = dependency
				.WithNullableAnnotation(NullableAnnotation.None)
				.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			codeBuilder.AppendLine($"{Constants.GetServiceMethodName}<{dependencyTypeName}>(){comma}");
		}
		
		codeBuilder.Unindent();
		codeBuilder.AppendLine(");");
	}

	private void GenerateEnumerableFieldCreator(
		GenerationContext generationContext,
		INamedTypeSymbol typeSymbol,
		string fieldTypeName)
	{
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		IReadOnlyList<ServiceDescriptor> serviceDescriptors = generationContext.GetEnumerableDescriptors(typeSymbol);
		
		if (serviceDescriptors.Count == 0)
		{
			codeBuilder.AppendLine($"return global::System.Array.Empty<{fieldTypeName}>();");
			return;
		}

		codeBuilder.AppendLine();
		codeBuilder.AppendLine($"return new {fieldTypeName}[]");
		codeBuilder.AppendLine("{");
		codeBuilder.Indent();

		for (int i = 0; i < serviceDescriptors.Count; i++)
		{
			string comma = i < serviceDescriptors.Count - 1 ? "," : "";

			ServiceDescriptor serviceDescriptor = serviceDescriptors[i];

			string getter = serviceDescriptor.Lifetime == Lifetime.Transient
				? $"{serviceDescriptor.GetCreateMethodName()}()"
				: serviceDescriptor.GetFieldName().ToPropertyName();
			
			codeBuilder.AppendLine($"{getter}{comma}");
		}
		
		codeBuilder.Unindent();
		codeBuilder.AppendLine("};");
	}
}