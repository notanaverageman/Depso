namespace Depso.CSharp;

public class ModifierBuilder
{
	private bool _hasTypeParameters;

	protected readonly CodeBuilder CodeBuilder;

	protected string VisibilityModifier { get; private set; }

	protected int VisibilityOffset { get; }
	protected int StaticOffset => VisibilityOffset + VisibilityLength;
	protected int ReadOnlyOffset => StaticOffset + StaticLength;
	protected int PartialOffset => ReadOnlyOffset + ReadOnlyLength;
	protected int AsyncOffset => PartialOffset + PartialLength;

	protected int ModifierEndOffset => AsyncOffset + AsyncLength;

	protected int VisibilityLength { get; private set; }
	protected int StaticLength { get; private set; }
	protected int ReadOnlyLength { get; private set; }
	protected int PartialLength { get; private set; }
	protected int AsyncLength { get; private set; }

	protected int TypeParameterLength { get; private set; }
	protected int WhereClauseLength { get; private set; }

	protected virtual int TypeParameterStartOffset { get; set; }
	protected virtual int WhereClauseStartOffset { get; set; }

	protected ModifierBuilder(CodeBuilder codeBuilder, string defaultVisibility)
	{
		CodeBuilder = codeBuilder;
			
		// Indent before getting start offset.
		CodeBuilder.Append("");

		VisibilityModifier = defaultVisibility;
			
		VisibilityOffset = CodeBuilder.CurrentOffset;
		VisibilityLength = VisibilityModifier.Length;

		if (!string.IsNullOrEmpty(defaultVisibility))
		{
			CodeBuilder.Append($"{defaultVisibility} ", indent: false);
			// For the space character.
			VisibilityLength++;
		}
	}

	public void Visibility(string visibility)
	{
		if (string.IsNullOrEmpty(visibility))
		{
			// 1 for space.
			int count = VisibilityModifier.Length + 1;
				
			CodeBuilder.Remove(VisibilityOffset, count);
			VisibilityLength -= count;
				
			return;
		}
			
		CodeBuilder.Overwrite(visibility, VisibilityOffset, VisibilityModifier.Length);

		VisibilityLength -= VisibilityModifier.Length;
		VisibilityLength += visibility.Length;

		VisibilityModifier = visibility;
	}

	public void AddTypeParameter(string type)
	{
		// Insert just after name.
		int insertIndex = TypeParameterStartOffset;

		if (!_hasTypeParameters)
		{
			CodeBuilder.Insert("<>", insertIndex);

			TypeParameterLength += "<>".Length;
			insertIndex += "<".Length;
		}
		else
		{
			insertIndex += TypeParameterLength;
			insertIndex -= ">".Length;

			CodeBuilder.Insert(", ", insertIndex);

			insertIndex += ", ".Length;
			TypeParameterLength += ", ".Length;
		}

		CodeBuilder.Insert(type, insertIndex);
		TypeParameterLength += type.Length;

		_hasTypeParameters = true;
	}

	public void AddWhereClause(string where)
	{
		int insertIndex = WhereClauseStartOffset + WhereClauseLength;
		string whereClause = $" where {where}";

		CodeBuilder.Insert(whereClause, insertIndex);

		WhereClauseLength += whereClause.Length;
	}

	public void SetStatic()
	{
		CodeBuilder.Insert("static ", StaticOffset);
		StaticLength = "static ".Length;
	}

	public void SetReadOnly()
	{
		CodeBuilder.Insert("readonly ", ReadOnlyOffset);
		ReadOnlyLength = "readonly ".Length;
	}

	public void SetPartial()
	{
		CodeBuilder.Insert("partial ", PartialOffset);
		PartialLength = "partial ".Length;
	}

	public void SetAsync()
	{
		CodeBuilder.Insert("async ", AsyncOffset);
		AsyncLength = "async ".Length;
	}
}