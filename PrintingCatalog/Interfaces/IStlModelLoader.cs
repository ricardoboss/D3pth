namespace PrintingCatalog.Interfaces;

public interface IStlModelLoader
{
    Task<IStlModel> LoadAsync(FileInfo file, CancellationToken cancellationToken = default);
}
