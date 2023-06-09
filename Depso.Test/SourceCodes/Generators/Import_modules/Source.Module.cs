using Depso;

public interface SingletonInterface1 { }
public interface SingletonInterface2 { }
public interface SingletonInterface3 { }
public interface SingletonInterface4 { }
public interface SingletonInterface5 { }
public interface SingletonInterface6 { }

public class Singleton1 : SingletonInterface1 { }
public class Singleton2 : SingletonInterface2, SingletonInterface1 { }
public class Singleton3 : SingletonInterface3 { }
public class Singleton4 : SingletonInterface4 { }
public class Singleton5 : SingletonInterface5 { }
public record Singleton6(SingletonInterface5 _) : SingletonInterface6;

public interface ScopedInterface1 { }
public interface ScopedInterface2 { }
public interface ScopedInterface3 { }
public interface ScopedInterface4 { }
public interface ScopedInterface5 { }
public interface ScopedInterface6 { }

public class Scoped1 : ScopedInterface1 { }
public class Scoped2 : ScopedInterface2, ScopedInterface1 { }
public class Scoped3 : ScopedInterface3 { }
public class Scoped4 : ScopedInterface4 { }
public class Scoped5 : ScopedInterface5 { }
public class Scoped6 : ScopedInterface6 { }

public interface TransientInterface1 { }
public interface TransientInterface2 { }
public interface TransientInterface3 { }
public interface TransientInterface4 { }
public interface TransientInterface5 { }
public interface TransientInterface6 { }

public class Transient1 : TransientInterface1 { }
public class Transient2 : TransientInterface2, ScopedInterface1 { }
public class Transient3 : TransientInterface3 { }
public class Transient4 : TransientInterface4 { }
public class Transient5 : TransientInterface5 { }
public class Transient6 : TransientInterface6 { }

[ServiceProviderModule]
public partial class Module
{
    private static void RegisterServices()
    {
        AddSingleton<Singleton1>().AlsoAs<SingletonInterface1>();
        AddSingleton(typeof(Singleton2)).AlsoAs(typeof(SingletonInterface2)).AlsoAs<SingletonInterface1>();
        AddSingleton(_ => new Singleton3());

        AddSingleton<SingletonInterface4, Singleton4>().AlsoAsSelf();
        AddSingleton(typeof(SingletonInterface5), typeof(Singleton5));
        AddSingleton<SingletonInterface6>(x => new Singleton6((SingletonInterface5)x.GetService(typeof(ScopedInterface5))!));
        
        AddScoped<Scoped1>().AlsoAs<ScopedInterface1>();
        AddScoped(typeof(Scoped2)).AlsoAs(typeof(ScopedInterface2)).AlsoAs<ScopedInterface1>();
        AddScoped(_ => new Scoped3());

        AddScoped<ScopedInterface4, Scoped4>().AlsoAsSelf();
        AddScoped(typeof(ScopedInterface5), typeof(Scoped5));
        AddScoped<ScopedInterface6>(_ => new Scoped6());
        
        AddTransient<Transient1>().AlsoAs<TransientInterface1>();
        AddTransient(typeof(Transient2)).AlsoAs(typeof(TransientInterface2)).AlsoAs<TransientInterface1>();
        AddTransient(_ => new Transient3());

        AddTransient<TransientInterface4, Transient4>().AlsoAsSelf();
        AddTransient(typeof(TransientInterface5), typeof(Transient5));
        AddTransient<TransientInterface6>(_ => new Transient6());

        ImportModule(typeof(ExternalModule));
    }
}