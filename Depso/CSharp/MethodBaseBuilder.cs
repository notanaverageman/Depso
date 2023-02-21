using System;

namespace Depso.CSharp;

public class MethodBaseBuilder : ModifierBuilder, IDisposable
{
	private bool _hasParameters;
		
	private int DefinitionLength { get; }
	private int ParameterLength { get; set; }

	private int ParameterEndOffset => ModifierEndOffset + DefinitionLength + TypeParameterLength + ParameterLength;

	protected override int TypeParameterStartOffset => ModifierEndOffset + DefinitionLength;
	protected override int WhereClauseStartOffset => ParameterEndOffset;

	protected MethodBaseBuilder(
		CodeBuilder codeBuilder,
		string definition,
		string defaultVisibility)
		:
		base(codeBuilder, defaultVisibility)
	{
		CodeBuilder.Append($"{definition}()", indent: false);

		DefinitionLength = definition.Length;
		ParameterLength = "()".Length;
			
		CodeBuilder.AppendLine();

		CodeBuilder.AppendLine("{");
		CodeBuilder.Indent();
	}

	public void AddParameter(string type, string name, bool onNewLine = false)
	{
		// Insert just before ) character.
		int insertIndex = ParameterEndOffset - 1;

		if (_hasParameters)
		{
			CodeBuilder.Insert(", ", insertIndex);
				
			insertIndex += ", ".Length;
			ParameterLength += ", ".Length;
		}

		if (onNewLine)
		{
			int count = CodeBuilder.Insert(Environment.NewLine, insertIndex);
			count += CodeBuilder.Insert("", insertIndex + count, indent: true);
				
			insertIndex += count;
			ParameterLength += count;
		}

		string parameter = $"{type} {name}";
			
		CodeBuilder.Insert(parameter, insertIndex);
		ParameterLength += parameter.Length;

		_hasParameters = true;
	}

	public void Dispose()
	{
		CodeBuilder.Unindent();
		CodeBuilder.AppendLine("}");
	}
}