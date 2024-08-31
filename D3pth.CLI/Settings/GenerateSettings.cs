using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace D3pth.CLI.Settings;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal class GenerateSettings : CommandSettings
{
    [CommandOption("-p|--preview")]
    [Description("Opens the generated catalog in the previewer")]
    public bool Preview { get; set; } = false;

    [CommandOption("-o|--output")]
    [Description("The path to the output file")]
    public string? OutputFile { get; set; }
}
