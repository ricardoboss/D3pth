using System.ComponentModel;
using System.Diagnostics;
using PrintingCatalog.Interfaces;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PrintingCatalog.Commands;

internal sealed class GenerateCommand(ICatalogGenerator generator) : AsyncCommand<GenerateCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-p|--preview")]
        [Description("Opens the generated catalog in the previewer")]
        public bool Preview { get; set; } = false;

        [CommandOption("-o|--output")]
        [Description("The path to the output file")]
        public string? OutputFile { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[green]Generating catalog...[/]");

        var stopwatch = Stopwatch.StartNew();
        var catalog = await generator.GenerateAsync(Directory.GetCurrentDirectory());
        stopwatch.Stop();

        AnsiConsole.MarkupLine($"[green]Generated catalog in [bold]{stopwatch.Elapsed}[/][/]");

        if (settings.Preview)
        {
            var document = catalog as IDocument ?? throw new NotSupportedException(
                $"Expected QuestPDF.Infrastructure.IDocument, but got {catalog.GetType().FullName} instead");

            AnsiConsole.MarkupLine("[green]Opening previewer...[/]");

            await document.ShowInPreviewerAsync();
        }
        else if (settings.OutputFile is { } outFile)
            await StoreOnDiskAsync(catalog, outFile);
        else
            throw new ArgumentException("Either --preview or --output must be specified");

        AnsiConsole.MarkupLine("[green]Done![/]");

        return 0;
    }

    private static async Task StoreOnDiskAsync(ICatalog catalog, string path)
    {
        AnsiConsole.MarkupLine($"[green]Saving to '[bold]{path}[/]'[/]");

        await using var stream = File.OpenWrite(path);

        await stream.WriteAsync(catalog.GeneratePdf());
    }
}
