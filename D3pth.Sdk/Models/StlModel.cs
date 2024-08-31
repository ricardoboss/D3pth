using D3pth.Abstractions.Models;

namespace D3pth.Sdk.Models;

public class StlModel : IStlModel
{
    public required FileInfo File { get; init; }

    public required IModelMetadata Metadata { get; init; }

    public required Triangle[] Triangles { get; init; }

    public required string Md5Hash { get; init; }
}
