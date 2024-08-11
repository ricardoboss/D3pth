using System.ComponentModel;
using System.Diagnostics;
using PrintingCatalog.Interfaces;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
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
        var catalog = await generator.GenerateAsync(Directory.GetCurrentDirectory());

        if (settings.Preview)
        {
            var document = catalog as IDocument ?? throw new NotSupportedException(
                $"Expected QuestPDF.Infrastructure.IDocument, but got {catalog.GetType().FullName} instead");

            await document.ShowInPreviewerAsync();
        }
        else if (settings.OutputFile is { } outFile)
            await StoreOnDiskAsync(catalog, outFile);
        else
            throw new ArgumentException("Either --preview or --output must be specified");

        return 0;
    }

    private async Task StoreOnDiskAsync(ICatalog catalog, string path)
    {
        throw new NotImplementedException();
    }
}
