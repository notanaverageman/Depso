[global::System.AttributeUsage(global::System.AttributeTargets.Class)]
file class GeneratedServiceProviderModuleAttribute : global::System.Attribute
{
}

[global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true)]
file class SingletonAttribute : global::System.Attribute
{
    public SingletonAttribute(
        global::System.Type serviceType,
        global::System.Type? implementationType = null,
        string? factory = null,
        params global::System.Type[] registerAlsoAs)
    {
    }
}

[global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true)]
file class ScopedAttribute : global::System.Attribute
{
    public ScopedAttribute(
        global::System.Type serviceType,
        global::System.Type? implementationType = null,
        string? factory = null,
        params global::System.Type[] registerAlsoAs)
    {
    }
}

[global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true)]
file class TransientAttribute : global::System.Attribute
{
    public TransientAttribute(
        global::System.Type serviceType,
        global::System.Type? implementationType = null,
        string? factory = null,
        params global::System.Type[] registerAlsoAs)
    {
    }
}

public interface ExternalSingletonInterface1 { }
public interface ExternalSingletonInterface2 { }
public interface ExternalSingletonInterface3 { }
public interface ExternalSingletonInterface4 { }
public interface ExternalSingletonInterface5 { }
public interface ExternalSingletonInterface6 { }

public class ExternalSingleton1 : ExternalSingletonInterface1 { }
public class ExternalSingleton2 : ExternalSingletonInterface2, ExternalSingletonInterface1 { }
public class ExternalSingleton3 : ExternalSingletonInterface3 { }
public class ExternalSingleton4 : ExternalSingletonInterface4 { }
public class ExternalSingleton5 : ExternalSingletonInterface5 { }
public record ExternalSingleton6(ExternalSingletonInterface5 _) : ExternalSingletonInterface6;

public interface ExternalScopedInterface1 { }
public interface ExternalScopedInterface2 { }
public interface ExternalScopedInterface3 { }
public interface ExternalScopedInterface4 { }
public interface ExternalScopedInterface5 { }
public interface ExternalScopedInterface6 { }

public class ExternalScoped1 : ExternalScopedInterface1 { }
public class ExternalScoped2 : ExternalScopedInterface2, ExternalScopedInterface1 { }
public class ExternalScoped3 : ExternalScopedInterface3 { }
public class ExternalScoped4 : ExternalScopedInterface4 { }
public class ExternalScoped5 : ExternalScopedInterface5 { }
public class ExternalScoped6 : ExternalScopedInterface6 { }

public interface ExternalTransientInterface1 { }
public interface ExternalTransientInterface2 { }
public interface ExternalTransientInterface3 { }
public interface ExternalTransientInterface4 { }
public interface ExternalTransientInterface5 { }
public interface ExternalTransientInterface6 { }

public class ExternalTransient1 : ExternalTransientInterface1 { }
public class ExternalTransient2 : ExternalTransientInterface2, ExternalScopedInterface1 { }
public class ExternalTransient3 : ExternalTransientInterface3 { }
public class ExternalTransient4 : ExternalTransientInterface4 { }
public class ExternalTransient5 : ExternalTransientInterface5 { }
public class ExternalTransient6 : ExternalTransientInterface6 { }

[global::Singleton(typeof(global::ExternalSingleton1), null, null, typeof(global::ExternalSingletonInterface1))]
[global::Singleton(typeof(global::ExternalSingleton2), null, null, typeof(global::ExternalSingletonInterface1), typeof(global::ExternalSingletonInterface2))]
[global::Singleton(typeof(global::ExternalSingleton3), null, "global::ExternalModule.FactoryExternalSingleton3_0")]
[global::Singleton(typeof(global::ExternalSingletonInterface4), typeof(global::ExternalSingleton4), null, typeof(global::ExternalSingleton4))]
[global::Singleton(typeof(global::ExternalSingletonInterface5), typeof(global::ExternalSingleton5), null)]
[global::Singleton(typeof(global::ExternalSingletonInterface6), null, "global::ExternalModule.FactoryExternalSingletonInterface6_0")]
[global::Scoped(typeof(global::ExternalScoped1), null, null, typeof(global::ExternalScopedInterface1))]
[global::Scoped(typeof(global::ExternalScoped2), null, null, typeof(global::ExternalScopedInterface1), typeof(global::ExternalScopedInterface2))]
[global::Scoped(typeof(global::ExternalScoped3), null, "global::ExternalModule.FactoryExternalScoped3_0")]
[global::Scoped(typeof(global::ExternalScopedInterface4), typeof(global::ExternalScoped4), null, typeof(global::ExternalScoped4))]
[global::Scoped(typeof(global::ExternalScopedInterface5), typeof(global::ExternalScoped5), null)]
[global::Scoped(typeof(global::ExternalScopedInterface6), null, "global::ExternalModule.FactoryExternalScopedInterface6_0")]
[global::Transient(typeof(global::ExternalTransient1), null, null, typeof(global::ExternalTransientInterface1))]
[global::Transient(typeof(global::ExternalTransient2), null, null, typeof(global::ExternalTransientInterface1), typeof(global::ExternalTransientInterface2))]
[global::Transient(typeof(global::ExternalTransient3), null, "global::ExternalModule.FactoryExternalTransient3_0")]
[global::Transient(typeof(global::ExternalTransientInterface4), typeof(global::ExternalTransient4), null, typeof(global::ExternalTransient4))]
[global::Transient(typeof(global::ExternalTransientInterface5), typeof(global::ExternalTransient5), null)]
[global::Transient(typeof(global::ExternalTransientInterface6), null, "global::ExternalModule.FactoryExternalTransientInterface6_0")]
[global::GeneratedServiceProviderModuleAttribute]
public partial class ExternalModule
{
    public static global::ExternalSingleton3 FactoryExternalSingleton3_0(global::System.IServiceProvider _)
    {
        return new global::ExternalSingleton3();
    }

    public static global::ExternalSingletonInterface6 FactoryExternalSingletonInterface6_0(global::System.IServiceProvider x)
    {
        return new global::ExternalSingleton6((global::ExternalSingletonInterface5)x.GetService(typeof(global::ExternalScopedInterface5))!);
    }

    public static global::ExternalScoped3 FactoryExternalScoped3_0(global::System.IServiceProvider _)
    {
        return new global::ExternalScoped3();
    }

    public static global::ExternalScopedInterface6 FactoryExternalScopedInterface6_0(global::System.IServiceProvider _)
    {
        return new global::ExternalScoped6();
    }

    public static global::ExternalTransient3 FactoryExternalTransient3_0(global::System.IServiceProvider _)
    {
        return new global::ExternalTransient3();
    }

    public static global::ExternalTransientInterface6 FactoryExternalTransientInterface6_0(global::System.IServiceProvider _)
    {
        return new global::ExternalTransient6();
    }
}