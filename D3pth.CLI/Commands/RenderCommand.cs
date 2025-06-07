using System.Diagnostics.CodeAnalysis;
using D3pth.Abstractions.Rendering;
using D3pth.Abstractions.Services;
using D3pth.CLI.Settings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace D3pth.CLI.Commands;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal sealed class RenderCommand(
    IFileDiscoverer fileDiscoverer,
    IStlModelLoader stlModelLoader,
    IModelMetadataLoader metadataLoader,
    IPngRenderer pngRenderer
) : AsyncCommand<RenderSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RenderSettings settings)
    {
        var files = settings.File is null
            ? fileDiscoverer.Discover(Directory.GetCurrentDirectory())
            : new List<FileInfo> { new(settings.File) };

        var options = RenderOptions.None;
        options.Mode = settings.Mode;
        if (settings.DrawGrid)
            options.DrawGrid = true;

        foreach (var file in files)
            await RenderFile(file, options, settings.RenderSize);

        return 0;
    }

    private async Task RenderFile(FileInfo file, RenderOptions options, int size)
    {
        var model = await stlModelLoader.LoadAsync(file);
        var metadata = await metadataLoader.LoadAsync(file);

        AnsiConsole.MarkupLine(
            $"[green]Loaded '[bold]{metadata.Name}[/]' with [bold]{model.Triangles.Length}[/] triangles[/]");

        var image = pngRenderer.Render(size, size, model, metadata, options);
        var extension = options.Mode switch
        {
            RenderMode.Shaded => ".png",
            RenderMode.Depth => ".Depth.png",
            RenderMode.Wireframe => ".Wireframe.png",
            _ => throw new ArgumentOutOfRangeException(nameof(options.Mode), options.Mode, null),
        };
        var imageFile = new FileInfo(Path.ChangeExtension(file.FullName, extension));
        await using var stream = imageFile.OpenWrite();
        await stream.WriteAsync(image);

        AnsiConsole.MarkupLine($"[green]Rendered to '[bold]{imageFile.Name}[/]'[/]");
    }
}
