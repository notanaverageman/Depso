using System.Collections.Generic;
using Depso;

public class Dependency1 { }
public class Dependency2 { }

public class Service
{
    public Service(Dependency1 d1, IEnumerable<Dependency2> d2) { }
}

[ServiceProvider]
public partial class Provider
{
    private void RegisterServices()
    {
        AddSingleton<Dependency1>();
        AddSingleton<Service>();
    }
}