using PrintingCatalog.Interfaces;

namespace PrintingCatalog.Models;

public class StlModel : IStlModel
{
    public required IModelMetadata Metadata { get; init; }

    public required byte[] Header { get; init;  }

    public required Triangle[] Triangles { get; init; }
}
