﻿// <auto-generated/>

#nullable enable

public partial class Provider
    :
    global::System.IDisposable,
    global::System.IAsyncDisposable,
    global::System.IServiceProvider
{
    private readonly object _sync = new object();

    private global::Provider.Scope? _rootScope;
    private global::Provider.Scope RootScope => _rootScope ??= CreateScope(_sync);

    private bool _isDisposed;
    private global::System.Collections.Generic.List<global::System.IDisposable>? _transientDisposables;
    private global::System.Collections.Generic.List<global::System.IAsyncDisposable>? _transientAsyncDisposables;

    private global::Singleton1? _singleton1_0;
    private global::Singleton1 Singleton1_0 => _singleton1_0 ??= CreateSingleton1();

    private global::Singleton2? _singleton2_0;
    private global::Singleton2 Singleton2_0 => _singleton2_0 ??= CreateSingleton2();

    private global::Singleton3? _singleton3_0;
    private global::Singleton3 Singleton3_0 => _singleton3_0 ??= CreateSingleton3();

    private global::Singleton4? _singleton4_0;
    private global::Singleton4 Singleton4_0 => _singleton4_0 ??= CreateSingleton4();

    public object? GetService(global::System.Type serviceType)
    {
        if (serviceType == typeof(global::Singleton1)) return Singleton1_0;
        if (serviceType == typeof(global::SingletonInterface1)) return Singleton1_0;
        if (serviceType == typeof(global::Singleton2)) return Singleton2_0;
        if (serviceType == typeof(global::SingletonInterface2)) return Singleton2_0;
        if (serviceType == typeof(global::Singleton3)) return Singleton3_0;
        if (serviceType == typeof(global::Singleton4)) return Singleton4_0;
        if (serviceType == typeof(global::Scoped1)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::ScopedInterface1)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::Scoped2)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::ScopedInterface2)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::Scoped3)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::Scoped4)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::Transient1)) return CreateTransient1AddDisposable();
        if (serviceType == typeof(global::TransientInterface1)) return CreateTransient1AddDisposable();
        if (serviceType == typeof(global::Transient2)) return CreateTransient2AddDisposable();
        if (serviceType == typeof(global::TransientInterface2)) return CreateTransient2AddDisposable();
        if (serviceType == typeof(global::Transient3)) return CreateTransient3AddDisposable();
        if (serviceType == typeof(global::Transient4)) return CreateTransient4AddDisposable();

        return null;
    }

    private T GetService<T>()
    {
        return (T)GetService(typeof(T))!;
    }

    private global::Singleton1 CreateSingleton1()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Singleton1();
        }
    }

    private global::Singleton2 CreateSingleton2()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Singleton2(GetService<global::SingletonInterface1>());
        }
    }

    private global::Singleton3 CreateSingleton3()
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            return new global::Singleton3(
                GetService<global::Singleton1>(),
                GetService<global::Singleton4>()
            );
        }
    }

    private global::Singleton4 CreateSingleton4()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Singleton4(GetService<global::Singleton2>());
        }
    }

    private global::Transient1 CreateTransient1()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Transient1();
        }
    }

    private global::Transient1 CreateTransient1AddDisposable()
    {
        global::Transient1 service = CreateTransient1();
        AddDisposable(service);

        return service;
    }

    private global::Transient2 CreateTransient2()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Transient2(GetService<global::TransientInterface1>());
        }
    }

    private global::Transient2 CreateTransient2AddDisposable()
    {
        global::Transient2 service = CreateTransient2();
        AddAsyncDisposable(service);

        return service;
    }

    private global::Transient3 CreateTransient3()
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            return new global::Transient3(
                GetService<global::Transient1>(),
                GetService<global::Transient4>()
            );
        }
    }

    private global::Transient3 CreateTransient3AddDisposable()
    {
        global::Transient3 service = CreateTransient3();
        AddDisposable(service);

        return service;
    }

    private global::Transient4 CreateTransient4()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Transient4(GetService<global::Transient2>());
        }
    }

    private global::Transient4 CreateTransient4AddDisposable()
    {
        global::Transient4 service = CreateTransient4();
        AddAsyncDisposable(service);

        return service;
    }

    private global::Provider.Scope CreateScope(object? sync)
    {
        ThrowIfDisposed();
        return new global::Provider.Scope(this, sync);
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
        }

        if (_transientDisposables != null)
        {
            for (int i = _transientDisposables.Count - 1; i >= 0; i--)
            {
                _transientDisposables[i].Dispose();
            }
        }

        if (_rootScope != null) _rootScope.Dispose();

        if (_singleton3_0 != null) _singleton3_0.Dispose();
        if (_singleton1_0 != null) _singleton1_0.Dispose();
    }

    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        lock (_sync)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
        }

        if (_transientDisposables != null)
        {
            for (int i = _transientDisposables.Count - 1; i >= 0; i--)
            {
                _transientDisposables[i].Dispose();
            }
        }

        if (_transientAsyncDisposables != null)
        {
            for (int i = _transientAsyncDisposables.Count - 1; i >= 0; i--)
            {
                await _transientAsyncDisposables[i].DisposeAsync();
            }
        }

        if (_rootScope != null) await _rootScope.DisposeAsync();

        if (_singleton3_0 != null) _singleton3_0.Dispose();
        if (_singleton4_0 != null) await _singleton4_0.DisposeAsync();
        if (_singleton2_0 != null) await _singleton2_0.DisposeAsync();
        if (_singleton1_0 != null) _singleton1_0.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new global::System.ObjectDisposedException("Provider");
        }
    }

    private void AddDisposable(global::System.IDisposable disposable)
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            if (_transientDisposables == null)
            {
                _transientDisposables = new global::System.Collections.Generic.List<global::System.IDisposable>();
            }

            _transientDisposables.Add(disposable);
        }
    }

    private void AddAsyncDisposable(global::System.IAsyncDisposable disposable)
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            if (_transientAsyncDisposables == null)
            {
                _transientAsyncDisposables = new global::System.Collections.Generic.List<global::System.IAsyncDisposable>();
            }

            _transientAsyncDisposables.Add(disposable);
        }
    }
}