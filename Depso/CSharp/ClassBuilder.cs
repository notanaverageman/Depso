namespace Depso.CSharp;

public class ClassBuilder : TypeBuilder
{
	public ClassBuilder(CodeBuilder codeBuilder, string name) : base(codeBuilder, "class", name)
	{
	}
}