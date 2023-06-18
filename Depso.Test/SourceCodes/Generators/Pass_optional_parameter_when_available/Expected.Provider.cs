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

    private global::Dependency1? _dependency1_0;
    private global::Dependency1 Dependency1_0 => _dependency1_0 ??= CreateDependency1_0();

    private global::Dependency2? _dependency2_0;
    private global::Dependency2 Dependency2_0 => _dependency2_0 ??= CreateDependency2_0();

    private global::Service? _service_0;
    private global::Service Service_0 => _service_0 ??= CreateService_0();

    global::Microsoft.Extensions.DependencyInjection.IServiceScope global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory.CreateScope() => this.CreateScope(_sync);

    public object? GetService(global::System.Type serviceType)
    {
        if (serviceType == typeof(global::System.IServiceProvider)) return this;
        if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory)) return this;
        if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService)) return this;
        if (serviceType == typeof(global::Dependency1)) return Dependency1_0;
        if (serviceType == typeof(global::Dependency2)) return Dependency2_0;
        if (serviceType == typeof(global::Service)) return Service_0;
        if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope)) return RootScope.GetService(serviceType);

        return null;
    }

    private T GetService<T>()
    {
        return (T)GetService(typeof(T))!;
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

    private global::Service CreateService_0()
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            return new global::Service(
                GetService<global::Dependency1>(),
                GetService<global::Dependency2>()
            );
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
            || serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService)
            || serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope)
            || serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory)
            || serviceType == typeof(global::Service)
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