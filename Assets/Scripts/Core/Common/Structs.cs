using Unity.Mathematics;
using UnityEngine.Serialization;

public struct VertexStream0
{
    public float3 Position;
    public float3 Normal;
    public half4 Tangent;
}

public struct Face
{
    public ushort TextureIndex;
    public TextureType TextureType;
    public byte SkyLight;
    public byte BlockLight;
    public byte Smoothness;
    public byte Metallic;
}
