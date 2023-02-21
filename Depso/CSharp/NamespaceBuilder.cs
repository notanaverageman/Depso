using System;

namespace Depso.CSharp;

public class NamespaceBuilder : IDisposable
{
	private readonly CodeBuilder _codeBuilder;

	public NamespaceBuilder(CodeBuilder codeBuilder, string @namespace)
	{
		_codeBuilder = codeBuilder;
			
		_codeBuilder.AppendLine($"namespace {@namespace}");
		_codeBuilder.AppendLine("{");
		_codeBuilder.Indent();
	}

	public void Dispose()
	{
		_codeBuilder.Unindent();
		_codeBuilder.AppendLine("}");
	}
}