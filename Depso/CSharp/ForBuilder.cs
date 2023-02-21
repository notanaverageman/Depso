namespace Depso.CSharp;

public class ForBuilder : BlockBuilder
{
	public ForBuilder(
		CodeBuilder codeBuilder,
		string type,
		string loopParameter,
		string initialValue,
		string comparisonOperator,
		string compareTo,
		string postIterationOperator)
		:
		base(codeBuilder)
	{
		CodeBuilder.AppendLine($"for ({type} {loopParameter} = {initialValue}; {loopParameter} {comparisonOperator} {compareTo}; {loopParameter}{postIterationOperator})");
		CodeBuilder.AppendLine("{");
		CodeBuilder.Indent();
	}
}