namespace PrintingCatalog.Interfaces;

public interface IStlModel
{
    IModelMetadata Metadata { get; }

    byte[] Header { get; }

    Triangle[] Triangles { get; }
}
