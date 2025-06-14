using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Security.Cryptography;
using D3pth.Abstractions.Models;
using D3pth.Abstractions.Services;
using D3pth.Sdk.Models;

namespace D3pth.Sdk.Services;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class StlModelLoader : IStlModelLoader
{
    public async Task<IStlModel> LoadAsync(FileInfo file, CancellationToken cancellationToken = default)
    {
        var triangles = await ReadTriangles(file, cancellationToken);
        var md5Hash = await CalculateMd5Hash(file, cancellationToken);

        return new StlModel
        {
            File = file,
            Triangles = triangles,
            Md5Hash = md5Hash,
        };
    }

    private static async Task<string> CalculateMd5Hash(FileInfo file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenRead();
        using var md5 = MD5.Create();
        var hash = await md5.ComputeHashAsync(stream, cancellationToken);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
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

        if (numberOfTriangles > 20_000_000)
            throw new InvalidDataException($"Invalid STL file: number of triangles is too high (> 20M; {file.FullName})");

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
