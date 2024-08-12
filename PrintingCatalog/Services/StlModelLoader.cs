using System.Text.Json;
using PrintingCatalog.Interfaces;
using PrintingCatalog.Models;

namespace PrintingCatalog.Services;

public class StlModelLoader : IStlModelLoader
{
    public async Task<IStlModel> LoadAsync(FileInfo file, CancellationToken cancellationToken = default)
    {
        var triangles = await ReadTriangles(file, cancellationToken);
        var metadata = await ReadMetadata(file, cancellationToken);

        return new StlModel
        {
            Metadata = metadata,
            Triangles = triangles,
        };
    }

    private static async Task<IModelMetadata> ReadMetadata(FileInfo file, CancellationToken cancellationToken)
    {
        ModelMetadata? metadata = null;

        var metadataFile = new FileInfo(Path.ChangeExtension(file.FullName, ".json"));
        if (metadataFile.Exists)
        {
            await using var stream = metadataFile.OpenRead();
            using var reader = new StreamReader(stream);

            var json = await reader.ReadToEndAsync(cancellationToken);
            metadata = JsonSerializer.Deserialize(json,  ModelSerializerContext.Default.ModelMetadata);
        }

        return metadata ?? new ModelMetadata
        {
            Name = Path.GetFileNameWithoutExtension(file.Name),
        };
    }

    private static async Task<Triangle[]> ReadTriangles(FileInfo file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenRead();
        using var reader = new BinaryReader(stream);

        var header = new byte[80];
        if (reader.Read(header, 0, 80) != 80)
            throw new InvalidDataException("Invalid STL file: header is missing or incomplete");

        if (header.AsSpan().StartsWith("solid"u8))
            throw new NotSupportedException("ASCII STL files are not supported");

        var numberOfTriangles = reader.ReadUInt32();
        if (numberOfTriangles == 0)
            throw new InvalidDataException("Invalid STL file: number of triangles is zero");

        var triangles = new Triangle[numberOfTriangles];
        for (var i = 0; i < numberOfTriangles; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var normalX = reader.ReadSingle();
            var normalY = reader.ReadSingle();
            var normalZ = reader.ReadSingle();

            var aX = reader.ReadSingle();
            var aY = reader.ReadSingle();
            var aZ = reader.ReadSingle();

            var bX = reader.ReadSingle();
            var bY = reader.ReadSingle();
            var bZ = reader.ReadSingle();

            var cX = reader.ReadSingle();
            var cY = reader.ReadSingle();
            var cZ = reader.ReadSingle();

            var attributeByteCount = reader.ReadUInt16();

            var triangle = new Triangle(
                Normal: new(normalX, normalY, normalZ),
                A: new(aX, aY, aZ),
                B: new(bX, bY, bZ),
                C: new(cX, cY, cZ),
                AttributeByteCount: attributeByteCount
            );

            triangles[i] = triangle;
        }

        return triangles;
    }
}
