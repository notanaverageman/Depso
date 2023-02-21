using System;

namespace Depso.CSharp;

public class TypeBuilder : ModifierBuilder, IDisposable
{
	private bool _hasBase;

	protected int DefinitionLength { get; set; }
	protected int DefinitionEndOffset => ModifierEndOffset + DefinitionLength;
		
	protected override int TypeParameterStartOffset => DefinitionEndOffset;
	protected override int WhereClauseStartOffset => TypeParameterStartOffset + TypeParameterLength;

	protected TypeBuilder(CodeBuilder codeBuilder, string kind, string name) : base(codeBuilder, "public")
	{
		string definition = $"{kind} {name}";
			
		CodeBuilder.Append(definition, indent: false);
		DefinitionLength += definition.Length;
			
		CodeBuilder.AppendLine();

		CodeBuilder.AppendLine("{");
		CodeBuilder.Indent();
	}

	public void AddBase(string @base)
	{
		if (!_hasBase)
		{
			string separator = $"\n{CodeBuilder.CurrentIndentation}:\n";

			CodeBuilder.Insert(separator, DefinitionEndOffset);
			DefinitionLength += separator.Length;

			_hasBase = true;
		}
		else
		{
			string separator = ",\n";

			CodeBuilder.Insert(separator, DefinitionEndOffset);
			DefinitionLength += separator.Length;
		}

		@base = $"{CodeBuilder.CurrentIndentation}{@base}";

		CodeBuilder.Insert(@base, DefinitionEndOffset);
		DefinitionLength += @base.Length;
	}

	public void Dispose()
	{
		CodeBuilder.Unindent();
		CodeBuilder.AppendLine("}");
	}
}