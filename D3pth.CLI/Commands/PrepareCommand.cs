using System.Text.Json;
using D3pth.Abstractions;
using D3pth.Sdk.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace D3pth.Commands;

internal sealed class PrepareCommand(IFileDiscoverer fileDiscoverer) : AsyncCommand<PrepareCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[path]")]
        public string? Path { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var files = fileDiscoverer.Discover(settings.Path ?? Directory.GetCurrentDirectory());

        foreach (var file in files)
            await PrepareFile(file);

        return 0;
    }

    private static async Task PrepareFile(FileInfo file)
    {
        var metadataFile = new FileInfo(Path.ChangeExtension(file.FullName, ".json"));
        if (metadataFile.Exists)
        {
            AnsiConsole.MarkupLine($"[yellow]Skipping [bold]{metadataFile.FullName}[/]:[/] File already exists");

            return;
        }

        var metadata = new ModelMetadata
        {
            Name = Path.GetFileNameWithoutExtension(file.Name),
            Description = "",
        };

        var json = JsonSerializer.Serialize(metadata, ModelSerializerContext.Default.ModelMetadata);

        await File.WriteAllTextAsync(metadataFile.FullName, json);

        AnsiConsole.MarkupLine($"[green]Successfully prepared [bold]{metadataFile.FullName}[/][/]");
    }
}
