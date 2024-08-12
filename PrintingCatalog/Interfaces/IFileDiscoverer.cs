namespace PrintingCatalog.Interfaces;

public interface IFileDiscoverer
{
    IEnumerable<FileInfo> Discover(string path, string extension = "stl");
}