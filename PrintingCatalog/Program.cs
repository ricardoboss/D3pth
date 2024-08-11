using Microsoft.Extensions.DependencyInjection;
using PrintingCatalog.Commands;
using PrintingCatalog.Interfaces;
using PrintingCatalog.Services;
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
    app.SetApplicationName("PrintingCatalog");
    app.AddCommand<GenerateCommand>("generate");
    app.AddCommand<LoadFile>("load");
}
