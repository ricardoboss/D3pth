namespace PrintingCatalog.Interfaces;

public interface IFileDiscoverer
{
    IAsyncEnumerable<FileInfo> DiscoverAsync(string path, CancellationToken cancellationToken = default,
        params string[] extensions);
}