using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

public class Chunk : MonoBehaviour
{
    public int3 dims;
    public List<int> voxels;
    public NativeArray<Face> faces;
    // public Mesh mesh;
            
    private void Start()
    {
        ComputeFaces();
        ComputeMesh();
        GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Blocks.Instance.opaqueTexture2DArray);
        GetComponent<MeshRenderer>().materials[1].SetTexture("_TextureArray", Blocks.Instance.alphaClipTexture2DArray);
        GetComponent<MeshRenderer>().materials[2].SetTexture("_TextureArray", Blocks.Instance.transTexture2DArray);
    }

    private void ComputeFaces()
    {
        var numFaces = dims.x * dims.y * dims.z * 6;
        faces = new NativeArray<Face>(numFaces, Allocator.Temp);
        for (var i = 0; i < numFaces; i++)
        {
            var side = i % 6;
            var voxelIndex = i / 6; // Calculate the voxel index from the job index
            var voxelXyz = new int3(voxelIndex % dims.y, voxelIndex / (dims.y * dims.z), voxelIndex / dims.y % dims.z); // YZX => XYZ
            var voxel = voxels[voxelIndex];

            var adjacentVoxelXyz = voxelXyz + Offsets[side];
            var adjacentVoxelIndex = GetYZXIndex(adjacentVoxelXyz, dims);
            var adjacentIsWithinBounds = IsWithinBounds(adjacentVoxelXyz, dims);
            var adjacentVoxel = adjacentIsWithinBounds ? voxels[adjacentVoxelIndex] : 0;

            if (IsFaceVisible(Blocks.Instance.blocks[voxel].type, Blocks.Instance.blocks[adjacentVoxel].type))
                faces[i] = new Face {TextureIndex = Blocks.Instance.blocks[voxel].textureArrayIndex, BlockType = Blocks.Instance.blocks[voxel].type};
        }
    }

    private void ComputeMesh()
    {
        var meshDataArray = Mesh.AllocateWritableMeshData(1);
        var meshData = meshDataArray[0];
        
        meshData.SetVertexBufferParams(faces.Length * 4, new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2)
        });
       
        var vertices = meshData.GetVertexData<MyVertex>();
        var opaqueTriangles = new NativeList<int>(Allocator.Temp);
        var transparentTriangles = new NativeList<int>(Allocator.Temp);
        var alphaClipTriangles = new NativeList<int>(Allocator.Temp);

        // vertices and triangles
        for (var i = 0; i < faces.Length; i++)
        {
            if (faces[i].Equals(default(Face)))
                continue;
                
            var frontOrBack = i % 2;
            var voxelIndex = i / 6; // Calculate the voxel index from the job index
            var side = (Side) (i % 6);
            var voxelXyz = new int3(voxelIndex % dims.y, voxelIndex / (dims.y * dims.z), voxelIndex / dims.y % dims.z); // YZX => XYZ

            // 2B textureIndex 
            // 4b voxelBlockLight; 4b skyLight
            var face = faces[i]; // Get the face using the index
            var textureIndexHighByte = (byte) ((face.TextureIndex & 0xFF00) >> 8);
            var textureIndexLowByte = (byte) (face.TextureIndex & 0x00FF);
            // var color = new Color32(face.BlockLight, face.SkyLight, textureIndexHighByte, textureIndexLowByte);
            var color = new Color32(0, 0, textureIndexHighByte, textureIndexLowByte);
            half h0 = half(0f), h1 = half(1f);
            var vertexIndex = i * 4;
            
            
            // case statement on face.BlockType to set triangles
            NativeList<int> triangles;
            switch (face.BlockType)
            {
                case BlockType.Opaque:
                    triangles = opaqueTriangles;
                    break;
                case BlockType.Transparent:
                    triangles = transparentTriangles;
                    break;
                case BlockType.AlphaClip:
                    triangles = alphaClipTriangles;
                    break;
                default:
                    triangles = opaqueTriangles;
                    break;
            }
            
            if (side == Side.East)
            {
                vertices[vertexIndex + 0] = new MyVertex {Position = new float3(1f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                vertices[vertexIndex + 1] = new MyVertex {Position = new float3(1f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                vertices[vertexIndex + 2] = new MyVertex {Position = new float3(1f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                vertices[vertexIndex + 3] = new MyVertex {Position = new float3(1f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
            }
            else if (side == Side.Up)
            {
                vertices[vertexIndex + 0] = new MyVertex {Position = new float3(0f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                vertices[vertexIndex + 1] = new MyVertex {Position = new float3(0f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                vertices[vertexIndex + 2] = new MyVertex {Position = new float3(1f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                vertices[vertexIndex + 3] = new MyVertex {Position = new float3(1f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
            }
            else if (side == Side.North)
            {
                vertices[vertexIndex + 0] = new MyVertex {Position = new float3(0f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};
                vertices[vertexIndex + 1] = new MyVertex {Position = new float3(1f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                vertices[vertexIndex + 2] = new MyVertex {Position = new float3(0f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                vertices[vertexIndex + 3] = new MyVertex {Position = new float3(1f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 2);
            }
            else if (side == Side.West)
            {
                vertices[vertexIndex + 0] = new MyVertex {Position = new float3(0f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                vertices[vertexIndex + 1] = new MyVertex {Position = new float3(0f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                vertices[vertexIndex + 2] = new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};
                vertices[vertexIndex + 3] = new MyVertex {Position = new float3(0f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 2);
            }
            else if (side == Side.Down)
            {
                vertices[vertexIndex + 0] = new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                vertices[vertexIndex + 1] = new MyVertex {Position = new float3(1f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};
                vertices[vertexIndex + 2] = new MyVertex {Position = new float3(0f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                vertices[vertexIndex + 3] = new MyVertex {Position = new float3(1f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 2);
            }
            else if (side == Side.South)
            {
                vertices[vertexIndex + 0] = new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                vertices[vertexIndex + 1] = new MyVertex {Position = new float3(0f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                vertices[vertexIndex + 2] = new MyVertex {Position = new float3(1f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};
                vertices[vertexIndex + 3] = new MyVertex {Position = new float3(1f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 2);
            }
        }

        // Mesh opaque, transparent, and alphaClip triangles
        meshData.SetIndexBufferParams(opaqueTriangles.Length + alphaClipTriangles.Length + transparentTriangles.Length, IndexFormat.UInt16);
        var indexBuffer = meshData.GetIndexData<ushort>();
        for (var i = 0; i < opaqueTriangles.Length; i++)
            indexBuffer[i] = (ushort) opaqueTriangles[i];
        for (var i = 0; i < alphaClipTriangles.Length; i++)
            indexBuffer[opaqueTriangles.Length +  i] = (ushort) alphaClipTriangles[i];
        for (var i = 0; i < transparentTriangles.Length; i++)
            indexBuffer[opaqueTriangles.Length + alphaClipTriangles.Length + i] = (ushort) transparentTriangles[i];
        
        
        
        // Set subMeshes
        meshData.subMeshCount = 3;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, opaqueTriangles.Length));
        meshData.SetSubMesh(1, new SubMeshDescriptor(opaqueTriangles.Length, alphaClipTriangles.Length));
        meshData.SetSubMesh(2, new SubMeshDescriptor(opaqueTriangles.Length + alphaClipTriangles.Length, transparentTriangles.Length));
        
        
        var mesh = new Mesh();
        mesh.name = "ChunkMesh";
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
    }


    private int GetYZXIndex(int3 voxelXyz, int3 Dims)
    {
        return voxelXyz.y * Dims.z * Dims.x + voxelXyz.z * Dims.x + voxelXyz.x;
    }

    private bool IsWithinBounds(int3 voxel, int3 dimensions)
    {
        return voxel.x >= 0 && voxel.x < dimensions.x &&
               voxel.y >= 0 && voxel.y < dimensions.y &&
               voxel.z >= 0 && voxel.z < dimensions.z;
    }

    private bool IsFaceVisible(BlockType blockType, BlockType otherBlockType)
    {
        return blockType switch
        {
            BlockType.Opaque => otherBlockType != BlockType.Opaque,
            BlockType.AlphaClip => otherBlockType != BlockType.Opaque,
            BlockType.Transparent => otherBlockType != BlockType.Opaque && otherBlockType != BlockType.Transparent,
            _ => false
        };
    }

    public struct MyVertex
    {
        public float3 Position;
        public Color32 Color;
        public half2 TexCoord0;
    }

    public struct Face
    {
        public int TextureIndex;
        public BlockType BlockType;
        public byte SkyLight;
        public byte BlockLight;
    }
    
    public enum Side : byte
    {
        East,
        Up,
        North,
        West,
        Down,
        South
    } // %3 => xyz;  // Voxels have sides

    private static readonly int3[] Offsets =
    {
        new(1, 0, 0), // east
        new(0, 1, 0), // up
        new(0, 0, 1), // north
        new(-1, 0, 0), // west
        new(0, -1, 0), // down
        new(0, 0, -1) // south
    };
}