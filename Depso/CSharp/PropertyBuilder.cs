namespace Depso.CSharp;

public class PropertyBuilder : ModifierBuilder
{
	protected int DefinitionLength { get; set; }
	protected int DefinitionEndOffset => ModifierEndOffset + DefinitionLength;

	public PropertyBuilder(CodeBuilder codeBuilder, string type, string name) : base(codeBuilder, "public")
	{
		string definition = $"{type} {name} {{ get; }}";
			
		CodeBuilder.AppendLine(definition, indent: false);

		DefinitionLength = definition.Length;
	}

	public PropertyBuilder Initializer(string initializer)
	{
		string code = $" = {initializer};";
			
		CodeBuilder.Insert(code, DefinitionEndOffset);
		DefinitionLength += code.Length;

		return this;
	}
}