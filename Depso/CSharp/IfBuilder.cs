namespace Depso.CSharp;

public class IfBuilder : BlockBuilder
{
	public IfBuilder(CodeBuilder codeBuilder, string condition) : base(codeBuilder)
	{
		CodeBuilder.AppendLine($"if ({condition})");
		CodeBuilder.AppendLine("{");
		CodeBuilder.Indent();
	}
}