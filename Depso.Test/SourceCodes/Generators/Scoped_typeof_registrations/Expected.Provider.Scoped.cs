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

        private global::Service1? _service1_0;
        private global::Service1 Service1_0 => _service1_0 ??= CreateService1_0();

        private global::Service1? _service1_1;
        private global::Service1 Service1_1 => _service1_1 ??= CreateService1_0();

        private global::Service2? _service2_0;
        private global::Service2 Service2_0 => _service2_0 ??= CreateService2_0();

        private global::Service2? _service2_1;
        private global::Service2 Service2_1 => _service2_1 ??= CreateService2_0();

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
            if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::System.IServiceProvider)) return this;
            if (serviceType == typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope)) return this;
            if (serviceType == typeof(global::Interface1)) return Service1_0;
            if (serviceType == typeof(global::Service1)) return Service1_0;
            if (serviceType == typeof(global::InterfaceA)) return Service1_1;
            if (serviceType == typeof(global::Service2)) return Service2_1;
            if (serviceType == typeof(global::InterfaceB)) return Service2_0;
            if (serviceType == typeof(global::Interface2)) return Service2_0;

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
                return new global::Service1();
            }
        }

        private global::Service2 CreateService2_0()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                return new global::Service2();
            }
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

            if (_service1_0 != null) _service1_0.Dispose();
            if (_service1_1 != null) _service1_1.Dispose();
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

            if (_service1_0 != null) _service1_0.Dispose();
            if (_service1_1 != null) _service1_1.Dispose();
            if (_service2_0 != null) await _service2_0.DisposeAsync();
            if (_service2_1 != null) await _service2_1.DisposeAsync();
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