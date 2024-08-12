using System.Numerics;
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
            metadata = JsonSerializer.Deserialize(json, ModelSerializerContext.Default.ModelMetadata);
        }

        var fallbackName = Path.GetFileNameWithoutExtension(file.Name);

        metadata ??= new();

        if (string.IsNullOrEmpty(metadata.Name))
            metadata.Name = fallbackName;

        metadata.CatalogNumber ??= ((uint)metadata.Name.ToUpper().GetHashCode() % 9999).ToString().PadLeft(4, '0');

        return metadata;
    }

    private static async Task<Triangle[]> ReadTriangles(FileInfo file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenRead();
        using var reader = new BinaryReader(stream);

        var header = new byte[80];
        if (reader.Read(header, 0, 80) != 80)
            throw new InvalidDataException($"Invalid STL file: header is missing or incomplete ({file.FullName})");

        // if (header.AsSpan().StartsWith("solid"u8))
        //     throw new NotSupportedException($"ASCII STL files are not supported ({file.FullName})");

        var numberOfTriangles = reader.ReadUInt32();
        if (numberOfTriangles == 0)
            throw new InvalidDataException($"Invalid STL file: number of triangles is zero ({file.FullName})");

        var triangles = new Triangle[numberOfTriangles];
        for (var i = 0; i < numberOfTriangles; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            // skip normal
            _ = reader.ReadSingle();
            _ = reader.ReadSingle();
            _ = reader.ReadSingle();

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

            var a = new Vector3(aX, aY, aZ);
            var b = new Vector3(bX, bY, bZ);
            var c = new Vector3(cX, cY, cZ);

            var triangle = new Triangle(
                Normal: CalculateNormal(a, b, c),
                A: a,
                B: b,
                C: c,
                AttributeByteCount: attributeByteCount
            );

            triangles[i] = triangle;
        }

        return triangles;
    }

    private static Vector3 CalculateNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // Calculate the edge vectors
        var edge1 = v2 - v1;
        var edge2 = v3 - v1;

        // Compute the cross product using the right-hand rule
        var normal = Vector3.Cross(edge1, edge2);

        // Normalize the normal vector to ensure it's a unit vector
        normal = Vector3.Normalize(normal);

        return normal;
    }
}