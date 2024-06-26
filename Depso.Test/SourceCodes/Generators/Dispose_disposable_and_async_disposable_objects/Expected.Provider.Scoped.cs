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
        private global::System.Collections.Generic.List<global::System.IDisposable>? _transientDisposables;
        private global::System.Collections.Generic.List<global::System.IAsyncDisposable>? _transientAsyncDisposables;

        private global::Scoped1? _scoped1_0;
        private global::Scoped1 Scoped1_0 => _scoped1_0 ??= CreateScoped1_0();

        private global::Scoped2? _scoped2_0;
        private global::Scoped2 Scoped2_0 => _scoped2_0 ??= CreateScoped2_0();

        private global::Scoped3? _scoped3_0;
        private global::Scoped3 Scoped3_0 => _scoped3_0 ??= CreateScoped3_0();

        private global::Scoped4? _scoped4_0;
        private global::Scoped4 Scoped4_0 => _scoped4_0 ??= CreateScoped4_0();

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
            if (serviceType == typeof(global::Singleton1)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::SingletonInterface1)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::Singleton2)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::SingletonInterface2)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::Singleton3)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::Singleton4)) return _root.GetService(serviceType);
            if (serviceType == typeof(global::Scoped1)) return Scoped1_0;
            if (serviceType == typeof(global::ScopedInterface1)) return Scoped1_0;
            if (serviceType == typeof(global::Scoped2)) return Scoped2_0;
            if (serviceType == typeof(global::ScopedInterface2)) return Scoped2_0;
            if (serviceType == typeof(global::Scoped3)) return Scoped3_0;
            if (serviceType == typeof(global::Scoped4)) return Scoped4_0;
            if (serviceType == typeof(global::Transient1)) return CreateTransient1_0AddDisposable();
            if (serviceType == typeof(global::TransientInterface1)) return CreateTransient1_0AddDisposable();
            if (serviceType == typeof(global::Transient2)) return CreateTransient2_0AddDisposable();
            if (serviceType == typeof(global::TransientInterface2)) return CreateTransient2_0AddDisposable();
            if (serviceType == typeof(global::Transient3)) return CreateTransient3_0AddDisposable();
            if (serviceType == typeof(global::Transient4)) return CreateTransient4_0AddDisposable();
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

        private global::Scoped1 CreateScoped1_0()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                return new global::Scoped1();
            }
        }

        private global::Scoped2 CreateScoped2_0()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                return new global::Scoped2(GetService<global::ScopedInterface1>());
            }
        }

        private global::Scoped3 CreateScoped3_0()
        {
            lock (_sync)
            {
                ThrowIfDisposed();

                return new global::Scoped3(
                    GetService<global::Scoped1>(),
                    GetService<global::Scoped4>()
                );
            }
        }

        private global::Scoped4 CreateScoped4_0()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                return new global::Scoped4(GetService<global::Scoped2>());
            }
        }

        private global::Transient1 CreateTransient1_0AddDisposable()
        {
            global::Transient1 service = _root.CreateTransient1_0();
            AddDisposable(service);

            return service;
        }

        private global::Transient2 CreateTransient2_0AddDisposable()
        {
            global::Transient2 service = _root.CreateTransient2_0();
            AddAsyncDisposable(service);

            return service;
        }

        private global::Transient3 CreateTransient3_0AddDisposable()
        {
            global::Transient3 service = _root.CreateTransient3_0();
            AddDisposable(service);

            return service;
        }

        private global::Transient4 CreateTransient4_0AddDisposable()
        {
            global::Transient4 service = _root.CreateTransient4_0();
            AddAsyncDisposable(service);

            return service;
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

            if (_scoped3_0 != null) _scoped3_0.Dispose();
            if (_scoped1_0 != null) _scoped1_0.Dispose();
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

            if (_scoped3_0 != null) _scoped3_0.Dispose();
            if (_scoped4_0 != null) await _scoped4_0.DisposeAsync();
            if (_scoped2_0 != null) await _scoped2_0.DisposeAsync();
            if (_scoped1_0 != null) _scoped1_0.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new global::System.ObjectDisposedException("Provider.Scope");
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
}