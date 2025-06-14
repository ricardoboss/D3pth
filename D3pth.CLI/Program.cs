﻿using System.Diagnostics.CodeAnalysis;
using D3pth.Abstractions.Catalog;
using D3pth.Abstractions.Rendering;
using D3pth.Abstractions.Services;
using D3pth.Catalog.QuestPdf;
using D3pth.CLI.Commands;
using D3pth.CLI.Settings;
using D3pth.Rendering.Skia;
using D3pth.Sdk.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();

ConfigureServices(serviceCollection);

using var registrar = new DependencyInjectionRegistrar(serviceCollection);

var commandApp = new CommandApp(registrar);

commandApp.Configure(ConfigureApp);

return await commandApp.RunAsync(args);

static void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(b =>
    {
        b.ClearProviders();
        b.AddSpectreConsole();
    });

    services.AddSingleton<ICatalogGenerator, QuestPdfCatalogGenerator>();
    services.AddSingleton<IFileDiscoverer, RecursiveFileDiscoverer>();
    services.AddSingleton<IStlModelLoader, StlModelLoader>();
    services.AddSingleton<SkiaModelRenderer>();
    services.AddSingleton<IPngRenderer, SkiaPngRenderer>();
}

[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(GenerateCommand))]
[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(GenerateSettings))]
[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RenderCommand))]
[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RenderSettings))]
[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PrepareCommand))]
[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PrepareSettings))]
static void ConfigureApp(IConfigurator app)
{
    app.SetApplicationName("D3pth");
    app.AddCommand<GenerateCommand>("generate");
    app.AddCommand<RenderCommand>("render");
    app.AddCommand<PrepareCommand>("prepare");
}
