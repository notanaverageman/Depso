namespace Depso.CSharp;

public class ElseIfBuilder : BlockBuilder
{
	public ElseIfBuilder(CodeBuilder codeBuilder, string condition) : base(codeBuilder)
	{
		CodeBuilder.AppendLine($"else if ({condition})");
		CodeBuilder.AppendLine("{");
		CodeBuilder.Indent();
	}
}