﻿// <auto-generated/>

#nullable enable

public partial class Provider
    :
    global::System.IDisposable,
    global::System.IAsyncDisposable,
    global::System.IServiceProvider,
    global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory,
    global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService
{
    private readonly object _sync = new object();

    private global::Provider.Scope? _rootScope;
    private global::Provider.Scope RootScope => _rootScope ??= CreateScope(_sync);

    private bool _isDisposed;

    private global::Service1? _service1_0;
    private global::Service1 Service1_0 => _service1_0 ??= CreateService1_0();

    private global::Service1? _service1_1;
    private global::Service1 Service1_1 => _service1_1 ??= CreateService1_0();

    private global::Service2? _service2_0;
    private global::Service2 Service2_0 => _service2_0 ??= CreateService2_0();

    private global::Service2? _service2_1;
    private global::Service2 Service2_1 => _service2_1 ??= CreateService2_0();

    private global::Service3? _service3_0;
    private global::Service3 Service3_0 => _service3_0 ??= CreateService3_0();

    private global::Dependency1? _dependency1_0;
    private global::Dependency1 Dependency1_0 => _dependency1_0 ??= CreateDependency1_0();

    private global::Dependency1? _dependency1_1;
    private global::Dependency1 Dependency1_1 => _dependency1_1 ??= CreateDependency1_0();

    private global::Dependency1? _dependency1_2;
    private global::Dependency1 Dependency1_2 => _dependency1_2 ??= CreateDependency1_0();

    private global::Dependency2? _dependency2_0;
    private global::Dependency2 Dependency2_0 => _dependency2_0 ??= CreateDependency2_0();

    private global::Dependency2? _dependency2_1;
    private global::Dependency2 Dependency2_1 => _dependency2_1 ??= CreateDependency2_0();

    private global::Dependency3? _dependency3_0;
    private global::Dependency3 Dependency3_0 => _dependency3_0 ??= CreateDependency3_0();

    private global::Dependency3? _dependency3_1;
    private global::Dependency3 Dependency3_1 => _dependency3_1 ??= CreateDependency3_0();

    private global::Dependency4? _dependency4_0;
    private global::Dependency4 Dependency4_0 => _dependency4_0 ??= CreateDependency4_0();

    private global::Dependency4? _dependency4_1;
    private global::Dependency4 Dependency4_1 => _dependency4_1 ??= CreateDependency4_0();

    global::Microsoft.Extensions.DependencyInjection.IServiceScope global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory.CreateScope() => this.CreateScope(_sync);

    public object? GetService(global::System.Type serviceType)
    {
        if (serviceType == typeof(global::System.IServiceProvider)) return this;
        if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory)) return this;
        if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService)) return this;
        if (serviceType == typeof(global::Service1)) return Service1_1;
        if (serviceType == typeof(global::Service2)) return Service3_0;
        if (serviceType == typeof(global::Service3)) return Service3_0;
        if (serviceType == typeof(global::Interface1)) return Dependency1_1;
        if (serviceType == typeof(global::Dependency1)) return Dependency1_2;
        if (serviceType == typeof(global::Interface2)) return Dependency2_0;
        if (serviceType == typeof(global::Dependency2)) return Dependency2_1;
        if (serviceType == typeof(global::Dependency3)) return Dependency3_0;
        if (serviceType == typeof(global::Interface3)) return Dependency4_0;
        if (serviceType == typeof(global::Interface4)) return Dependency4_1;
        if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope)) return RootScope.GetService(serviceType);

        return null;
    }

    private T GetService<T>()
    {
        return (T)GetService(typeof(T))!;
    }

    private global::Service1 CreateService1_0()
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            return new global::Service1(
                GetService<global::Dependency1>(),
                GetService<global::Dependency2>(),
                GetService<global::Dependency3>()
            );
        }
    }

    private global::Service2 CreateService2_0()
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            return new global::Service2(
                GetService<global::Interface1>(),
                GetService<global::Interface2>(),
                GetService<global::Interface3>(),
                GetService<global::Interface4>()
            );
        }
    }

    private global::Service3 CreateService3_0()
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            return new global::Service3(
                GetService<global::Interface1>(),
                GetService<global::Interface2>(),
                GetService<global::Interface3>(),
                GetService<global::Interface4>()
            );
        }
    }

    private global::Dependency1 CreateDependency1_0()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Dependency1();
        }
    }

    private global::Dependency2 CreateDependency2_0()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Dependency2();
        }
    }

    private global::Dependency3 CreateDependency3_0()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Dependency3();
        }
    }

    private global::Dependency4 CreateDependency4_0()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Dependency4();
        }
    }

    private global::Provider.Scope CreateScope(object? sync)
    {
        ThrowIfDisposed();
        return new global::Provider.Scope(this, sync);
    }

    public bool IsService(global::System.Type serviceType)
    {
        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(global::System.Collections.Generic.IEnumerable<>))
        {
            serviceType = serviceType.GetGenericArguments()[0];
        }

        return false
            || serviceType == typeof(global::Dependency1)
            || serviceType == typeof(global::Dependency2)
            || serviceType == typeof(global::Dependency3)
            || serviceType == typeof(global::Interface1)
            || serviceType == typeof(global::Interface2)
            || serviceType == typeof(global::Interface3)
            || serviceType == typeof(global::Interface4)
            || serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService)
            || serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope)
            || serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory)
            || serviceType == typeof(global::Service1)
            || serviceType == typeof(global::Service2)
            || serviceType == typeof(global::Service3)
            || serviceType == typeof(global::System.IServiceProvider);
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

        if (_rootScope != null) _rootScope.Dispose();
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

        if (_rootScope != null) await _rootScope.DisposeAsync();
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new global::System.ObjectDisposedException("Provider");
        }
    }
}