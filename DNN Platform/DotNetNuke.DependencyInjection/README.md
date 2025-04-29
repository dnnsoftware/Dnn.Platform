# DotNetNuke.DependencyInjection project

This project exposes `IDnnStartup`, which any component can implement in order
to register services with DNN's dependency injection container. It also exposes
an extension method on the `Assembly` class, `SafeGetTypes`, which gets all of
the `Type` objects from the assembly while handling potential load errors.

This project targets
[.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
2.0, meaning that it is compatible with both .NET Framework and .NET Core. Any
interfaces defined in this project cannot reference types which are only
available to .NET Framework (for example, types in the `System.Web` namespace).

## Requesting Dependencies

DNN supports dependency injection for all of the major module patterns, and is
in the process of adding support in other areas.

Within a Web API controller or MVC controller, requesting a type from the
dependency injection container is simple. Add a constructor to the controller
class, and any constructor arguments which can be resolved from the container
will be provided automatically. For example:

```csharp
public class MyController : DnnApiController
{
    private readonly INavigationManager navigationManager;

    public MyController(INavigationManager navigationManager)
    {
        this.navigationManager = navigationManager;
    }

}
```

## Registering Dependencies

In addition to the types that DNN automatically adds to the dependency injection
container, developers can register their own types as well. Simply create a
class that implements `IDnnStartup` and use the `ConfigureServices` method to
add registrations to the collection of services. For example:

```csharp
public class Startup : IDnnStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IMyEmailCommunicator, MyEmailCommunicator>();
    }
}
```

In this example, the `MyEmailCommunicator` class is mapped to the
`IMyEmailCommunicator` interface, using the _scoped_ lifetime. It is also
possible to register concrete types directly, or register factory methods or
specific instances. A registration will use one of the three available
lifetimes, `Transient` (a new instance every time), `Scoped` (a new instance for
each scope, e.g. web request), or `Singleton` (a single shared instance).
