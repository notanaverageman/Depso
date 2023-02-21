namespace Depso;

public abstract class AttributeInfo
{
	private string? _sourceCode;

	public string Name { get; }
	public string FullName { get; }
	public string NameWithAttribute => $"{Name}Attribute";
	public string SourceCode => _sourceCode ??= GetSourceCode();

	public AttributeInfo(string name)
	{
		Name = name;
		FullName = $"{Constants.GeneratorNamespace}.{NameWithAttribute}";
	}

	protected abstract string GetSourceCode();
}