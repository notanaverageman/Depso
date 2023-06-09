using Microsoft.CodeAnalysis;

namespace Depso;

public static class Diagnostics
{
	public static readonly DiagnosticDescriptor ClassNotPartial = new(
		"DEP001",
		"Class has to be partial",
		"Class has to be partial {0}",
		"Usage",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor RegisterServicesMethodNotFound = new(
		"DEP002",
		$"{Constants.RegisterServicesMethodName} method not found",
		$"Method private void {Constants.RegisterServicesMethodName}() not found on class {0}",
		"Usage",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor RegisterServicesStaticMethodNotFound = new(
		"DEP003",
		$"{Constants.RegisterServicesMethodName} method not found",
		$"Method private static void {Constants.RegisterServicesMethodName}() not found on class {0}",
		"Usage",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor RegisterServicesMethodBodyNotFound = new(
		"DEP004",
		$"{Constants.RegisterServicesMethodName} method body not found",
		$"{Constants.RegisterServicesMethodName} method body not found",
		"Usage",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor IllegalStatement = new(
		"DEP005",
		$"Illegal statement on {Constants.RegisterServicesMethodName} method",
		$"{Constants.RegisterServicesMethodName} method body contains illegal statements.",
		"Usage",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor RecursiveDependency = new(
		"DEP006",
		"Recursive dependency detected",
		"Recursive dependency detected while graph. Recursive chains:\n{0}",
		"Usage",
		DiagnosticSeverity.Error,
		true);
}