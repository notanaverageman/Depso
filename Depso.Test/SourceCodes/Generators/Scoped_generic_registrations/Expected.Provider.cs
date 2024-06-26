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

    global::Microsoft.Extensions.DependencyInjection.IServiceScope global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory.CreateScope() => this.CreateScope(_sync);

    public object? GetService(global::System.Type serviceType)
    {
        if (serviceType == typeof(global::Interface1)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::Service1)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::InterfaceA)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::Service2)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::InterfaceB)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::Interface2)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::System.IServiceProvider)) return this;
        if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory)) return this;
        if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService)) return this;
        if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope)) return RootScope.GetService(serviceType);

        return null;
    }

    private T GetService<T>()
    {
        return (T)GetService(typeof(T))!;
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
            || serviceType == typeof(global::Interface1)
            || serviceType == typeof(global::Interface2)
            || serviceType == typeof(global::InterfaceA)
            || serviceType == typeof(global::InterfaceB)
            || serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService)
            || serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope)
            || serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory)
            || serviceType == typeof(global::Service1)
            || serviceType == typeof(global::Service2)
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