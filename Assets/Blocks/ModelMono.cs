using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;


[Serializable]
public struct KVP
{
    public string Key;
    public string Value;
}


[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class ModelMono : MonoBehaviour
{
    public Block block;
    [SerializeField] public KVP[] states;
    
    private Mesh.MeshDataArray meshDataArray;

    private void Start()
    {
        if (block.Type != BlockType.Model) return;

        var numFaces = ComputeNumFaces();
        var meshData = CreateMeshData(numFaces);
        var verts = meshData.GetVertexData<VertexStream0>();
        var triangles = meshData.GetIndexData<ushort>();
        var texCoord0 = meshData.GetVertexData<half2>(1);
        var texCoord1 = meshData.GetVertexData<float>(2);
        var colors = meshData.GetVertexData<Color32>(3);
        var opaqueTriangles = new NativeList<ushort>(Allocator.Temp);
        var transparentTriangles = new NativeList<ushort>(Allocator.Temp);
        var alphaClipTriangles = new NativeList<ushort>(Allocator.Temp);


        NativeList<ushort> tris;
        if (block.TextureType == TextureType.AlphaClip)
            tris = alphaClipTriangles;
        else if (block.TextureType == TextureType.Transparent)
            tris = transparentTriangles;
        else
            tris = opaqueTriangles;

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
                AddTexCoord0(texCoord0, f, element.EastUvs);
                AddTexCoord1(texCoord1, f, element.EastUvs);
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
                AddTexCoord0(texCoord0, f, element.UpUvs); // Todo uvlock
                AddTexCoord1(texCoord1, f, element.UpUvs);
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
                AddTexCoord0(texCoord0, f, element.NorthUvs);
                AddTexCoord1(texCoord1, f, element.NorthUvs);
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
                AddTexCoord0(texCoord0, f, element.WestUvs);
                AddTexCoord1(texCoord1, f, element.WestUvs);
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
                AddTexCoord0(texCoord0, f, element.DownUvs);
                AddTexCoord1(texCoord1, f, element.DownUvs);
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
                AddTexCoord0(texCoord0, f, element.SouthUvs);
                AddTexCoord1(texCoord1, f, element.SouthUvs);
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
        
        
        // rotate if state contains "y" key
        if (states != null)
        {
            foreach (var state in states)
                if (state.Key == "y")
                {
                    var angle = int.Parse(state.Value.ToString());
                    transform.rotation = Quaternion.Euler(0, angle, 0);
                }
        }
    }
    

    // “My suggestion is not to pack the floats at all, *just use 12 floats spread across different UV sets*.” - bgolus
    private void AddColors(int i, byte textureIndex, ref NativeArray<Color32> colors)
    {
        const byte metallic = 0;
        const byte smoothness = 0;
        colors[i * 4 + 0] = new Color32(0, metallic, smoothness, textureIndex);
        colors[i * 4 + 1] = new Color32(0, metallic, smoothness, textureIndex);
        colors[i * 4 + 2] = new Color32(0, metallic, smoothness, textureIndex);
        colors[i * 4 + 3] = new Color32(0, metallic, smoothness, textureIndex);
    }

    // MC UVs start in top left, so y = 16-y
    private static NativeArray<half2> AddTexCoord0(NativeArray<half2> texCoord0, int i, int4 ii)
    {
        texCoord0[i * 4 + 0] = half2((half)(ii.x/16f),  (half)(ii.y/16f));
        texCoord0[i * 4 + 1] = half2((half)(ii.x/16f),  (half)(ii.w/16f));
        texCoord0[i * 4 + 2] = half2((half)(ii.z/16f),  (half)(ii.w/16f));
        texCoord0[i * 4 + 3] = half2((half)(ii.z/16f),  (half)(ii.y/16f));
        return texCoord0;
    }

    // Face UVs
    private static NativeArray<float> AddTexCoord1(NativeArray<float> texCoord1, int i, int4 uvs)
    {
        texCoord1[i * 4 + 0] = (uvs[0]/16f);
        texCoord1[i * 4 + 1] = (uvs[1]/16f);
        texCoord1[i * 4 + 2] = (uvs[2]/16f);
        texCoord1[i * 4 + 3] = (uvs[3]/16f);
        return texCoord1;
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
        meshData.SetVertexBufferParams(numFaces * 4, new[]
        {
            new VertexAttributeDescriptor( VertexAttribute.Position, dimension: 3, stream: 0),
            new VertexAttributeDescriptor( VertexAttribute.Normal, dimension: 3, stream: 0 ),
            new VertexAttributeDescriptor( VertexAttribute.Tangent, VertexAttributeFormat.Float16, dimension: 4, stream: 0 ),
            new VertexAttributeDescriptor( VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, dimension: 2, stream: 1 ),
            new VertexAttributeDescriptor( VertexAttribute.TexCoord1, VertexAttributeFormat.UNorm8, dimension: 4, stream: 2 ),  // ModelSizeX, ModelSizeY, ModelStartX, ModelStartY
            new VertexAttributeDescriptor( VertexAttribute.Color, VertexAttributeFormat.UNorm8, dimension: 4, stream: 3 ), // TextureIndex, Metallic, Smoothness, 0
        });
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