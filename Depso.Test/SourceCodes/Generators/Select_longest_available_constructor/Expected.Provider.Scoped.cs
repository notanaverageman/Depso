﻿// <auto-generated/>

#nullable enable

public partial class Provider
{
    public class Scope
        :
        global::System.IDisposable,
        global::System.IAsyncDisposable,
        global::System.IServiceProvider,
        global::Microsoft.Extensions.DependencyInjection.IServiceScope
    {
        private readonly object _sync = new object();
        private readonly global::Provider _root;

        private bool _isDisposed;

        global::System.IServiceProvider global::Microsoft.Extensions.DependencyInjection.IServiceScope.ServiceProvider => this;

        public Scope(global::Provider root, object? sync)
        {
            _root = root;

            if (sync != null)
            {
                _sync = sync;
            }
        }

        public object? GetService(global::System.Type serviceType)
        {
            if (serviceType == typeof(global::Service)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::Dependency1)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::Dependency2)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::System.IServiceProvider)) return this;
            if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope)) return this;

            return null;
        }

        private T GetService<T>()
        {
            return (T)GetService(typeof(T))!;
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
        }

        public global::System.Threading.Tasks.ValueTask DisposeAsync()
        {
            lock (_sync)
            {
                if (_isDisposed)
                {
                    return default;
                }

                _isDisposed = true;
            }

            return default;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new global::System.ObjectDisposedException("Provider.Scope");
            }
        }
    }
}