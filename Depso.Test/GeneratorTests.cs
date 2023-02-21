using NUnit.Framework;

namespace Depso.Test;

public class GeneratorTests : TestBase
{
	[Test]
	public void Empty()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Singleton_factory_registrations()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Singleton_generic_registrations()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Singleton_typeof_registrations()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Scoped_factory_registrations()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Scoped_generic_registrations()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Scoped_typeof_registrations()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Transient_factory_registrations()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Transient_generic_registrations()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Transient_typeof_registrations()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Duplicate_registration_should_generate_correct_code()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Select_longest_available_constructor()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Ignore_optional_parameter_when_unavailable()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Pass_optional_parameter_when_available()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Dispose_disposable_and_async_disposable_objects()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Resolve_ienumerable()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}

	[Test]
	public void Resolve_ienumerable_even_if_not_registered()
	{
		GenerateAndCheck<ServiceProviderGenerator>();
	}
}