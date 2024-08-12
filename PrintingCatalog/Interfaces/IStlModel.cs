namespace PrintingCatalog.Interfaces;

public interface IStlModel
{
    IModelMetadata Metadata { get; }

    Triangle[] Triangles { get; }
}
