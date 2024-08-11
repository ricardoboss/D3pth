using PrintingCatalog.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PrintingCatalog.Commands;

internal sealed class LoadFile(IStlModelLoader stlModelLoader, IStlModelRenderer stlModelRenderer) : AsyncCommand<LoadFile.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<file>")] public required string File { get; init; }

        [CommandOption("-r|--renderer")]
        public bool Render { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var file = new FileInfo(settings.File);
        if (!file.Exists)
        {
            AnsiConsole.MarkupLine($"[red]File '[bold]{file.Name}[/]' does not exist");

            return 1;
        }

        var model = await stlModelLoader.LoadAsync(file);

        AnsiConsole.MarkupLine(
            $"[green]Loaded '[bold]{model.Metadata.Name}[/]' with [bold]{model.Triangles.Length}[/] triangles[/]");

        if (!settings.Render) return 0;

        var image = await stlModelRenderer.RenderToPngAsync(model);
        var imageFile = new FileInfo($"{model.Metadata.Name}.png");
        await using var stream = imageFile.OpenWrite();
        await stream.WriteAsync(image);

        AnsiConsole.MarkupLine($"[green]Rendered to '[bold]{imageFile.Name}[/]'[/]");

        return 0;
    }
}
