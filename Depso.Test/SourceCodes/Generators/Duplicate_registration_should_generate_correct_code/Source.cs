using Depso;

public interface Interface1 { }
public interface Interface2 { }
public interface Interface3 { }
public interface Interface4 { }
public class Dependency1 : Interface1 { }
public class Dependency2 : Interface2 { }
public class Dependency3 : Interface3 { }
public class Dependency4 : Interface3, Interface4 { }

public class Service1
{
    public Service1(Dependency1 d1, Dependency2 d2, Dependency3 d3, Dependency4? d4 = null) { }
}

public class Service2
{
    public Service2(Interface1 i1, Interface2 i2, Interface3 i3, Interface4 i4) { }
}

public class Service3 : Service2
{
    public Service3(Interface1 i1, Interface2 i2, Interface3 i3, Interface4 i4) : base(i1, i2, i3, i4)
    {
    }
}

[ServiceProvider]
public partial class Provider
{
    private void RegisterServices()
    {
        AddSingleton<Service1>();
        AddSingleton<Service1>();

        AddSingleton<Service2, Service2>();
        AddSingleton<Service2>();
        AddSingleton<Service2, Service3>().AlsoAsSelf();

        AddSingleton<Interface1, Dependency1>();
        AddSingleton<Interface1, Dependency1>();
        AddSingleton<Dependency1>();

        AddSingleton<Interface2, Dependency2>().AlsoAsSelf();
        AddSingleton<Dependency2>();

        AddSingleton<Dependency3>();
        AddSingleton<Interface3, Dependency3>();

        AddSingleton<Interface3, Dependency4>();
        AddSingleton<Interface4, Dependency4>();
    }
}