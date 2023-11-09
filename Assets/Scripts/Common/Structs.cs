using Unity.Mathematics;

public struct VertexStream0
{
    public float3 Position;
    public float3 Normal;
    public half4 Tangent;
}

public struct Face
{
    public ushort TextureIndex;
    public BlockType BlockType;
    public byte SkyLight;
    public byte BlockLight;
}
