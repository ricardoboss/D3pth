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
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var files = settings.File is null
            ? fileDiscoverer.Discover(Directory.GetCurrentDirectory())
            : new List<FileInfo> { new(settings.File) };

        foreach (var file in files)
            await RenderFile(file);

        return 0;
    }

    private async Task RenderFile(FileInfo file)
    {
        var model = await stlModelLoader.LoadAsync(file);

        AnsiConsole.MarkupLine(
            $"[green]Loaded '[bold]{model.Metadata.Name}[/]' with [bold]{model.Triangles.Length}[/] triangles[/]");

        var image = stlModelRenderer.RenderToPng(model);
        var imageFile = new FileInfo(file.FullName.Replace(".stl", ".png"));
        await using var stream = imageFile.OpenWrite();
        await stream.WriteAsync(image);

        AnsiConsole.MarkupLine($"[green]Rendered to '[bold]{imageFile.Name}[/]'[/]");
    }
}
