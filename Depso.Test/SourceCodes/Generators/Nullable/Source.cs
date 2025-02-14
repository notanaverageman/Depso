using Depso;
using System;

public interface Interface1 : IDisposable;
public class Service1 : Interface1
{
    public void Dispose()
    {
    }
}

public class Service2
{
    public Service2(Interface1 i1, Interface1? i2)
    {
    }
}

[ServiceProvider]
public partial class Provider
{
    private void RegisterServices()
    {
        AddSingleton<Interface1, Service1>().AlsoAsSelf().AlsoAs<Interface1?>();
        AddSingleton<Service2>();
    }
}