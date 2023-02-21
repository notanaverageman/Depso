// ReSharper disable InconsistentNaming
namespace Depso;

public class Constants
{
    public const string GeneratorNamespace = "Depso";
	
    public const string TypeMetadataName = "System.Type";
    public const string FuncMetadataName = "System.Func";
    public const string IEnumerableMetadataName = "System.Collections.Generic.IEnumerable";
    public const string ListMetadataName = "System.Collections.Generic.List";
    public const string ValueTaskMetadataName = "global::System.Threading.Tasks.ValueTask";
    public const string IDisposableMetadataName = "System.IDisposable";
    public const string IAsyncDisposableMetadataName = "System.IAsyncDisposable";
    public const string ObjectDisposedException = "System.ObjectDisposedException";
    
    public const string IServiceProviderMetadataName = "System.IServiceProvider";
    public const string IServiceScopeMetadataName = "Microsoft.Extensions.DependencyInjection.IServiceScope";
    public const string IServiceScopeFactoryMetadataName = "Microsoft.Extensions.DependencyInjection.IServiceScopeFactory";
    public const string IServiceProviderIsServiceMetadataName = "Microsoft.Extensions.DependencyInjection.IServiceProviderIsService";

    public const string RegistrationModifierClassName = "RegistrationModifier";
    public const string ScopeClassName = "Scope";

    public const string GetServiceMethodName = "GetService";
    public const string RegisterServicesMethodName = "RegisterServices";
    public const string DisposeMethodName = "Dispose";
    public const string DisposeAsyncMethodName = "DisposeAsync";
    public const string AddDisposableMethodName = "AddDisposable";
    public const string AddAsyncDisposableMethodName = "AddAsyncDisposable";
    public const string CreateScopeMethodName = "CreateScope";
    public const string CreateDisposableMethodNameSuffix = "AddDisposable";
    public const string ThrowIfDisposedMethodName = "ThrowIfDisposed";
    
    public const string SingletonMethodName = "AddSingleton";
    public const string ScopedMethodName = "AddScoped";
    public const string TransientMethodName = "AddTransient";

    public const string AlsoAsSelfMethodName = "AlsoAsSelf";
    public const string AlsoAsMethodName = "AlsoAs";

    public const string LockFieldName = "_sync";
    public const string RootScopeFieldName = "_rootScope";
    public const string RootScopePropertyName = "RootScope";
    public const string LambdaFieldPrefix = "_factoryFunc";
    public const string IsDisposedFieldName = "_isDisposed";
    public const string DisposablesFieldName = "_transientDisposables";
    public const string AsyncDisposablesFieldName = "_transientAsyncDisposables";
}