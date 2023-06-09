﻿namespace Depso;

public class ServiceProviderModuleAttributeInfo : AttributeInfo
{
	public ServiceProviderModuleAttributeInfo() : base("ServiceProviderModule")
	{
	}

	protected override string GetSourceCode()
	{
		return $$"""
			// <auto-generated/>
			
			#nullable enable

			namespace {{Constants.GeneratorNamespace}}
			{
			    [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
			    internal sealed class {{NameWithAttribute}} : global::System.Attribute
			    {
			    }
			}
			""".Trim();
	}
}