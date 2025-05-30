using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using D3pth.Abstractions.Rendering;
using Spectre.Console.Cli;

namespace D3pth.CLI.Settings;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal class RenderSettings : CommandSettings
{
    [CommandArgument(0, "[file]")] public string? File { get; init; }

    [CommandOption("-m|--mode")]
    [Description("Render mode (shaded, depth, wireframe)")]
    public RenderMode Mode { get; init; } = RenderMode.Shaded;

    [CommandOption("--grid")]
    [Description("Draw a grid in the background")]
    public bool DrawGrid { get; init; }

    [CommandOption("--size")]
    public int RenderSize { get; init; } = 1024;
}
