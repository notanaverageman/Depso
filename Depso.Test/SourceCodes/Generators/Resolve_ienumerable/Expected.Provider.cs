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

    private global::Dependency1_A? _dependency1_A_0;
    private global::Dependency1_A Dependency1_A_0 => _dependency1_A_0 ??= CreateDependency1_A_0();

    private global::Dependency1_B? _dependency1_B_0;
    private global::Dependency1_B Dependency1_B_0 => _dependency1_B_0 ??= CreateDependency1_B_0();

    private global::Service? _service_0;
    private global::Service Service_0 => _service_0 ??= CreateService_0();

    private global::Dependency4? _dependency4_0;
    private global::Dependency4 Dependency4_0 => _dependency4_0 ??= CreateDependency4_0();

    private global::System.Collections.Generic.IEnumerable<global::Interface1>? _enumerableInterface1;
    private global::System.Collections.Generic.IEnumerable<global::Interface1> EnumerableInterface1 => _enumerableInterface1 ??= CreateEnumerableInterface1();

    private global::System.Collections.Generic.IEnumerable<global::Interface2>? _enumerableInterface2;
    private global::System.Collections.Generic.IEnumerable<global::Interface2> EnumerableInterface2 => _enumerableInterface2 ??= CreateEnumerableInterface2();

    private global::System.Collections.Generic.IEnumerable<global::Dependency4>? _enumerableDependency4;
    private global::System.Collections.Generic.IEnumerable<global::Dependency4> EnumerableDependency4 => _enumerableDependency4 ??= CreateEnumerableDependency4();

    global::Microsoft.Extensions.DependencyInjection.IServiceScope global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory.CreateScope() => this.CreateScope(_sync);

    public object? GetService(global::System.Type serviceType)
    {
        if (serviceType == typeof(global::Interface1)) return Dependency1_B_0;
        if (serviceType == typeof(global::Interface2)) return Dependency1_B_0;
        if (serviceType == typeof(global::Service)) return Service_0;
        if (serviceType == typeof(global::Dependency4)) return Dependency4_0;
        if (serviceType == typeof(global::Dependency5)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::Dependency6)) return CreateDependency6_0();
        if (serviceType == typeof(global::System.Collections.Generic.IEnumerable<global::Interface1>)) return EnumerableInterface1;
        if (serviceType == typeof(global::System.Collections.Generic.IEnumerable<global::Interface2>)) return EnumerableInterface2;
        if (serviceType == typeof(global::System.Collections.Generic.IEnumerable<global::Interface3>)) return global::System.Array.Empty<global::Interface3>();
        if (serviceType == typeof(global::System.Collections.Generic.IEnumerable<global::Dependency4>)) return EnumerableDependency4;
        if (serviceType == typeof(global::System.Collections.Generic.IEnumerable<global::Dependency5>)) return RootScope.GetService(serviceType);
        if (serviceType == typeof(global::System.Collections.Generic.IEnumerable<global::Dependency6>)) return CreateEnumerableDependency6();
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

    private global::Dependency1_A CreateDependency1_A_0()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Dependency1_A();
        }
    }

    private global::Dependency1_B CreateDependency1_B_0()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            return new global::Dependency1_B();
        }
    }

    private global::Service CreateService_0()
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            return new global::Service(
                GetService<global::System.Collections.Generic.IEnumerable<global::Interface1>>(),
                GetService<global::System.Collections.Generic.IEnumerable<global::Interface2>>(),
                GetService<global::System.Collections.Generic.IEnumerable<global::Interface3>>(),
                GetService<global::System.Collections.Generic.IEnumerable<global::Dependency4>>(),
                GetService<global::System.Collections.Generic.IEnumerable<global::Dependency5>>(),
                GetService<global::System.Collections.Generic.IEnumerable<global::Dependency6>>()
            );
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

    private global::Interface1[] CreateEnumerableInterface1()
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            return new global::Interface1[]
            {
                Dependency1_A_0,
                Dependency1_B_0
            };
        }
    }

    private global::Interface2[] CreateEnumerableInterface2()
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            return new global::Interface2[]
            {
                Dependency1_B_0
            };
        }
    }

    private global::Dependency4[] CreateEnumerableDependency4()
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            return new global::Dependency4[]
            {
                Dependency4_0
            };
        }
    }

    private global::Dependency6 CreateDependency6_0()
    {
        return new global::Dependency6();
    }

    private global::Dependency6[] CreateEnumerableDependency6()
    {
        return new global::Dependency6[]
        {
            CreateDependency6_0(),
            CreateDependency6_0()
        };
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
            || serviceType == typeof(global::Dependency4)
            || serviceType == typeof(global::Dependency5)
            || serviceType == typeof(global::Dependency6)
            || serviceType == typeof(global::Interface1)
            || serviceType == typeof(global::Interface2)
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