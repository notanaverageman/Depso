using System.Collections.Generic;
using Depso;

public interface Interface1 { }
public interface Interface2 { }
public interface Interface3 { }

public class Dependency1_A : Interface1, Interface2 { }
public class Dependency1_B : Interface1, Interface2 { }

public class Dependency3 : Interface3 { }
public class Dependency4 { }
public class Dependency5 { }
public class Dependency6 { }

public class Service
{
    public Service(
        IEnumerable<Interface1> i1,
        IEnumerable<Interface2> i2,
        IEnumerable<Interface3> i3,
        IEnumerable<Dependency4> d4,
        IEnumerable<Dependency5> d5,
        IEnumerable<Dependency6> d6)
    {
    }
}

[ServiceProvider]
public partial class Provider
{
    private void RegisterServices()
    {
        AddSingleton<Interface1, Dependency1_A>();
        AddSingleton<Interface1, Dependency1_B>().AlsoAs<Interface2>();
        AddSingleton<Service>();
        AddSingleton<Dependency4>();
        AddScoped<Dependency5>();
        AddTransient<Dependency6>();
        AddTransient<Dependency6>();
    }
}