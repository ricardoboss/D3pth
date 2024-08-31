using System.Diagnostics.CodeAnalysis;
using D3pth.Abstractions.Services;

namespace D3pth.Sdk.Services;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class RecursiveFileDiscoverer : IFileDiscoverer
{
    public IEnumerable<FileInfo> Discover(string path, string extension = "stl")
    {
        return Directory.EnumerateFiles(path, $"*.{extension}", SearchOption.AllDirectories)
            .Select(file => new FileInfo(file));
    }
}
