using Microsoft.CodeAnalysis;

#pragma warning disable RS2008 // Enable analyzer release tracking
#pragma warning disable RS1032 // Define diagnostic message correctly

namespace Depso;

public static class Diagnostics
{
	public static readonly DiagnosticDescriptor InternalError = new(
		"DEP000",
		"Internal error while generating source code",
		"Internal error while generating source code {0}",
		"Usage",
		DiagnosticSeverity.Error,
		true);

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
		$"{Constants.RegisterServicesMethodName} method body contains illegal statements",
		"Usage",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor RecursiveDependency = new(
		"DEP006",
		"Recursive dependency detected",
		"Recursive dependency detected while graph. Recursive chains: {0}",
		"Usage",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor ClassNotConstructible = new(
		"DEP007",
		"Class cannot be constructed",
		"Class cannot be constructed",
		"Usage",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor NoPublicConstructors = new(
		"DEP008",
		"No public constructors exist",
		"No public constructors exist",
		"Usage",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor MissingDependencies = new(
		"DEP009",
		"Constructor misses dependencies",
		"Constructor misses dependencies: {0}",
		"Usage",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor AmbiguousConstructors = new(
		"DEP010",
		"Ambiguous constructors",
		"More than one constructor can be selected: {0}",
		"Usage",
		DiagnosticSeverity.Error,
		true);
}