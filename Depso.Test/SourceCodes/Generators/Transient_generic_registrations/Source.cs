using Depso;
using System;
using System.Threading.Tasks;

public interface Interface1 : IDisposable { }
public interface Interface2 : IAsyncDisposable { }
public interface Interface3 { }
public interface Interface4 { }
public interface InterfaceA { }
public interface InterfaceB { }

public class Service1 : Interface1, InterfaceA
{
    public void Dispose()
    {
    }
}

public class Service2 : Interface2, InterfaceB
{
    public ValueTask DisposeAsync()
    {
        return default;
    }
}

public class Service3 : Interface3
{
    public Service3(Interface1 i1)
    {
    }
}

public class Service4 : Interface4
{
    public Service4(Interface1 i1, InterfaceA iA, Interface2 i2, Interface3 i3, Service1 s1, Service2 s2)
    {
    }
}

[ServiceProvider]
public partial class Provider
{
    private void RegisterServices()
    {
        AddTransient<Interface1, Service1>().AlsoAsSelf();
        AddTransient<InterfaceA, Service1>();
        AddTransient<Service2>().AlsoAs<Interface2>().AlsoAs<InterfaceB>();
        AddTransient<Service2>();
        AddTransient<Service3>().AlsoAs<Interface3>();
        AddTransient<Service4>().AlsoAs<Interface4>();
    }
}