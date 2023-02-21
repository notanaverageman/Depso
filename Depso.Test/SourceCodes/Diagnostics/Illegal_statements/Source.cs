using Depso;
using System;

public class Service { }

[ServiceProvider]
public partial class Provider
{
    private void RegisterServices()
    {
        {|#1:object x = new object();|}

        {|#2:if (true)
        {
            return;
        }|}

        AddSingleton<Service>().{|#3:Equals(this)|};

        {|#4:Generic<Service>()|};
        {|#5:AddSingleton<Service>(this)|};
        {|#6:AddSingleton<Service, Service, Service>()|};
        
        {|#7:AddSingleton()|};
        {|#8:Method()|};
    }

    private void Generic<T>()
    {
    }

    private void Method()
    {
    }

    private void AddSingleton<T>(object o)
    {
    }

    private void AddSingleton()
    {
    }

    private void AddSingleton<T1, T2, T3>()
    {
    }
}

public partial class ProviderNoDiagnostic
{
    private void RegisterServices()
    {
        if (true)
        {
            return;
        }
    }
}