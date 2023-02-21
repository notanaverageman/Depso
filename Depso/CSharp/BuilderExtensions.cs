namespace Depso.CSharp;

public static class ModifierExtensions
{
	public static T Public<T>(this T builder) where T : ModifierBuilder
	{
		builder.Visibility("public");
		return builder;
	}

	public static T Internal<T>(this T builder) where T : ModifierBuilder
	{
		builder.Visibility("internal");
		return builder;
	}

	public static T Protected<T>(this T builder) where T : ModifierBuilder
	{
		builder.Visibility("protected");
		return builder;
	}

	public static T Private<T>(this T builder) where T : ModifierBuilder
	{
		builder.Visibility("private");
		return builder;
	}
		
	public static T Static<T>(this T builder) where T : ModifierBuilder
	{
		builder.SetStatic();
		return builder;
	}
		
	public static T ReadOnly<T>(this T builder) where T : ModifierBuilder
	{
		builder.SetReadOnly();
		return builder;
	}
		
	public static T Partial<T>(this T builder) where T : ModifierBuilder
	{
		builder.SetPartial();
		return builder;
	}
		
	public static T Async<T>(this T builder) where T : ModifierBuilder
	{
		builder.SetAsync();
		return builder;
	}

	public static T Parameter<T>(
		this T builder,
		string type,
		string name,
		bool onNewLine = false) where T : MethodBaseBuilder
	{
		builder.AddParameter(type, name, onNewLine);
		return builder;
	}

	public static T Base<T>(this T builder, string @base) where T : TypeBuilder
	{
		builder.AddBase(@base);
		return builder;
	}
}