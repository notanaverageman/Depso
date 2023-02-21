namespace Depso.CSharp;

public class ElseBuilder : BlockBuilder
{
	public ElseBuilder(CodeBuilder codeBuilder) : base(codeBuilder)
	{
		CodeBuilder.AppendLine("else");
		CodeBuilder.AppendLine("{");
		CodeBuilder.Indent();
	}
}