using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace D3pth.CLI.Settings;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal class PrepareSettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    public string? Path { get; set; }
}
