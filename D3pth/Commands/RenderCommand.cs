using System.ComponentModel;
using D3pth.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace D3pth.Commands;

internal sealed class RenderCommand(
    IFileDiscoverer fileDiscoverer,
    IStlModelLoader stlModelLoader,
    IStlModelRenderer stlModelRenderer
) : AsyncCommand<RenderCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[file]")] public string? File { get; init; }

        [CommandOption("-m|--mode")]
        [Description("Render mode (shaded, depth, wireframe)")]
        public RenderMode Mode { get; init; } = RenderMode.Shaded;

        [CommandOption("--grid")]
        [Description("Draw a grid in the background")]
        public bool DrawGrid { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var files = settings.File is null
            ? fileDiscoverer.Discover(Directory.GetCurrentDirectory())
            : new List<FileInfo> { new(settings.File) };

        var options = RenderOptions.None;
        if (settings.DrawGrid)
            options |= RenderOptions.DrawGrid;

        foreach (var file in files)
            await RenderFile(file, settings.Mode, options);

        return 0;
    }

    private async Task RenderFile(FileInfo file, RenderMode mode, RenderOptions options)
    {
        var model = await stlModelLoader.LoadAsync(file);

        AnsiConsole.MarkupLine(
            $"[green]Loaded '[bold]{model.Metadata.Name}[/]' with [bold]{model.Triangles.Length}[/] triangles[/]");

        var image = stlModelRenderer.RenderToPng(model, mode, options);
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
