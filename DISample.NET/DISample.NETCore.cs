/*
appsettings.json

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="7.0.1" />
  </ItemGroup>
</Project>
 */
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DISample.NET;

class ServiceBase
{
    static int id = 0;

    public ServiceBase()
    {
        ++id;
        Log("Created!");
    }

    public int Id { get => id; }

    public void Start() => Log("Started!");

    void Log(string message = "") => Console.WriteLine($"{GetType().Name}\t: {Id} {message}");
}

public interface IMyService
{
    int Id { get; }
    void Start();
}

interface IMyTransientService : IMyService
{ }

interface IMyScopedService : IMyService
{ }

interface IMySingletonService : IMyService
{ }

class MyTransientService : ServiceBase, IMyTransientService
{}

class MyScopedService : ServiceBase, IMyScopedService
{}

class MySingletonService : ServiceBase, IMySingletonService
{}

class Program
{
    static async Task Main(string[] args)
    {
        //await CreateServiceSample(args);
        //await ServiceLifeTimeSample(args);
        await HostedServiceSample(args);
    }

    static async Task CreateServiceSample(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
                               .ConfigureServices((_, services) => {
                                   services.AddTransient<IMyTransientService, MyTransientService>()
                                           .AddScoped   <IMyScopedService   , MyScopedService   >()
                                           .AddSingleton<IMySingletonService, MySingletonService>();
                               })
                               .Build();

        IMyService service = host.Services.GetService<IMyTransientService>();
        service            = host.Services.GetService<IMyScopedService   >();
        service            = host.Services.GetService<IMySingletonService>();

        await host.RunAsync();
    }

    static async Task ServiceLifeTimeSample(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
                               .ConfigureServices((_, services) => {
                                   services.AddTransient<IMyTransientService, MyTransientService>()
                                           .AddScoped   <IMyScopedService   , MyScopedService   >()
                                           .AddSingleton<IMySingletonService, MySingletonService>();
                               })
                               .Build();

        ExemplifyServiceLifetime(host.Services, 1);
        ExemplifyServiceLifetime(host.Services, 2);

        await host.RunAsync();
    }

    static async Task HostedServiceSample(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
                               .ConfigureServices((_, services) => {
                                   services.AddSingleton<IMyService, MyService>()
                                           .AddSingleton<IHostedService, MyHostedService>()
                                           .AddSingleton<IHostedService, MyHostedService>();
                               })
                               .Build();
        await host.RunAsync();
    }

    static void ExemplifyServiceLifetime(IServiceProvider hostProvider, int count)
    {
        Console.WriteLine($"--------- ExemplifyServiceLifetime({count}) -----------");

        using IServiceScope serviceScope = hostProvider.CreateScope();
        IServiceProvider    provider     = serviceScope.ServiceProvider;

        IMyService service = provider.GetService<IMyTransientService>();
        service            = provider.GetService<IMyTransientService>();

        service            = provider.GetService<IMyScopedService   >();
        service            = provider.GetService<IMyScopedService   >();

        service            = provider.GetService<IMySingletonService>();
        service            = provider.GetService<IMySingletonService>();
    }

    public class MyService : ServiceBase, IMyService
    {}

    class MyHostedService : IHostedService
    {
        IMyService  service;
        static long count = 0;
        long        myCount;

        public MyHostedService(IMyService service)
        {
            Log("Created!!");
            (this.service, myCount) = (service, ++count);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log("Started!!");
            service.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        void Log(string message = "") => Console.WriteLine($"{GetType().Name}\t: {myCount} {message}");
    }
}
