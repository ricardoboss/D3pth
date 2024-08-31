using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using D3pth.Abstractions.Services;
using D3pth.CLI.Settings;
using D3pth.Sdk.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace D3pth.CLI.Commands;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal sealed class PrepareCommand(IFileDiscoverer fileDiscoverer) : AsyncCommand<PrepareSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, PrepareSettings prepareSettings)
    {
        var files = fileDiscoverer.Discover(prepareSettings.Path ?? Directory.GetCurrentDirectory());

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
