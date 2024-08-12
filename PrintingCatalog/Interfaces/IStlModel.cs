namespace PrintingCatalog.Interfaces;

public interface IStlModel
{
    FileInfo File { get; }

    IModelMetadata Metadata { get; }

    Triangle[] Triangles { get; }

    string Md5Hash { get; }
}
