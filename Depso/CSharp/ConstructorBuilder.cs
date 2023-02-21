namespace Depso.CSharp;

public class ConstructorBuilder : MethodBaseBuilder
{
	public ConstructorBuilder(CodeBuilder codeBuilder, string type) : base(codeBuilder, $"{type}", "public")
	{
	}

	public ConstructorBuilder Static()
	{
		SetStatic();
		Visibility("");
			
		return this;
	}
}