using Depso;

public class Dependency1 { }
public class Dependency2 { }
public class Dependency3 { }

public class Service
{
    public Service() { }
    public Service(Dependency1 d1) { }
    public Service(Dependency2 d2) { }
    public Service(Dependency1 d1, Dependency2 d2) { }
    public Service(Dependency1 d1, Dependency2 d2, Dependency3 d3) { }
}

[ServiceProvider]
public partial class Provider
{
    private void RegisterServices()
    {
        AddSingleton<Service>();
        AddSingleton<Dependency1>();
        AddSingleton<Dependency2>();
    }
}