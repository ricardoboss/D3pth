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
    IStlModelRenderer stlModelRenderer
) : AsyncCommand<RenderSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RenderSettings settings)
    {
        var files = settings.File is null
            ? fileDiscoverer.Discover(Directory.GetCurrentDirectory())
            : new List<FileInfo> { new(settings.File) };

        var options = RenderOptions.None;
        if (settings.DrawGrid)
            options |= RenderOptions.DrawGrid;

        foreach (var file in files)
            await RenderFile(file, settings.Mode, options, settings.RenderSize);

        return 0;
    }

    private async Task RenderFile(FileInfo file, RenderMode mode, RenderOptions options, int size)
    {
        var model = await stlModelLoader.LoadAsync(file);
        var metadata = await metadataLoader.LoadAsync(file);

        AnsiConsole.MarkupLine(
            $"[green]Loaded '[bold]{metadata.Name}[/]' with [bold]{model.Triangles.Length}[/] triangles[/]");

        var image = stlModelRenderer.RenderToPng(size, size, model, metadata, mode, options);
        var extension = mode switch
        {
            RenderMode.Shaded => ".png",
            RenderMode.Depth => ".Depth.png",
            RenderMode.Wireframe => ".Wireframe.png",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
        };
        var imageFile = new FileInfo(Path.ChangeExtension(file.FullName, extension));
        await using var stream = imageFile.OpenWrite();
        await stream.WriteAsync(image);

        AnsiConsole.MarkupLine($"[green]Rendered to '[bold]{imageFile.Name}[/]'[/]");
    }
}
