using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class BlockMono : MonoBehaviour
{
    public Block block;

    private void Start()
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
        
        // East
        vertexStream0[0 * 4 + 0] = new VertexStream0 {Position = new float3(1f, 0f, 0f) };
        vertexStream0[0 * 4 + 1] = new VertexStream0 {Position = new float3(1f, 1f, 0f) };
        vertexStream0[0 * 4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 1f) };
        vertexStream0[0 * 4 + 3] = new VertexStream0 {Position = new float3(1f, 0f, 1f) };
        
        // Up
        vertexStream0[1 * 4 + 0] = new VertexStream0 {Position = new float3(0f, 1f, 0f) };
        vertexStream0[1 * 4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 1f) };
        vertexStream0[1 * 4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 1f) };
        vertexStream0[1 * 4 + 3] = new VertexStream0 {Position = new float3(1f, 1f, 0f) };
        
        // North
        vertexStream0[2 * 4 + 0] = new VertexStream0 {Position = new float3(1f, 0f, 1f) };
        vertexStream0[2 * 4 + 1] = new VertexStream0 {Position = new float3(1f, 1f, 1f) };
        vertexStream0[2 * 4 + 2] = new VertexStream0 {Position = new float3(0f, 1f, 1f) };
        vertexStream0[2 * 4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 1f) };
        
        // West
        vertexStream0[3 * 4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 1f) };
        vertexStream0[3 * 4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 1f) };
        vertexStream0[3 * 4 + 2] = new VertexStream0 {Position = new float3(0f, 1f, 0f) };
        vertexStream0[3 * 4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 0f) };
        
        // Down
        vertexStream0[4 * 4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 0f) };
        vertexStream0[4 * 4 + 1] = new VertexStream0 {Position = new float3(1f, 0f, 0f) };
        vertexStream0[4 * 4 + 2] = new VertexStream0 {Position = new float3(1f, 0f, 1f) };
        vertexStream0[4 * 4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 1f) };
        
        // South
        vertexStream0[5 * 4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 0f) };
        vertexStream0[5 * 4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 0f) };
        vertexStream0[5 * 4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 0f) };
        vertexStream0[5 * 4 + 3] = new VertexStream0 {Position = new float3(1f, 0f, 0f) };
        
        
        
        // Triangles
        var triangles = meshData.GetIndexData<ushort>();
        var texCoords = meshData.GetVertexData<half2>(1);
        var colors = meshData.GetVertexData<Color32>(2);
        var opaqueTriangles = new NativeList<ushort>(Allocator.Temp);
        var transparentTriangles = new NativeList<ushort>(Allocator.Temp);
        var alphaClipTriangles = new NativeList<ushort>(Allocator.Temp);
        
        NativeList<ushort> tris;
        if (block.Type == BlockType.AlphaClip)
            tris = alphaClipTriangles;
        else if (block.Type == BlockType.Transparent)
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
            texCoords[i*4 + 1] = half2((half)0, (half)1);
            texCoords[i*4 + 2] = (half)1;
            texCoords[i*4 + 3] = half2((half)1, (half)0);

            // TextureIndex
            var textureIndex = (byte) block.SideTextures[i].TextureObject.TextureIndex;
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
        mesh.name = block.name;
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
        
        GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Textures.Instance.opaqueTexture2DArray);
    }
}