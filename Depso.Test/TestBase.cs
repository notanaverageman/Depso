using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Depso.Test;

public abstract class TestBase
{
	public virtual bool PrintSyntaxTrees => false;
	public virtual bool AddServiceProviderAssembly => true;
	public virtual bool AddServiceCollectionAssembly => false;

	public void GenerateAndCheck<T>([CallerMemberName] string? testName = null) where T : IIncrementalGenerator, new()
	{
		string prefix = $"{Constants.GeneratorNamespace}.Test.SourceCodes.Generators.{testName}";
		string commonPrefix = $"{Constants.GeneratorNamespace}.Test.SourceCodes.Generators.Common";

		IReadOnlyList<string> resourceNames = GetResourceNames(prefix);
		IReadOnlyList<string> sourceNames = resourceNames.Where(x => x.StartsWith($"{prefix}.Source")).ToArray();
		IReadOnlyList<string> expectedNames = resourceNames.Where(x => x.StartsWith($"{prefix}.Expected")).ToArray();
		IReadOnlyList<string> commonExpectedNames = GetResourceNames(commonPrefix);

		List<SyntaxTree> inputSyntaxTrees = new();

		foreach (string sourceName in sourceNames)
		{
			string source = ReadResource(sourceName);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

			inputSyntaxTrees.Add(syntaxTree);
		}

		if (inputSyntaxTrees.Count == 0)
		{
			Assert.Fail($"No source found for {testName}");
		}
		
		List<SyntaxTree> syntaxTrees = Generate<T>(inputSyntaxTrees);

		Assert.Multiple(() =>
		{
			foreach (string expectedName in expectedNames.Concat(commonExpectedNames))
			{
				string fileName = expectedName
					.Replace(prefix, "")
					.Replace(commonPrefix, "")
					.Replace(".cs", ".g.cs")
					.Replace("Expected", Constants.GeneratorNamespace)
					.Trim('.');
				
				SyntaxTree? syntaxTree = syntaxTrees
					.Where(x => x.FilePath.EndsWith(".g.cs"))
					.SingleOrDefault(x => Path.GetFileName(x.FilePath) == fileName);
				
				Assert.That(syntaxTree, Is.Not.Null, $"Syntax tree not found for {expectedName}");

				string generated = syntaxTree!.ToString().Trim();
				string expected = ReadResource(expectedName);

				if (!PrintSyntaxTrees)
				{
					Debug.WriteLine(generated);
				}

				CheckEquality(generated, expected);
			}
		});
	}

	public List<SyntaxTree> Generate<T>(
		IEnumerable<SyntaxTree>? additionalSyntaxTrees = null,
		IEnumerable<MetadataReference>? additionalReferences = null,
		IEnumerable<AdditionalText>? additionalTexts = null)
		where T : IIncrementalGenerator, new()
	{
		Compilation outputCompilation = RunGenerator<T>(
			additionalSyntaxTrees,
			additionalReferences,
			additionalTexts,
			out ImmutableArray<Diagnostic> diagnostics);

		List<string> errorMessages = diagnostics
			.Where(x => x.Severity >= DiagnosticSeverity.Error)
			.Select(x => x.ToString())
			.ToList();

		List<string> warningMessages = diagnostics
			.Where(x => x.Severity == DiagnosticSeverity.Warning)
			.Select(x => x.ToString())
			.ToList();

		foreach (string message in warningMessages)
		{
			Console.WriteLine(message);
		}

		if (errorMessages.Any())
		{
			Assert.Fail(string.Join("\n", errorMessages));
		}

		List<SyntaxTree> result = outputCompilation.SyntaxTrees.ToList();

		if (PrintSyntaxTrees)
		{
			string separator = new('/', 100);
			separator = $"\n\n{separator}\n\n";

			Console.WriteLine(string.Join(separator, result));
		}

		return result;
	}

	private Compilation RunGenerator<T>(
		IEnumerable<SyntaxTree>? additionalSyntaxTrees,
		IEnumerable<MetadataReference>? additionalReferences,
		IEnumerable<AdditionalText>? additionalTexts,
		out ImmutableArray<Diagnostic> diagnostics) where T : IIncrementalGenerator, new()
	{
		List<MetadataReference> references = new();

		Assembly[] assemblies =
		{
			typeof(object).Assembly
		};

		foreach (Assembly assembly in assemblies)
		{
			if (!assembly.IsDynamic)
			{
				references.Add(MetadataReference.CreateFromFile(assembly.Location));
			}
		}

		List<SyntaxTree> syntaxTrees = new();

		if (additionalSyntaxTrees != null)
		{
			syntaxTrees.AddRange(additionalSyntaxTrees);
		}

		if (additionalReferences != null)
		{
			references.AddRange(additionalReferences);
		}

		if (AddServiceProviderAssembly)
		{
			references.Add(MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location));
		}

		if (AddServiceCollectionAssembly)
		{
			references.Add(MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location));
		}

		CSharpCompilationOptions compilationOptions = new(OutputKind.DynamicallyLinkedLibrary);

		CSharpCompilation compilation = CSharpCompilation.Create(
			"original",
			syntaxTrees,
			references,
			compilationOptions);

		GeneratorDriver driver = CSharpGeneratorDriver.Create(new T());

		if (additionalTexts != null)
		{
			driver = driver.AddAdditionalTexts(ImmutableArray.CreateRange(additionalTexts));
		}

		driver.RunGeneratorsAndUpdateCompilation(
			compilation,
			out Compilation outputCompilation,
			out diagnostics);

		return outputCompilation;
	}

	public void CheckDiagnostics<T>(
		DiagnosticResult diagnostic,
		[CallerMemberName] string? sourceResource = null) where T : IIncrementalGenerator, new()
	{
		CheckDiagnostics<T>(new[] { diagnostic }, sourceResource);
	}

	public void CheckDiagnostics<T>(
		DiagnosticResult[] diagnostics,
		[CallerMemberName] string? testMethod = null) where T : IIncrementalGenerator, new()
	{
		string prefix = $"{Constants.GeneratorNamespace}.Test.SourceCodes.Diagnostics.{testMethod}";
		string source = ReadResource($"{prefix}.Source.cs");

		GeneratorTest test = new()
		{
			TestCode = source
		};

		test.ExpectedDiagnostics.AddRange(diagnostics);

		SolutionState solutionState = test.ParseMarkup();
		List<DiagnosticResult> parsedDiagnostics = solutionState.ExpectedDiagnostics;

		RunGenerator<T>(
			solutionState.Sources.Select(x => CSharpSyntaxTree.ParseText(x.content)),
			Enumerable.Empty<MetadataReference>(),
			Enumerable.Empty<AdditionalText>(),
			out ImmutableArray<Diagnostic> producedDiagnostics);

		Diagnostic[] producedDiagnosticsSorted = producedDiagnostics
			.OrderBy(d => d.Location.GetLineSpan().Path, StringComparer.Ordinal)
			.ThenBy(d => d.Location.SourceSpan.Start)
			.ThenBy(d => d.Location.SourceSpan.End)
			.ThenBy(d => d.Id)
			.ToArray();

		Assert.Multiple(() =>
		{
			Assert.That(producedDiagnostics.Length, Is.EqualTo(parsedDiagnostics.Count));

			for (int i = 0; i < producedDiagnosticsSorted.Length; i++)
			{
				Diagnostic producedDiagnostic = producedDiagnosticsSorted[i];
				DiagnosticResult parsedDiagnostic = parsedDiagnostics[i];

				string? producedDiagnosticText = producedDiagnostic.Location.SourceTree
					?.GetText()
					.ToString(producedDiagnostic.Location.SourceSpan);

				string? message = producedDiagnosticText == null ? null : $"Text: {producedDiagnosticText}";

				Assert.That(
					producedDiagnostic.Id,
					Is.EqualTo(parsedDiagnostic.Id),
					message);

				Assert.That(
					producedDiagnostic.Location.GetLineSpan().Span,
					Is.EqualTo(parsedDiagnostic.Spans[0].Span.Span),
					message);
			}
		});
	}

	private void CheckEquality(string generated, string expected)
	{
		generated = generated.ReplaceLineEndings();
		expected = expected.ReplaceLineEndings();

		Assert.That(generated, Is.EqualTo(expected));
	}

	protected string ReadResource(string resource)
	{
		Assembly assembly = typeof(TestBase).Assembly;

		Stream? stream = assembly.GetManifestResourceStream(resource);

		if (stream == null)
		{
			throw new ArgumentException($"Resource not found: {resource}");
		}

		using (stream)
		using (StreamReader reader = new(stream))
		{
			return reader.ReadToEnd();
		}
	}

	protected IReadOnlyList<string> GetResourceNames(string prefix)
	{
		return typeof(TestBase).Assembly
			.GetManifestResourceNames()
			.Where(x => x.StartsWith(prefix))
			.ToArray();
	}

	private class DummyAnalyzer : DiagnosticAnalyzer
	{
		public override void Initialize(AnalysisContext context)
		{
		}

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray<DiagnosticDescriptor>.Empty;
	}

	private class GeneratorTest : CSharpAnalyzerTest<DummyAnalyzer, NUnitVerifier>
	{

		public SolutionState ParseMarkup()
		{
			TestState.InheritanceMode = StateInheritanceMode.Explicit;
			TestState.MarkupHandling = MarkupMode.Allow;

			SolutionState state = TestState.WithProcessedMarkup(
				MarkupOptions.None,
				null,
				ImmutableArray<DiagnosticDescriptor>.Empty,
				ImmutableArray<string>.Empty,
				DefaultFilePath);

			return state;
		}
	}
}