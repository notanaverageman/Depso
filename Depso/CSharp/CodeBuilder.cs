using System.Text;

namespace Depso.CSharp;

public class CodeBuilder
{
	public const string NewLine = "\n";

	private readonly StringBuilder _builder;

	public int IndentationLevel { get; private set; }
	public int SpacesPerIndentation => 4;

	public int CurrentOffset => _builder.Length;
	public string CurrentIndentation { get; private set; }

	public CodeBuilder()
	{
		_builder = new StringBuilder();

		CurrentIndentation = "";
		IndentationLevel = 0;
	}

	public int Append(string text, bool indent = true)
	{
		int count = text.Length;
			
		if (indent)
		{
			_builder.Append(CurrentIndentation);
			count += CurrentIndentation.Length;
		}
			
		_builder.Append(text);

		return count;
	}

	public int AppendLine(string text, bool indent = true)
	{
		int count = text.Length + NewLine.Length;

		if (indent)
		{
			_builder.Append(CurrentIndentation);
			count += CurrentIndentation.Length;
		}
			
		_builder.AppendLine(text);

		return count;
	}

	public int AppendLine()
	{
		_builder.AppendLine();
		return NewLine.Length;
	}

	public int Insert(string text, int offset, bool indent = false)
	{
		int count = text.Length;

		if (indent)
		{
			_builder.Insert(offset, CurrentIndentation);
			count += CurrentIndentation.Length;

			offset += CurrentIndentation.Length;
		}

		_builder.Insert(offset, text);

		return count;
	}

	public void Remove(int offset, int length)
	{
		_builder.Remove(offset, length);
	}

	public void Overwrite(string text, int offset, int length)
	{
		_builder.Remove(offset, length);
		_builder.Insert(offset, text);
	}

	public void Clear()
	{
		_builder.Clear();
	}

	public void Indent()
	{
		IndentationLevel++;
		CurrentIndentation = new string(' ', IndentationLevel * SpacesPerIndentation);
	}

	public void Unindent()
	{
		IndentationLevel--;
		CurrentIndentation = new string(' ', IndentationLevel * SpacesPerIndentation);
	}

	public override string ToString()
	{
		return _builder.ToString();
	}

	public void Comment(string comment)
	{
		AppendLine($"// {comment}");
	}

	public void Using(string @using)
	{
		AppendLine($"using {@using};");
	}

	public NamespaceBuilder Namespace(string @namespace)
	{
		return new(this, @namespace);
	}

	public StructBuilder Struct(string @struct)
	{
		return new(this, @struct);
	}

	public ClassBuilder Class(string @class)
	{
		return new(this, @class);
	}

	public FieldBuilder Field(string type, string name)
	{
		return new(this, type, name);
	}

	public PropertyBuilder Property(string type, string name)
	{
		return new(this, type, name);
	}

	public ConstructorBuilder Constructor(string type)
	{
		return new(this, type);
	}

	public MethodBuilder Method(string returnType, string name)
	{
		return new(this, returnType, name);
	}

	public LocalMethodBuilder LocalMethod(string returnType, string name)
	{
		return new(this, returnType, name);
	}

	public ForEachBuilder ForEach(string type, string loopParameter, string collection)
	{
		return new(this, type, loopParameter, collection);
	}

	public ForBuilder For(
		string compareTo,
		string type = "int",
		string loopParameter = "i",
		string initialValue = "0")
	{
		return new(this, type, loopParameter, initialValue, "<", compareTo, "++");
	}

	public ForBuilder ForReverse(
		string initialValue,
		string type = "int",
		string loopParameter = "i",
		string compareTo = "0")
	{
		return new(this, type, loopParameter, initialValue, ">=", compareTo, "--");
	}

	public IfBuilder If(string condition)
	{
		return new(this, condition);
	}

	public ElseIfBuilder ElseIf(string condition)
	{
		return new(this, condition);
	}

	public ElseBuilder Else()
	{
		return new(this);
	}

	public TryBuilder Try()
	{
		return new(this);
	}

	public CatchBuilder Catch(string? exception)
	{
		return new(this, exception);
	}

	public FinallyBuilder Finally()
	{
		return new(this);
	}

	public SwitchBuilder Switch(string switchOn)
	{
		return new(this, switchOn);
	}

	public CaseBuilder Case(string label, bool addBraces = false)
	{
		return new(this, label, addBraces);
	}

	public DefaultBuilder Default(bool addBraces = false)
	{
		return new(this, addBraces);
	}

	public LockBuilder Lock(string lockObject)
	{
		return new(this, lockObject);
	}

	public InitializerBuilder Initializer()
	{
		return new(this, false);
	}

	public InitializerBuilder NestedInitializer()
	{
		return new(this, true);
	}
}