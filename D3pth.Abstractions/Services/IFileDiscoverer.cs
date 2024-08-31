namespace D3pth.Abstractions.Services;

public interface IFileDiscoverer
{
    IEnumerable<FileInfo> Discover(string path, string extension = "stl");
}
