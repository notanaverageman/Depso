using System.Collections.Generic;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Depso.Test;

public class DiagnosticTests : TestBase
{
	[Test]
	public void Non_partial_class()
	{
		DiagnosticResult diagnostic = DiagnosticResult
			.CompilerError(Diagnostics.ClassNotPartial.Id)
			.WithLocation(1);

		CheckDiagnostics<ServiceProviderGenerator>(diagnostic);
	}

	[Test]
	public void No_register_services_method()
	{
		DiagnosticResult diagnostic = DiagnosticResult
			.CompilerError(Diagnostics.RegisterServicesMethodNotFound.Id)
			.WithLocation(1);

		CheckDiagnostics<ServiceProviderGenerator>(diagnostic);
	}

	[Test]
	public void Illegal_statements()
	{
		DiagnosticResult diagnostic = DiagnosticResult.CompilerError(Diagnostics.IllegalStatement.Id);
		List<DiagnosticResult> diagnostics = new();

		for (int i = 1; i <= 8; i++)
		{
			diagnostics.Add(diagnostic.WithLocation(i));
		}

		CheckDiagnostics<ServiceProviderGenerator>(diagnostics.ToArray());
	}
}