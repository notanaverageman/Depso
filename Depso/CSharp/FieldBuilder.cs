namespace Depso.CSharp;

public class FieldBuilder : ModifierBuilder
{
	public FieldBuilder(CodeBuilder codeBuilder, string type, string name) : base(codeBuilder, "private")
	{
		string definition = $"{type} {name};";
			
		CodeBuilder.AppendLine(definition, indent: false);
	}
}