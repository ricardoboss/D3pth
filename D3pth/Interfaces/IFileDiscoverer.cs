namespace D3pth.Interfaces;

public interface IFileDiscoverer
{
    IEnumerable<FileInfo> Discover(string path, string extension = "stl");
}
