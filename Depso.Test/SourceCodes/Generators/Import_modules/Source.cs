using Depso;

[ServiceProvider]
public partial class Provider
{
    private void RegisterServices()
    {
        ImportModule<Module>();
    }
}