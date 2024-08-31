namespace D3pth.Abstractions;

public interface IFileDiscoverer
{
    IEnumerable<FileInfo> Discover(string path, string extension = "stl");
}
