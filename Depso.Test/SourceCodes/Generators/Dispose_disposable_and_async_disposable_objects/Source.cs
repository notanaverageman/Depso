using Depso;
using System;
using System.Threading.Tasks;

public interface SingletonInterface1 : IDisposable { }
public interface SingletonInterface2 : IAsyncDisposable { }
public interface ScopedInterface1 : IDisposable { }
public interface ScopedInterface2 : IAsyncDisposable { }
public interface TransientInterface1 : IDisposable { }
public interface TransientInterface2 : IAsyncDisposable { }

public class Singleton1 : SingletonInterface1
{
    public void Dispose()
    {
    }
}

public class Singleton2 : SingletonInterface2
{
    public Singleton2(SingletonInterface1 i1)
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

public class Singleton3 : IDisposable
{
    public Singleton3(Singleton1 s1, Singleton4 s4)
    {
    }

    public void Dispose()
    {
    }
}

public class Singleton4 : IAsyncDisposable
{
    public Singleton4(Singleton2 s2)
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

public class Scoped1 : ScopedInterface1
{
    public void Dispose()
    {
    }
}

public class Scoped2 : ScopedInterface2
{
    public Scoped2(ScopedInterface1 i1)
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

public class Scoped3 : IDisposable
{
    public Scoped3(Scoped1 s1, Scoped4 s4)
    {
    }

    public void Dispose()
    {
    }
}

public class Scoped4 : IAsyncDisposable
{
    public Scoped4(Scoped2 s2)
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

public class Transient1 : TransientInterface1
{
    public void Dispose()
    {
    }
}

public class Transient2 : TransientInterface2
{
    public Transient2(TransientInterface1 i1)
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

public class Transient3 : IDisposable
{
    public Transient3(Transient1 t1, Transient4 t4)
    {
    }

    public void Dispose()
    {
    }
}

public class Transient4 : IAsyncDisposable
{
    public Transient4(Transient2 t2)
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

[ServiceProvider]
public partial class Provider
{
    private void RegisterServices()
    {
        AddSingleton<Singleton1>().AlsoAs<SingletonInterface1>();
        AddSingleton<Singleton2>().AlsoAs<SingletonInterface2>();
        AddSingleton<Singleton3>();
        AddSingleton<Singleton4>();

        AddScoped<Scoped1>().AlsoAs<ScopedInterface1>();
        AddScoped<Scoped2>().AlsoAs<ScopedInterface2>();
        AddScoped<Scoped3>();
        AddScoped<Scoped4>();

        AddTransient<Transient1>().AlsoAs<TransientInterface1>();
        AddTransient<Transient2>().AlsoAs<TransientInterface2>();
        AddTransient<Transient3>();
        AddTransient<Transient4>();
    }
}