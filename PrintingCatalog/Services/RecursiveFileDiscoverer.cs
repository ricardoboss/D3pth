using System.Runtime.CompilerServices;
using PrintingCatalog.Interfaces;

namespace PrintingCatalog.Services;

public class RecursiveFileDiscoverer : IFileDiscoverer
{
    public async IAsyncEnumerable<FileInfo> DiscoverAsync(string path,
        [EnumeratorCancellation] CancellationToken cancellationToken = default,
        params string[] extensions)
    {
        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            if (extensions.Any(e => file.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                yield return new(file);
        }
    }
}
