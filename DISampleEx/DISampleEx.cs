/*
appsettings.json

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
 */

class DIContainer
{
    Dictionary<string, Type> map = new();

    public DIContainer Register<TInterface, T>()
    {
        var interfaceType = typeof(TInterface);
        var type          = typeof(T);
        var key           = interfaceType.FullName;
        if (!string.IsNullOrWhiteSpace(key) && !map.ContainsKey(key) && interfaceType.IsAssignableFrom(type))
            map[key] = type;
        return this;
    }

    public TInterface? Resolve<TInterface>()
    {
        if (map.TryGetValue(typeof(TInterface).FullName ?? "", out var type)) {
            var constructor = type.GetConstructors().SingleOrDefault();
            if (constructor is not null) {
                var parameters = constructor.GetParameters();
                var arguments  = parameters.Select(parameter => (object?)Resolve<TInterface>(parameter.ParameterType)).ToArray();
                return (TInterface?)constructor.Invoke(arguments);
            }
        }
        return default(TInterface);
    }

    object? Resolve<TInterface>(Type interfaceType)
        => map.TryGetValue(interfaceType.FullName ?? "", out var type)
           ? Activator.CreateInstance(type)
           : null;
}

static class Extensions
{
    public static void Log<T>(this T @this, string message) => Console.WriteLine($"{@this?.GetType()?.Name ?? ""} {message}");
}

public interface IMyService
{
    void Start();
}

public class MyService : IMyService
{
    public MyService() => this.Log("Created!");

    public void Start() => this.Log("Started!");
}

public interface IMyHostedService
{
    void Start();
}

public class MyHostedService : IMyHostedService
{
    readonly IMyService service;

    public MyHostedService(IMyService service)
    {
        this.Log("Created!");
        this.service = service;
    }

    public void Start()
    {
        this.Log("Started!");
        service.Start();
    }
}

class Program
{
    static void Main()
    {
        CreateServiceSample      ();
        CreateHosterServiceSample();
    }

    static void CreateServiceSample() 
    {
        DIContainer dIContainer = new();

        dIContainer.Register<IMyService, MyService>();

        var service = dIContainer.Resolve<IMyService>();
        service?.Start();
    }

    static void CreateHosterServiceSample()
    {
        DIContainer dIContainer = new();

        dIContainer.Register<IMyService, MyService>()
                   .Register<IMyHostedService, MyHostedService>();

        var hostedService = dIContainer.Resolve<IMyHostedService>();
        hostedService?.Start();
    }
}
