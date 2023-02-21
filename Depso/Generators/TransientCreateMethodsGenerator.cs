using Depso.CSharp;
using Microsoft.CodeAnalysis;

namespace Depso.Generators;

public class TransientCreateMethodsGenerator : CreateMethodsGenerator
{
	public override void Generate(GenerationContext generationContext)
	{
		if (!generationContext.HasTransient)
		{
			return;
		}

		generationContext.Actions.Add(x => Generate(x, Lifetime.Transient));
	}

	protected override void GenerateCreateMethod(
		GenerationContext generationContext,
		INamedTypeSymbol concreteType,
		string methodName,
		bool isEnumerable)
	{
		if (generationContext.IsScopeClass)
		{
			return;
		}

		base.GenerateCreateMethod(generationContext, concreteType, methodName, isEnumerable);
	}

	protected override void GenerateFactoryMethod(
		GenerationContext generationContext,
		ServiceDescriptor serviceDescriptor,
		Lifetime lifetime,
		string methodName)
	{
		if (generationContext.IsScopeClass)
		{
			return;
		}

		base.GenerateFactoryMethod(generationContext, serviceDescriptor, lifetime, methodName);
	}

	protected override void ProcessServiceDescriptor(
		GenerationContext generationContext,
		ServiceDescriptor serviceDescriptor,
		string methodName)
	{
		KnownTypes knownTypes = generationContext.KnownTypes;
		CodeBuilder codeBuilder = generationContext.CodeBuilder;
		INamedTypeSymbol concreteType = serviceDescriptor.ImplementationType ?? serviceDescriptor.ServiceType;

		if (!concreteType.IsDisposableOrAsyncDisposable(knownTypes))
		{
			return;
		}

		string disposableMethodName = $"{methodName}{Constants.CreateDisposableMethodNameSuffix}";

		string fieldTypeName = concreteType
			.WithNullableAnnotation(NullableAnnotation.None)
			.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		string transientCreator = generationContext.IsScopeClass
			? $"_root.{methodName}()"
			: $"{methodName}()";

		codeBuilder.AppendLine();

		using (codeBuilder.Method(fieldTypeName, disposableMethodName).Private())
		{
			codeBuilder.AppendLine($"{fieldTypeName} service = {transientCreator};");

			codeBuilder.AppendLine(concreteType.IsDisposable(knownTypes)
				? $"{Constants.AddDisposableMethodName}(service);"
				: $"{Constants.AddAsyncDisposableMethodName}(service);");

			codeBuilder.AppendLine();

			codeBuilder.AppendLine("return service;");
		}
	}
}