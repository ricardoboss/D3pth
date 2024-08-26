using System.ComponentModel;
using PrintingCatalog.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PrintingCatalog.Commands;

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
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var files = settings.File is null
            ? fileDiscoverer.Discover(Directory.GetCurrentDirectory())
            : new List<FileInfo> { new(settings.File) };

        foreach (var file in files)
            await RenderFile(file, settings.Mode);

        return 0;
    }

    private async Task RenderFile(FileInfo file, RenderMode mode)
    {
        var model = await stlModelLoader.LoadAsync(file);

        AnsiConsole.MarkupLine(
            $"[green]Loaded '[bold]{model.Metadata.Name}[/]' with [bold]{model.Triangles.Length}[/] triangles[/]");

        var image = stlModelRenderer.RenderToPng(model, mode);
        var imageFile = new FileInfo(Path.ChangeExtension(file.FullName, $".{mode}.png"));
        await using var stream = imageFile.OpenWrite();
        await stream.WriteAsync(image);

        AnsiConsole.MarkupLine($"[green]Rendered to '[bold]{imageFile.Name}[/]'[/]");
    }
}
