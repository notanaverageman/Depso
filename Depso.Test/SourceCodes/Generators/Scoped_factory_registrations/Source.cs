using Depso;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UsingStatic = System.Collections.Generic.List<System.Action>;
using static StaticClass;

public interface Interface1 : IDisposable { }
public interface Interface2 : IAsyncDisposable { }
public interface Interface3 { }
public interface Interface4 { }
public interface InterfaceA : IDisposable { }
public interface InterfaceB { }
public interface InterfaceC { }

public class Service1 : Interface1, InterfaceA
{
    public void Dispose()
    {
    }
}

public class Service2 : Interface2, InterfaceB, InterfaceC
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
    private readonly Func<IServiceProvider, InterfaceA> Func = (IServiceProvider _) => new Service1();

    private void RegisterServices()
    {
        AddScoped(x =>
        {
            return new Service1();
        }).AlsoAs(typeof(Interface1)).AlsoAs(typeof(InterfaceA));
        AddScoped(x => new System.Exception());
        AddScoped(x => new List<IDisposable>());
        AddScoped<InterfaceC>(x => (InterfaceC)x.GetService(typeof(Service2)));
        AddScoped(Func);
        AddScoped(GetService2).AlsoAs<Interface2>().AlsoAs<InterfaceB>();
        AddScoped(x => new UsingStatic());
        AddScoped<Service2>(StaticClass.StaticFunc);
        AddScoped<Service2>(StaticFunc2);
    }

    private Service2 GetService2(IServiceProvider serviceProvider)
    {
        return new Service2();
    }
}

public class StaticClass
{
    public static readonly Func<IServiceProvider, Service2> StaticFunc = _ => new Service2();
    public static Func<IServiceProvider, Service2> StaticFunc2 => _ => new Service2();
}