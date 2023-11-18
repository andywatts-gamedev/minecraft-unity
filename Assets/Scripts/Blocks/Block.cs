using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Block", menuName = "Oasis/Block")]
public class Block : ScriptableObject
{
    public string Name;
    public BlockType Type;
    public SideTexture[] SideTextures;
    public ModelElement[] ModelElements;
    public Texture2D image;

    public Mesh GenerateMesh()
    {
        var numFaces = 6;
        var meshDataArray = Mesh.AllocateWritableMeshData(1);
        var meshData = meshDataArray[0];
        meshData.SetVertexBufferParams(numFaces * 4, new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3, stream: 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3, stream: 0),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, dimension: 4, stream: 0),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, dimension: 2, stream: 1),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, dimension: 4, stream: 2)
        });
        meshData.SetIndexBufferParams(numFaces * 6, IndexFormat.UInt16);

        // VERTICES
        var vertexStream0 = meshData.GetVertexData<VertexStream0>(0);
        var offset = new float3(0.5f, 0.5f, 0.5f);
        
        // East
        vertexStream0[0 * 4 + 0] = new VertexStream0 {Position = new float3(1f, 0f, 0f) - offset };
        vertexStream0[0 * 4 + 1] = new VertexStream0 {Position = new float3(1f, 1f, 0f) - offset };
        vertexStream0[0 * 4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 1f) - offset };
        vertexStream0[0 * 4 + 3] = new VertexStream0 {Position = new float3(1f, 0f, 1f) - offset };
        
        // Up
        vertexStream0[1 * 4 + 0] = new VertexStream0 {Position = new float3(0f, 1f, 0f) - offset };
        vertexStream0[1 * 4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 1f) - offset };
        vertexStream0[1 * 4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 1f) - offset };
        vertexStream0[1 * 4 + 3] = new VertexStream0 {Position = new float3(1f, 1f, 0f) - offset };
        
        // North
        vertexStream0[2 * 4 + 0] = new VertexStream0 {Position = new float3(1f, 0f, 1f) - offset };
        vertexStream0[2 * 4 + 1] = new VertexStream0 {Position = new float3(1f, 1f, 1f) - offset };
        vertexStream0[2 * 4 + 2] = new VertexStream0 {Position = new float3(0f, 1f, 1f) - offset };
        vertexStream0[2 * 4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 1f) - offset };
        
        // West
        vertexStream0[3 * 4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 1f) - offset };
        vertexStream0[3 * 4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 1f) - offset };
        vertexStream0[3 * 4 + 2] = new VertexStream0 {Position = new float3(0f, 1f, 0f) - offset };
        vertexStream0[3 * 4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 0f) - offset };
        
        // Down
        vertexStream0[4 * 4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 0f) - offset };
        vertexStream0[4 * 4 + 1] = new VertexStream0 {Position = new float3(1f, 0f, 0f) - offset };
        vertexStream0[4 * 4 + 2] = new VertexStream0 {Position = new float3(1f, 0f, 1f) - offset };
        vertexStream0[4 * 4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 1f) - offset };
        
        // South
        vertexStream0[5 * 4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 0f) - offset };
        vertexStream0[5 * 4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 0f) - offset };
        vertexStream0[5 * 4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 0f) - offset };
        vertexStream0[5 * 4 + 3] = new VertexStream0 {Position = new float3(1f, 0f, 0f) - offset };
        
        
        
        // Triangles
        var triangles = meshData.GetIndexData<ushort>();
        var texCoords = meshData.GetVertexData<half2>(1);
        var colors = meshData.GetVertexData<Color32>(2);
        var opaqueTriangles = new NativeList<ushort>(Allocator.Temp);
        var transparentTriangles = new NativeList<ushort>(Allocator.Temp);
        var alphaClipTriangles = new NativeList<ushort>(Allocator.Temp);
        
        NativeList<ushort> tris;
        if (Type == BlockType.AlphaClip)
            tris = alphaClipTriangles;
        else if (Type == BlockType.Transparent)
            tris = transparentTriangles;
        else 
            tris = opaqueTriangles;
        
        for (var i = 0; i < 6; i++)
        {
            // Tris
            tris.Add((ushort)(i*4 + 0));
            tris.Add((ushort)(i*4 + 1));
            tris.Add((ushort)(i*4 + 2));
            tris.Add((ushort)(i*4 + 2));
            tris.Add((ushort)(i*4 + 3));
            tris.Add((ushort)(i*4 + 0));
            
            // UVs
            texCoords[i*4 + 0] = (half)0;
            texCoords[i*4 + 1] = new half2((half)0, (half)1);
            texCoords[i*4 + 2] = (half)1;
            texCoords[i*4 + 3] = new half2((half)1, (half)0);

            // TextureIndex
            var textureIndex = (byte) SideTextures[i].TextureObject.TextureIndex;
            colors[i*4 + 0] = new Color32(0, 0, 0,textureIndex);
            colors[i*4 + 1] = new Color32(0, 0, 0,textureIndex);
            colors[i*4 + 2] = new Color32(0, 0, 0,textureIndex);
            colors[i*4 + 3] = new Color32(0, 0, 0,textureIndex); 
        }
        
        for (var i = 0; i < opaqueTriangles.Length; i++)
            triangles[i] = opaqueTriangles[i];
        for (var i = 0; i < alphaClipTriangles.Length; i++)
            triangles[opaqueTriangles.Length +  i] = alphaClipTriangles[i];
        for (var i = 0; i < transparentTriangles.Length; i++)
            triangles[opaqueTriangles.Length + alphaClipTriangles.Length + i] = transparentTriangles[i];

        
        meshData.subMeshCount = 3;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, opaqueTriangles.Length));
        meshData.SetSubMesh(1, new SubMeshDescriptor(opaqueTriangles.Length, alphaClipTriangles.Length));
        meshData.SetSubMesh(2, new SubMeshDescriptor(opaqueTriangles.Length + alphaClipTriangles.Length, transparentTriangles.Length));

        var mesh = new Mesh();
        mesh.name = name;
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}


[System.Serializable]
public struct SideTexture
{
    public Side Side;
    public TextureObject TextureObject;
}
// East, Up, North, West, Down, South



[System.Serializable]
public struct ModelElement
{
    public float3 From; // Cross is float
    public float3 To;
    public TextureObject East;
    public TextureObject North;
    public TextureObject Up;
    public TextureObject West;
    public TextureObject South;
    public TextureObject Down; // TODO Cullface
    public int4 UpUvs;
    public int4 DownUvs;
    public int4 NorthUvs;
    public int4 SouthUvs;
    public int4 EastUvs;
    public int4 WestUvs;

    public int3 RotationOrigin;
    public int RotationAxis;
    public int RotationAngle;
    public bool RotationRescale;
}