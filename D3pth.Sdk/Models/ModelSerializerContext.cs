using System.Text.Json.Serialization;
using D3pth.Abstractions;

namespace D3pth.Sdk.Models;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ModelMetadata))]
[JsonSerializable(typeof(Plane))]
public sealed partial class ModelSerializerContext : JsonSerializerContext;
