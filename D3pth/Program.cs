using Microsoft.Extensions.DependencyInjection;
using D3pth.Commands;
using D3pth.Interfaces;
using D3pth.Services;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();

ConfigureServices(serviceCollection);

using var registrar = new DependencyInjectionRegistrar(serviceCollection);

var commandApp = new CommandApp<GenerateCommand>(registrar);

commandApp.Configure(ConfigureApp);

return await commandApp.RunAsync(args);

static void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ICatalogGenerator, QuestPdfCatalogGenerator>();
    services.AddSingleton<IFileDiscoverer, RecursiveFileDiscoverer>();
    services.AddSingleton<IStlModelLoader, StlModelLoader>();
    services.AddSingleton<IStlModelRenderer, SkiaStlModelRenderer>();
}

static void ConfigureApp(IConfigurator app)
{
    app.SetApplicationName("D3pth");
    app.AddCommand<GenerateCommand>("generate");
    app.AddCommand<RenderCommand>("render");
    app.AddCommand<PrepareCommand>("prepare");
}
