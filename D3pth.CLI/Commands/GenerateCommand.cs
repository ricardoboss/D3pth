using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using D3pth.Abstractions.Catalog;
using D3pth.CLI.Settings;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using Spectre.Console;
using Spectre.Console.Cli;

namespace D3pth.CLI.Commands;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal sealed class GenerateCommand(ICatalogGenerator generator) : AsyncCommand<GenerateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateSettings settings)
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
            await StoreOnDiskAsync(catalog, new(outFile));
        else
            throw new ArgumentException("Either --preview or --output must be specified");

        AnsiConsole.MarkupLine("[green]Done![/]");

        return 0;
    }

    private static async Task StoreOnDiskAsync(ICatalog catalog, FileInfo outputFile)
    {
        AnsiConsole.MarkupLine($"[green]Generating PDF...[/]");

        var stopwatch = Stopwatch.StartNew();
        var pdf = catalog.GeneratePdf();
        stopwatch.Stop();

        AnsiConsole.MarkupLine($"[green]Generated PDF in [bold]{stopwatch.Elapsed}[/][/]");

        AnsiConsole.MarkupLine($"[green]Saving to '[bold]{outputFile.FullName}[/]'...[/]");

        if (outputFile.Exists)
            outputFile.Delete();

        await using var stream = outputFile.OpenWrite();

        await stream.WriteAsync(pdf);
    }
}
