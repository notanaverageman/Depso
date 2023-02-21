namespace Depso.CSharp;

public class LockBuilder : BlockBuilder
{
	public LockBuilder(CodeBuilder codeBuilder, string lockObject) : base(codeBuilder)
	{
		CodeBuilder.AppendLine($"lock ({lockObject})");
		CodeBuilder.AppendLine("{");
		CodeBuilder.Indent();
	}
}