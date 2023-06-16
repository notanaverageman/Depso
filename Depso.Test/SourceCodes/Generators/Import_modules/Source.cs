using Depso;
using Test.Nested;

[ServiceProvider]
public partial class Provider
{
    private void RegisterServices()
    {
        ImportModule<Module>();
    }
}