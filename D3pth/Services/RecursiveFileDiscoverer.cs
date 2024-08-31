using D3pth.Interfaces;

namespace D3pth.Services;

public class RecursiveFileDiscoverer : IFileDiscoverer
{
    public IEnumerable<FileInfo> Discover(string path, string extension = "stl")
    {
        return Directory.EnumerateFiles(path, $"*.{extension}", SearchOption.AllDirectories)
            .Select(file => new FileInfo(file));
    }
}
