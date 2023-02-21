using Depso;

public class Service { }

[ServiceProvider]
public class {|#1:Provider|}
{
    private void RegisterServices()
    {
        AddSingleton<Service>();
    }
}

public class ProviderNoDiagnostic
{
    private void RegisterServices()
    {
        AddSingleton<Service>();
    }

    private void AddSingleton<T>()
    {
    }
}