// using System.Collections.Generic;
// using System.Linq;
// using Unity.Collections;
// using Unity.Mathematics;
// using UnityEngine;
// using UnityEngine.Rendering;
// using static Unity.Mathematics.math;
//
// public class Chunk : MonoBehaviour
// {
//     public int3 dims;
//     // public List<int> voxels;
//     public NativeArray<ushort> voxels;
//     public NativeArray<Face> faces;
//     public ChunkType chunkType;
//             
//     private void Start()
//     {
//         ComputeVoxels();
//         ComputeFaces();
//         ComputeMesh();
//         GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Blocks.Instance.opaqueTexture2DArray);
//         GetComponent<MeshRenderer>().materials[1].SetTexture("_TextureArray", Blocks.Instance.alphaClipTexture2DArray);
//         GetComponent<MeshRenderer>().materials[2].SetTexture("_TextureArray", Blocks.Instance.transTexture2DArray);
//     }
//
//     private void ComputeVoxels()
//     {
//         voxels = new NativeArray<ushort>(dims.x * dims.y * dims.z, Allocator.Temp);
//         for (var i = 0; i < voxels.Length; i++)
//         {
//             var xyz = IndexToXYZ(i, dims);
//             if (chunkType == ChunkType.Flat)
//                 voxels[i] = (ushort)(xyz.y < 1 ? 1 : 0);  // Assume 1 is stone and 0 is air
//             else
//                 voxels[i] = 1;
//         }
//     }
//
//     private void ComputeFaces()
//     {
//         var numFaces = dims.x * dims.y * dims.z * 6;
//         faces = new NativeArray<Face>(numFaces, Allocator.Temp);
//         for (var i = 0; i < numFaces; i++)
//         {
//             var side = i % 6;
//             var voxelIndex = i / 6; // Calculate the voxel index from the job index
//             var voxelXyz = IndexToXYZ(voxelIndex, dims);
//             var voxel = voxels[voxelIndex];
//             // Debug.Log($"voxel {voxelXyz}");
//
//             var adjacentVoxelXyz = voxelXyz + Offsets[side];
//             var adjacentVoxelIndex = XYZToIndex(adjacentVoxelXyz, dims);
//             var adjacentIsWithinBounds = IsWithinBounds(adjacentVoxelXyz, dims);
//             var adjacentVoxel = (ushort)(adjacentIsWithinBounds ? voxels[adjacentVoxelIndex] : 0);
//
//             if (IsFaceVisible(Blocks.Instance.blocks[voxel].type, Blocks.Instance.blocks[adjacentVoxel].type))
//             {
//                 // Debug.Log($"Got face {side} for voxel {voxel} at {voxelXyz}");
//                 faces[i] = new Face {TextureIndex = Blocks.Instance.blocks[voxel].textureArrayIndex, BlockType = Blocks.Instance.blocks[voxel].type};
//             }
//         }
//         Debug.Log($"Got {faces.Count(f => !f.Equals(default(Face)))} faces");
//     }
//
//     private void ComputeMesh()
//     {
//         var vertices = new NativeList<MyVertex>(Allocator.Temp);
//         var opaqueTriangles = new NativeList<int>(Allocator.Temp);
//         var transparentTriangles = new NativeList<int>(Allocator.Temp);
//         var alphaClipTriangles = new NativeList<int>(Allocator.Temp);
//
//         
//         var faceCount = 0;
//         for (var i = 0; i < faces.Length; i++)
//         {
//             if (faces[i].Equals(default(Face)))
//                 continue;
//
//             var voxelIndex = i / 6;
//             var side = (Side) (i % 6);
//             var voxelXyz = IndexToXYZ(voxelIndex, dims);
//             var vertIndex = faceCount * 4;
//             var face = faces[i];
//             faceCount++;
//
//             var triangles = face.BlockType switch
//             {
//                 BlockType.Opaque => opaqueTriangles,
//                 BlockType.Transparent => transparentTriangles,
//                 BlockType.AlphaClip => alphaClipTriangles,
//                 _ => opaqueTriangles
//             };
//
//             if (side == Side.East)
//             {
//                 vertices.Add(new MyVertex {Position = new float3(1f, 0f, 0f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(1f, 1f, 0f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(1f, 1f, 1f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(1f, 0f, 1f) + voxelXyz});
//                 triangles.Add(vertIndex + 0);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 3);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 2);
//                 triangles.Add(vertIndex + 3);
//             }
//             else if (side == Side.Up)
//             {
//                 vertices.Add(new MyVertex {Position = new float3(0f, 1f, 0f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(0f, 1f, 1f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(1f, 1f, 1f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(1f, 1f, 1f) + voxelXyz});
//                 triangles.Add(vertIndex);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 3);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 2);
//                 triangles.Add(vertIndex + 3);
//             }
//             else if (side == Side.North)
//             {
//                 vertices.Add(new MyVertex {Position = new float3(0f, 0f, 1f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(1f, 0f, 1f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(0f, 1f, 1f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(1f, 1f, 1f) + voxelXyz});
//                 triangles.Add(vertIndex);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 2);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 3);
//                 triangles.Add(vertIndex + 2);
//             }
//             else if (side == Side.West)
//             {
//                 vertices.Add(new MyVertex {Position = new float3(0f, 0f, 1f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(0f, 0f, 1f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(0f, 0f, 1f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(0f, 0f, 1f) + voxelXyz});
//                 triangles.Add(vertIndex);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 2);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 3);
//                 triangles.Add(vertIndex + 2);
//             }
//             else if (side == Side.Down)
//             {
//                 vertices.Add(new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(1f, 0f, 0f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(1f, 0f, 0f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(1f, 0f, 0f) + voxelXyz});
//                 triangles.Add(vertIndex);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 2);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 3);
//                 triangles.Add(vertIndex + 2);
//             }
//             else if (side == Side.South)
//             {
//                 vertices.Add(new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz});
//                 vertices.Add(new MyVertex {Position = new float3(0f, 0f, 0f) + voxelXyz});
//                 triangles.Add(vertIndex);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 2);
//                 triangles.Add(vertIndex + 1);
//                 triangles.Add(vertIndex + 3);
//                 triangles.Add(vertIndex + 2);
//             }
//
//         }
//
//         var meshDataArray = Mesh.AllocateWritableMeshData(1);
//         var meshData = meshDataArray[0];
//         var layout = new[]
//         {
//             new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
//             // new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 2),
//             // new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.UNorm8, 4),
//             // new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
//         };
//         meshData.SetVertexBufferParams(vertices.Length, layout);
//
//         // meshData.SetIndexBufferParams(opaqueTriangles.Length + alphaClipTriangles.Length + transparentTriangles.Length, IndexFormat.UInt16);
//         meshData.SetIndexBufferParams(opaqueTriangles.Length, IndexFormat.UInt16);
//         var indexBuffer = meshData.GetIndexData<ushort>();
//         for (var i = 0; i < opaqueTriangles.Length; i++)
//             indexBuffer[i] = (ushort) opaqueTriangles[i];
//         // for (var i = 0; i < alphaClipTriangles.Length; i++)
//             // indexBuffer[opaqueTriangles.Length +  i] = (ushort) alphaClipTriangles[i];
//         // for (var i = 0; i < transparentTriangles.Length; i++)
//             // indexBuffer[opaqueTriangles.Length + alphaClipTriangles.Length + i] = (ushort) transparentTriangles[i];
//         
//         // Set subMeshes
//         meshData.subMeshCount = 1;
//         meshData.SetSubMesh(0, new SubMeshDescriptor(0, opaqueTriangles.Length));
//         // meshData.SetSubMesh(1, new SubMeshDescriptor(opaqueTriangles.Length, alphaClipTriangles.Length));
//         // meshData.SetSubMesh(2, new SubMeshDescriptor(opaqueTriangles.Length + alphaClipTriangles.Length, transparentTriangles.Length));
//         
//         
//         
//         var mesh = new Mesh();
//         mesh.name = "ChunkMesh";
//         Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
//         // mesh.RecalculateNormals();
//         // mesh.RecalculateBounds();
//         GetComponent<MeshFilter>().mesh = mesh;
//     }
//
//
//     private bool IsWithinBounds(int3 voxel, int3 dimensions)
//     {
//         return voxel.x >= 0 && voxel.x < dimensions.x &&
//                voxel.y >= 0 && voxel.y < dimensions.y &&
//                voxel.z >= 0 && voxel.z < dimensions.z;
//     }
//
//     private bool IsFaceVisible(BlockType blockType, BlockType otherBlockType)
//     {
//         return blockType switch
//         {
//             BlockType.Opaque => otherBlockType != BlockType.Opaque,
//             BlockType.AlphaClip => otherBlockType != BlockType.Opaque,
//             BlockType.Transparent => otherBlockType != BlockType.Opaque && otherBlockType != BlockType.Transparent,
//             _ => false
//         };
//     }
//
//     public struct MyVertex
//     {
//         public float3 Position;
//         // public ushort normalX, normalY;
//         // public Color32 tangent;
//         // public Color32 Color;
//         // public half2 TexCoord0;
//     }
//
//     public struct Face
//     {
//         public ushort TextureIndex;
//         public BlockType BlockType;
//         public byte SkyLight;
//         public byte BlockLight;
//     }
//     
//     public enum Side : byte
//     {
//         East,
//         Up,
//         North,
//         West,
//         Down,
//         South
//     } // %3 => xyz;  // Voxels have sides
//
//     private static readonly int3[] Offsets =
//     {
//         new(1, 0, 0), // east
//         new(0, 1, 0), // up
//         new(0, 0, 1), // north
//         new(-1, 0, 0), // west
//         new(0, -1, 0), // down
//         new(0, 0, -1) // south
//     };
//     
//     int XYZToIndex(int3 xyz, int3 dims)
//     {
//         return xyz.x + xyz.y * dims.x + xyz.z * dims.x * dims.y;
//     }
//
//     int3 IndexToXYZ(int voxelIndex, int3 dims)
//     {
//         var z = voxelIndex / (dims.x * dims.y);
//         var y = (voxelIndex - z * dims.x * dims.y) / dims.x;
//         var x = voxelIndex - z * dims.x * dims.y - y * dims.x;
//         return new int3(x, y, z);
//     }
//
// }