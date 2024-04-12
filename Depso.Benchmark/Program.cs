using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Jab;
using Microsoft.Extensions.DependencyInjection;

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);

internal interface IService;
internal class Service: IService;

[Depso.ServiceProvider]
internal partial class DepsoContainer
{
	private void RegisterServices()
	{
		AddTransient<IService, Service>();
	}
}

[Jab.ServiceProvider]
[Transient(typeof(IService), typeof(Service))]
internal partial class JabContainer
{
}

[MemoryDiagnoser]
public class GetService
{
	private readonly IServiceProvider _provider;
	private readonly IServiceProvider _jabContainer;
	private readonly IServiceProvider _depsoContainer;

	public GetService()
	{
		ServiceCollection serviceCollection = new();
		serviceCollection.AddTransient<IService, Service>();
		_provider = serviceCollection.BuildServiceProvider();
		_jabContainer = new JabContainer();
		_depsoContainer = new DepsoContainer();
	}

	[Benchmark]
	public void MEDI() => _provider.GetService<IService>();

	[Benchmark]
	public void Jab() => _jabContainer.GetService<IService>();

	[Benchmark(Baseline = true)]
	public void Depso() => _depsoContainer.GetService<IService>();
}

[MemoryDiagnoser]
public class StartupTime
{
	[Benchmark]
	public void MEDI()
	{
		ServiceCollection serviceCollection = new();
		serviceCollection.AddTransient<IService, Service>();

		IServiceProvider provider = serviceCollection.BuildServiceProvider();
		provider.GetService<IService>();
	}

	[Benchmark]
	public void Jab()
	{
		IServiceProvider provider = new JabContainer();
		provider.GetService<IService>();
	}

	[Benchmark(Baseline = true)]
	public void Depso()
	{
		IServiceProvider provider = new DepsoContainer();
		provider.GetService<IService>();
	}
}