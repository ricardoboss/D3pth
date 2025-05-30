using System.Collections;

namespace D3pth.Abstractions.Models;

public class CatalogModelCollection(IEnumerable<CatalogEntry> entries) : IReadOnlyCollection<CatalogEntry>
{
    private readonly Dictionary<string, CatalogEntry> dict =
        new(entries.Select(e => new KeyValuePair<string, CatalogEntry>(e.Model.Md5Hash, e)));

    public CatalogEntry this[string md5Hash] => dict[md5Hash];

    public IEnumerator<CatalogEntry> GetEnumerator() => dict.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => dict.Count;
}
