using System;

namespace Depso.CSharp;

public class CaseBuilder : IDisposable
{
	private readonly CodeBuilder _codeBuilder;
	private readonly bool _addBraces;

	public CaseBuilder(CodeBuilder codeBuilder, string label, bool addBraces)
	{
		_codeBuilder = codeBuilder;
		_addBraces = addBraces;

		codeBuilder.AppendLine($"case {label}:");
			
		if (addBraces)
		{
			codeBuilder.AppendLine("{");
		}
			
		codeBuilder.Indent();
	}

	public void Dispose()
	{
		_codeBuilder.Unindent();

		if (_addBraces)
		{
			_codeBuilder.AppendLine("}");
		}
	}
}