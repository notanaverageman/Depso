using System;

namespace Depso.CSharp;

public class BlockBuilder : IDisposable
{
	private readonly bool _appendNewLineAtTheEnd;
		
	protected CodeBuilder CodeBuilder { get; }

	protected BlockBuilder(CodeBuilder codeBuilder, bool appendNewLineAtTheEnd = true)
	{
		_appendNewLineAtTheEnd = appendNewLineAtTheEnd;
		CodeBuilder = codeBuilder;
	}

	public virtual void Dispose()
	{
		CodeBuilder.Unindent();

		if (_appendNewLineAtTheEnd)
		{
			CodeBuilder.AppendLine("}");
		}
		else
		{
			CodeBuilder.Append("}");
		}
	}
}