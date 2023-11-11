using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class ModelMono : MonoBehaviour
{
    public Block block;
    private Mesh.MeshDataArray meshDataArray;

    private void Start()
    {
        if (block.Type != BlockType.Model) return;

        var numFaces = ComputeNumFaces();
        var meshData = CreateMeshData(numFaces);
        var verts = meshData.GetVertexData<VertexStream0>();
        var triangles = meshData.GetIndexData<ushort>();
        var texCoords = meshData.GetVertexData<half2>(1);
        var colors = meshData.GetVertexData<Color32>(2);
        var opaqueTriangles = new NativeList<ushort>(Allocator.Temp);
        var transparentTriangles = new NativeList<ushort>(Allocator.Temp);
        var alphaClipTriangles = new NativeList<ushort>(Allocator.Temp);
        var tris = alphaClipTriangles;

        var f = 0;
        foreach (var element in block.ModelElements)
        {
            if (element.East != null)
            {
                verts[f*4+0] = new VertexStream0 {Position = new float3(element.To.x/16f, element.From.y/16f, element.From.z/16f)};
                verts[f*4+1] = new VertexStream0 {Position = new float3(element.To.x/16f, element.To.y/16f, element.From.z/16f)};
                verts[f*4+2] = new VertexStream0 {Position = new float3(element.To.x/16f, element.To.y/16f, element.To.z/16f)};
                verts[f*4+3] = new VertexStream0 {Position = new float3(element.To.x/16f, element.From.y/16f, element.To.z/16f)};
                AddTris(tris, f);
                AddTexCoords(texCoords, f, element.EastUvs);
                AddColors(f, (byte)element.East.TextureIndex, ref colors);
                f++;
            }

            if (element.Up != null)
            {
                verts[f*4+0] = new VertexStream0 {Position = new float3(element.From.x/16f, element.To.y/16f, element.From.z/16f)};
                verts[f*4+1] = new VertexStream0 {Position = new float3(element.From.x/16f, element.To.y/16f, element.To.z/16f)};
                verts[f*4+2] = new VertexStream0 {Position = new float3(element.To.x/16f, element.To.y/16f, element.To.z/16f)};
                verts[f*4+3] = new VertexStream0 {Position = new float3(element.To.x/16f, element.To.y/16f, element.From.z/16f)};
                AddTris(tris, f);
                AddTexCoords(texCoords, f, element.UpUvs);
                AddColors(f, (byte)element.Up.TextureIndex, ref colors);
                f++;
            }

            if (element.North != null)
            {
                verts[f*4+0] = new VertexStream0 {Position = new float3(element.To.x/16f, element.From.y/16f, element.To.z/16f)};
                verts[f*4+1] = new VertexStream0 {Position = new float3(element.To.x/16f, element.To.y/16f, element.To.z/16f)};
                verts[f*4+2] = new VertexStream0 {Position = new float3(element.From.x/16f, element.To.y/16f, element.To.z/16f)};
                verts[f*4+3] = new VertexStream0 {Position = new float3(element.From.x/16f, element.From.y/16f, element.To.z/16f)};
                AddTris(tris, f);
                AddTexCoords(texCoords, f, element.NorthUvs);
                AddColors(f, (byte)element.North.TextureIndex, ref colors);
                f++;
            }

            if (element.West != null)
            {
                verts[f*4+0] = new VertexStream0 {Position = new float3(element.From.x/16f, element.From.y/16f, element.To.z/16f)};
                verts[f*4+1] = new VertexStream0 {Position = new float3(element.From.x/16f, element.To.y/16f, element.To.z/16f)};
                verts[f*4+2] = new VertexStream0 {Position = new float3(element.From.x/16f, element.To.y/16f, element.From.z/16f)};
                verts[f*4+3] = new VertexStream0 {Position = new float3(element.From.x/16f, element.From.y/16f, element.From.z/16f)};
                AddTris(tris, f);
                AddTexCoords(texCoords, f, element.WestUvs);
                AddColors(f, (byte)element.West.TextureIndex, ref colors);
                f++;
            }

            if (element.Down != null)
            {
                verts[f*4+0] = new VertexStream0 {Position = new float3(element.From.x/16f, element.From.y/16f, element.From.z/16f)};
                verts[f*4+1] = new VertexStream0 {Position = new float3(element.To.x/16f, element.From.y/16f, element.From.z/16f)};
                verts[f*4+2] = new VertexStream0 {Position = new float3(element.To.x/16f, element.From.y/16f, element.To.z/16f)};
                verts[f*4+3] = new VertexStream0 {Position = new float3(element.From.x/16f, element.From.y/16f, element.To.z/16f)};
                AddTris(tris, f);
                AddTexCoords(texCoords, f, element.DownUvs);
                AddColors(f, (byte)element.Down.TextureIndex, ref colors);
                f++;
            }

            if (element.South != null)
            {
                verts[f*4+0] = new VertexStream0 {Position = new float3(element.From.x/16f, element.From.y/16f, element.From.z/16f)};
                verts[f*4+1] = new VertexStream0 {Position = new float3(element.From.x/16f, element.To.y/16f, element.From.z/16f)};
                verts[f*4+2] = new VertexStream0 {Position = new float3(element.To.x/16f, element.To.y/16f, element.From.z/16f)};
                verts[f*4+3] = new VertexStream0 {Position = new float3(element.To.x/16f, element.From.y/16f, element.From.z/16f)};
                AddTris(tris, f);
                AddTexCoords(texCoords, f, element.SouthUvs);
                AddColors(f, (byte)element.South.TextureIndex, ref colors);
                f++;
            }
        }

        // Concat triangles
        for (var i = 0; i < opaqueTriangles.Length; i++)
            triangles[i] = opaqueTriangles[i];
        for (var i = 0; i < alphaClipTriangles.Length; i++)
            triangles[opaqueTriangles.Length + i] = alphaClipTriangles[i];
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
        GetComponent<MeshRenderer>().materials[1].SetTexture("_TextureArray", Textures.Instance.alphaClipTexture2DArray);
        GetComponent<MeshRenderer>().materials[2].SetTexture("_TextureArray", Textures.Instance.transTexture2DArray);
    }

    private void AddColors(int i, byte textureIndex, ref NativeArray<Color32> colors)
    {
        colors[i * 4 + 0] = new Color32(0, 0, 0, textureIndex);
        colors[i * 4 + 1] = new Color32(0, 0, 0, textureIndex);
        colors[i * 4 + 2] = new Color32(0, 0, 0, textureIndex);
        colors[i * 4 + 3] = new Color32(0, 0, 0, textureIndex);
    }

    private static NativeArray<half2> AddTexCoords(NativeArray<half2> texCoords, int i, int4 ii)
    {
                texCoords[i * 4 + 0] = half2((half)(ii.x/16f),  (half)(ii.y/16f));
                texCoords[i * 4 + 1] = half2((half)(ii.x/16f),  (half)(ii.w/16f));
                texCoords[i * 4 + 2] = half2((half)(ii.z/16f),  (half)(ii.w/16f));
                texCoords[i * 4 + 3] = half2((half)(ii.z/16f),  (half)(ii.y/16f));
        // texCoords[i * 4 + 0] = (half) 0;
        // texCoords[i * 4 + 1] = half2((half) 0, (half) 1);
        // texCoords[i * 4 + 2] = (half) 1;
        // texCoords[i * 4 + 3] = half2((half) 1, (half) 0);
        return texCoords;
    }

    private static void AddTris(NativeList<ushort> tris, int i)
    {
        tris.Add((ushort) (i * 4 + 0));
        tris.Add((ushort) (i * 4 + 1));
        tris.Add((ushort) (i * 4 + 2));
        tris.Add((ushort) (i * 4 + 2));
        tris.Add((ushort) (i * 4 + 3));
        tris.Add((ushort) (i * 4 + 0));
    }

    private Mesh.MeshData CreateMeshData(int numFaces)
    {
        meshDataArray = Mesh.AllocateWritableMeshData(1);
        var meshData = meshDataArray[0];
        meshData.SetVertexBufferParams(numFaces * 4, new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3, stream: 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3, stream: 0), new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4, 0),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2, 1), new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4, 2));
        meshData.SetIndexBufferParams(numFaces * 6, IndexFormat.UInt16);
        return meshData;
    }

    private int ComputeNumFaces()
    {
        var numFaces = 0;
        foreach (var element in block.ModelElements)
        {
            if (element.Up != null) numFaces++;
            if (element.Down != null) numFaces++;
            if (element.North != null) numFaces++;
            if (element.South != null) numFaces++;
            if (element.East != null) numFaces++;
            if (element.West != null) numFaces++;
        }

        Debug.Log(numFaces);
        return numFaces;
    }
}