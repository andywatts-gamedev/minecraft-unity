using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using static Unity.Mathematics.math;

public class Chunk : MonoBehaviour
{
    public int3 dims;
    public List<int> voxels;
    public List<byte> blockLights;
    public List<byte> skyLights;
    public List<int> visibleFacesIndexes;
    public List<Face> faces;
    public Mesh.MeshDataArray meshDataArray;

    private void Start()
    {
        faces = new List<Face>();
        visibleFacesIndexes = new List<int>();
        
        ComputeFaces();
        ComputeVisibleFaces(faces);

        // meshDataArray = new Mesh.MeshDataArray();
        meshDataArray = Mesh.AllocateWritableMeshData(1);
        meshDataArray[0].SetVertexBufferParams(visibleFacesIndexes.Count * 4,
            attributes: new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2)
            }
        );
        meshDataArray[0].SetIndexBufferParams(visibleFacesIndexes.Count * 6, IndexFormat.UInt16);
        
        ComputeMesh();
        
        // Set Mesh and Material
        Mesh mesh = CreateMesh(meshDataArray[0]);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Blocks.Instance.opaqueTexture2DArray);
        GetComponent<MeshRenderer>().materials[1].SetTexture("_TextureArray", Blocks.Instance.transTexture2DArray);
    }

    private void ComputeFaces()
    {
        var numFaces = dims.x * dims.y * dims.z * 6;
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
                faces.Add(new Face { TextureIndex = voxel, BlockType = Blocks.Instance.blocks[voxel].type});
        }
    }

    private void ComputeMesh()
    {
        for (var i = 0; i < visibleFacesIndexes.Count; i++)
        {
            // Compute face voxel, side, and front or back
            var visibleFaceIndex = visibleFacesIndexes[i]; // Get the index of the face to process
            var frontOrBack = visibleFaceIndex % 2;
            var voxelIndex = visibleFaceIndex / 6; // Calculate the voxel index from the job index
            var side = (Side) (visibleFaceIndex % 6);
            var voxelXyz = new int3(voxelIndex % dims.y, voxelIndex / (dims.y * dims.z), voxelIndex / dims.y % dims.z); // YZX => XYZ

            var outputVerts = meshDataArray[0].GetVertexData<MyVertex>();
            var outputTris = meshDataArray[0].GetIndexData<ushort>();


            // 2B textureIndex 
            // 4b voxelBlockLight
            // 4b skyLight
            var face = faces[visibleFaceIndex]; // Get the face using the index
            var textureIndexHighByte = (byte) ((face.TextureIndex & 0xFF00) >> 8);
            var textureIndexLowByte = (byte) (face.TextureIndex & 0x00FF);
            // var color = new Color32(face.BlockLight, face.SkyLight, textureIndexHighByte, textureIndexLowByte);
            var color = new Color32(0, 0, textureIndexHighByte, textureIndexLowByte);

            half h0 = half(0f), h1 = half(1f);
            var vertexIndex = i * 4;
            var triangleIndex = i * 6;

            if (side == Side.East)
            {
                outputVerts[vertexIndex + 0] = new MyVertex {Position = new float3(1f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                outputVerts[vertexIndex + 1] = new MyVertex {Position = new float3(1f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                outputVerts[vertexIndex + 2] = new MyVertex {Position = new float3(1f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                outputVerts[vertexIndex + 3] = new MyVertex {Position = new float3(1f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};

                outputTris[triangleIndex + 0] = (ushort) (vertexIndex + 0);
                outputTris[triangleIndex + 1] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 2] = (ushort) (vertexIndex + 3);
                outputTris[triangleIndex + 3] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 4] = (ushort) (vertexIndex + 2);
                outputTris[triangleIndex + 5] = (ushort) (vertexIndex + 3);
            }
            else if (side == Side.Up)
            {
                outputVerts[vertexIndex + 0] = new MyVertex {Position = new float3(0f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                outputVerts[vertexIndex + 1] = new MyVertex {Position = new float3(0f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                outputVerts[vertexIndex + 2] = new MyVertex {Position = new float3(1f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                outputVerts[vertexIndex + 3] = new MyVertex {Position = new float3(1f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};

                outputTris[triangleIndex + 0] = (ushort) (vertexIndex + 0);
                outputTris[triangleIndex + 1] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 2] = (ushort) (vertexIndex + 3);
                outputTris[triangleIndex + 3] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 4] = (ushort) (vertexIndex + 2);
                outputTris[triangleIndex + 5] = (ushort) (vertexIndex + 3);
            }
            else if (side == Side.North)
            {
                outputVerts[vertexIndex + 0] = new MyVertex {Position = new float3(0f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};
                outputVerts[vertexIndex + 1] = new MyVertex {Position = new float3(1f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                outputVerts[vertexIndex + 2] = new MyVertex {Position = new float3(0f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                outputVerts[vertexIndex + 3] = new MyVertex {Position = new float3(1f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};

                outputTris[triangleIndex + 0] = (ushort) (vertexIndex + 0);
                outputTris[triangleIndex + 1] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 2] = (ushort) (vertexIndex + 2);
                outputTris[triangleIndex + 3] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 4] = (ushort) (vertexIndex + 3);
                outputTris[triangleIndex + 5] = (ushort) (vertexIndex + 2);
            }
            else if (side == Side.West)
            {
                outputVerts[vertexIndex + 0] = new MyVertex {Position = new float3(0f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                outputVerts[vertexIndex + 1] = new MyVertex {Position = new float3(0f, 1f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                outputVerts[vertexIndex + 2] = new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};
                outputVerts[vertexIndex + 3] = new MyVertex {Position = new float3(0f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                outputTris[triangleIndex + 0] = (ushort) (vertexIndex + 0);
                outputTris[triangleIndex + 1] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 2] = (ushort) (vertexIndex + 2);
                outputTris[triangleIndex + 3] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 4] = (ushort) (vertexIndex + 3);
                outputTris[triangleIndex + 5] = (ushort) (vertexIndex + 2);
            }
            else if (side == Side.Down)
            {
                outputVerts[vertexIndex + 0] = new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                outputVerts[vertexIndex + 1] = new MyVertex {Position = new float3(1f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};
                outputVerts[vertexIndex + 2] = new MyVertex {Position = new float3(0f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                outputVerts[vertexIndex + 3] = new MyVertex {Position = new float3(1f, 0f, 1f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};

                outputTris[triangleIndex + 0] = (ushort) (vertexIndex + 0);
                outputTris[triangleIndex + 1] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 2] = (ushort) (vertexIndex + 2);
                outputTris[triangleIndex + 3] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 4] = (ushort) (vertexIndex + 3);
                outputTris[triangleIndex + 5] = (ushort) (vertexIndex + 2);
            }
            else if (side == Side.South)
            {
                outputVerts[vertexIndex + 0] = new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                outputVerts[vertexIndex + 1] = new MyVertex {Position = new float3(0f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                outputVerts[vertexIndex + 2] = new MyVertex {Position = new float3(1f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};
                outputVerts[vertexIndex + 3] = new MyVertex {Position = new float3(1f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                outputTris[triangleIndex + 0] = (ushort) (vertexIndex + 0);
                outputTris[triangleIndex + 1] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 2] = (ushort) (vertexIndex + 2);
                outputTris[triangleIndex + 3] = (ushort) (vertexIndex + 1);
                outputTris[triangleIndex + 4] = (ushort) (vertexIndex + 3);
                outputTris[triangleIndex + 5] = (ushort) (vertexIndex + 2);

                // outputVerts[vertexIndex + 0] = new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h0), Color = color};
                // outputVerts[vertexIndex + 1] = new MyVertex {Position = new float3(1f, 0f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h0), Color = color};
                // outputVerts[vertexIndex + 2] = new MyVertex {Position = new float3(0f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h0, h1), Color = color};
                // outputVerts[vertexIndex + 3] = new MyVertex {Position = new float3(1f, 1f, 0f) + voxelXyz, TexCoord0 = new half2(h1, h1), Color = color};
                // outputTris[triangleIndex + 0] = (ushort) (vertexIndex + 0);
                // outputTris[triangleIndex + 1] = (ushort) (vertexIndex + 2);
                // outputTris[triangleIndex + 2] = (ushort) (vertexIndex + 1);
                // outputTris[triangleIndex + 3] = (ushort) (vertexIndex + 1);
                // outputTris[triangleIndex + 4] = (ushort) (vertexIndex + 2);
                // outputTris[triangleIndex + 5] = (ushort) (vertexIndex + 3);
            }
        }
    }

    public void ComputeVisibleFaces(List<Face> faces)
    {
        for (ushort i = 0; i < faces.Count; i++)
            if (!faces[i].Equals(default(Face)))
                visibleFacesIndexes.Add(i);
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
            // BlockType.Cutout => otherBlockType != BlockType.Opaque,
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


    // Create mesh from meshData
    private Mesh CreateMesh(Mesh.MeshData meshData)
    {
        var mesh = new Mesh();
        mesh.name = "ChunkMesh";

        // Set Vertex Buffer
        var vertexBuffer = meshData.GetVertexData<MyVertex>();
        mesh.SetVertexBufferParams(vertexBuffer.Length, new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2));
        mesh.SetVertexBufferData(vertexBuffer, 0, 0, vertexBuffer.Length);

        // Set Index Buffer
        var indexBuffer = meshData.GetIndexData<ushort>();
        mesh.SetIndexBufferParams(indexBuffer.Length, IndexFormat.UInt16);
        mesh.SetIndexBufferData(indexBuffer, 0, 0, indexBuffer.Length);

        // Set Submesh
        mesh.subMeshCount = 1;
        var submesh = new SubMeshDescriptor(0, indexBuffer.Length);
        mesh.SetSubMesh(0, submesh);

        // Recalculate Bounds and Normals
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }


}