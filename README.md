# Depso Dependency Injection Source Generator

[![NuGet Depso](https://img.shields.io/nuget/v/Depso.svg?label=Depso)](https://www.nuget.org/packages/Depso/)

Depso is yet another source generator for dependency injection. However, it takes a different approach than existing
source generators and uses a restricted subset of C# instead of using attributes to define dependencies. This leads to
a more natural and readable code and allows for more extension points.

Behavior of the generated code is similar to the `IServiceCollection` interface in ASP.NET Core. For more information on
how to use dependency injection, see the [official documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection).

Examples of generated code can be found in the [Test](Depso.Test/SourceCodes/Generators) project.

## Features

- Clean and readable service registration code and generated code.
- No reflection or runtime code generation.
- Registering services with singleton, scoped, and transient lifetimes.
- Registering services as multiple types.
- Registering services using factory methods.
- Compile-time validation of missing service registrations.
- Resolving `IEnumerable<T>` services.
- Disposing of services that implement `IDisposable` or `IAsyncDisposable`.
- Grouping service registrations in modules and importing these registrations in service providers.

## Usage

Add the `Depso` package to your project:

```xml
<ItemGroup>
    <PackageReference Include="Depso" Version="1.0.0" PrivateAssets="all" />
</ItemGroup>
```

### Basic Usage

Services are registered by calling the `AddSingleton`, `AddScoped`, and `AddTransient` methods. It is possible to use the generic overloads
or the non-generic overloads that accept a `Type` parameter. The end result is exactly the same. Services can be registered as their own type
or as their base types or implemented interfaces.

```csharp
using Depso;

public interface ITransientInterface { }
public class Singleton { }
public class Scoped { }
public class Transient : ITransientInterface { }
public class TypeOf { }

// Decorate the class with the ServiceProvider attribute.
[ServiceProvider]
public partial class Container
{
    // Declare the method to register services. Its signature must be
    // private static void and it must be named RegisterServices.
    private void RegisterServices()
    {
        // Register services by calling the AddSingleton, AddScoped, and AddTransient methods.
        AddSingleton<Singleton>();
        AddScoped<Scoped>();
        // Service can only be resolved as an ITransientInterface.
        AddTransient<ITransientInterface, Transient>();

        // Register using typeof expressions.
        AddTransient(typeof(TypeOf));
    }
}
```

### Registering Services as Multiple Types

It is frequently needed to register a service as multiple types. Normally, this is done by registering the other types using factory
methods and resolving the service in these methods using the passed `IServiceProvider`. This library provides `AlsoAs` and `AlsoAsSelf`
methods to register a service as multiple types and reduce the boilerplate code.

```csharp
using Depso;

public interface SingletonInterface { }
public interface ScopedInterface { }
public interface TransientInterface { }
public class Singleton : SingletonInterface { }
public class Scoped : ScopedInterface { }
public class Transient : TransientInterface { }

[ServiceProvider]
public partial class Container
{
    private void RegisterServices()
    {
        // Register a service as its own type and also as an interface.
        AddSingleton<Singleton>().AlsoAs<SingletonInterface>();
        AddScoped(typeof(Scoped)).AlsoAs(typeof(ScopedInterface));
        
        // Register a service as an interface and also as its own type.
        AddTransient<TransientInterface, Transient>().AlsoAsSelf();
    }
}
```

### Factories

Factory methods can be used to create services manually or register existing instance objects. `AddX` methods have overloads that accept
a factory method of type `Func<IServiceProvider, T>`. It is possible to use static methods, instance methods, and lambda
expressions as factory methods.

```csharp
using Depso;

public class Member { }
public class Lambda { }
public class Static { }
public class Instance { }

[ServiceProvider]
public partial class Container
{
    private readonly Member _member;

    public Container(Member member)
    {
        _member = member;
    }

    private void RegisterServices()
    {
        // Register an object instance.
        AddTransient(_ => _member);

        // Register a service using a lambda.
        AddTransient(_ => new Lambda());

        // Register a service using a static factory method.
        AddTransient(CreateStatic);

        // Register a service using an instance factory method.
        AddTransient(CreateInstance);
    }

    private static Static CreateStatic(IServiceProvider _) => new Static();
    private Instance CreateInstance(IServiceProvider _) => new Instance();
}
```

### Modules

Modules can be used to group related services together. Modules can be nested and can reference other modules. Modules that are defined
on other assemblies can also be imported. In this case, the types that are registered in the imported module must be accessible by the declaring
service provider.

The `RegisterServices` method has to be `static` on modules. This means that modules can't use instance members.

```csharp
using Depso;

public class ModuleFactory { }
public class ModuleService { }
public class OtherModuleService { }

[ServiceProviderModule]
public partial class Module
{
    private static void RegisterServices()
    {
        // Registration of services is the same as in the service provider.
        AddSingleton<ModuleService>();
        // Modules can import other modules.
        ImportModule<OtherModule>();
    }
}

[ServiceProviderModule]
public partial class OtherModule
{
    private static void RegisterServices()
    {
        AddSingleton<OtherModuleService>();
    }
}

[ServiceProvider]
public partial class Container
{
    private void RegisterServices()
    {
        ImportModule<Module>();
    }
}
```

## Performance

The benchmark project is similar to the one in [Jab](https://github.com/pakrym/jab/tree/main/src/Jab.Performance) project. However, all
service providers are treated as `IServiceProvider` and the `IServiceProvider` interface is used to resolve services.

The differences between this library and Jab should probably be negligible when used in real-world applications.

### Service Registration and First Resolve

```text
BenchmarkDotNet v0.13.12, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2)
Intel Core i9-10900K CPU 3.70GHz, 1 CPU, 20 logical and 10 physical cores
.NET SDK 8.0.202
  [Host]     : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2


| Method | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------- |------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| MEDI   | 1,069.95 ns | 20.338 ns | 20.886 ns | 71.29 |    2.33 | 0.5188 | 0.0095 |    5432 B |       61.73 |
| Jab    |    15.86 ns |  0.207 ns |  0.194 ns |  1.06 |    0.03 | 0.0053 |      - |      56 B |        0.64 |
| Depso  |    15.02 ns |  0.322 ns |  0.331 ns |  1.00 |    0.00 | 0.0084 |      - |      88 B |        1.00 |
```

### Service Resolution

```text
BenchmarkDotNet v0.13.12, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2)
Intel Core i9-10900K CPU 3.70GHz, 1 CPU, 20 logical and 10 physical cores
.NET SDK 8.0.202
  [Host]     : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2


| Method | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| MEDI   | 19.857 ns | 0.0458 ns | 0.0406 ns |  2.32 |    0.02 | 0.0023 |      24 B |        1.00 |
| Jab    | 11.937 ns | 0.0528 ns | 0.0468 ns |  1.39 |    0.01 | 0.0023 |      24 B |        1.00 |
| Depso  |  8.569 ns | 0.0651 ns | 0.0609 ns |  1.00 |    0.00 | 0.0023 |      24 B |        1.00 |
```
