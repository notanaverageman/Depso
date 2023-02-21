using System;

namespace Depso.CSharp;

public class DefaultBuilder : IDisposable
{
	private readonly CodeBuilder _codeBuilder;
	private readonly bool _addBraces;

	public DefaultBuilder(CodeBuilder codeBuilder, bool addBraces)
	{
		_codeBuilder = codeBuilder;
		_addBraces = addBraces;

		codeBuilder.AppendLine("default:");
			
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