namespace Depso.CSharp;

public class ForEachBuilder : BlockBuilder
{
	public ForEachBuilder(
		CodeBuilder codeBuilder,
		string type,
		string loopParameter,
		string collection)
		:
		base(codeBuilder)
	{
		CodeBuilder.AppendLine($"foreach ({type} {loopParameter} in {collection})");
		CodeBuilder.AppendLine("{");
		CodeBuilder.Indent();
	}
}