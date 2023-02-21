namespace Depso;

public static class StringExtensions
{
	public static string ToPascalCase(this string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}

		if (char.ToUpperInvariant(text[0]) != text[0])
		{
			return char.ToUpperInvariant(text[0]) + text.Substring(1);
		}

		return text;
	}

	public static string ToCamelCase(this string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}

		if (char.ToLowerInvariant(text[0]) != text[0])
		{
			return char.ToLowerInvariant(text[0]) + text.Substring(1);
		}

		return text;
	}

	public static string ToPropertyName(this string fieldName)
	{
		return fieldName.TrimStart('_').ToPascalCase();
	}
	
	public static string ToFactoryMethodName(this string propertyName)
	{
		return $"Factory{propertyName}";
	}
	
	public static string WithGlobalPrefix(this string value)
	{
		return $"global::{value}";
	}
}