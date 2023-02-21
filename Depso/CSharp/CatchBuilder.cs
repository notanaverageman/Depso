namespace Depso.CSharp;

public class CatchBuilder : BlockBuilder
{
	public CatchBuilder(CodeBuilder codeBuilder, string? exception) : base(codeBuilder)
	{
		string @catch = string.IsNullOrEmpty(exception)
			? "catch"
			: $"catch ({exception})";

		CodeBuilder.AppendLine(@catch);
		CodeBuilder.AppendLine("{");
		CodeBuilder.Indent();
	}
}