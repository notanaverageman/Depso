namespace Depso.CSharp;

public class InitializerBuilder : BlockBuilder
{
	private readonly bool _isNested;

	public InitializerBuilder(CodeBuilder codeBuilder, bool isNested) : base(codeBuilder, false)
	{
		_isNested = isNested;

		CodeBuilder.AppendLine("{");
		CodeBuilder.Indent();
	}

	public override void Dispose()
	{
		base.Dispose();
			
		string text = _isNested ? "," : ";";
		CodeBuilder.AppendLine(text, indent: false);
	}
}