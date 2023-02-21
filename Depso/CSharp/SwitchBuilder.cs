namespace Depso.CSharp;

public class SwitchBuilder : BlockBuilder
{
	public SwitchBuilder(CodeBuilder codeBuilder, string switchOn) : base(codeBuilder)
	{
		CodeBuilder.AppendLine($"switch ({switchOn})");
		CodeBuilder.AppendLine("{");
		CodeBuilder.Indent();
	}
}