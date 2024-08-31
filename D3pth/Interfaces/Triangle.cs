using System.Numerics;

namespace D3pth.Interfaces;

public record Triangle(Vector3 Normal, Vector3 A, Vector3 B, Vector3 C, ushort AttributeByteCount);