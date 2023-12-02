using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

/*
SHADER CHANNELS
Color
    TextureIndex: float;  TODO 12+ bits
    Metallic: float
    Smoothness: float

UV1
    ModelStartX: float;  TODO half
    ModelStartY: float;  TODO half
    ModelSizeX: float;  TODO half
    ModelSizeY: float;  TODO half

UV2
    BlockLight: float
    SkyLight: float
*/

public class Mesher
{

    public static Mesh Compute(int3 dims, NativeArray<ushort> voxels)
    {
        var faces = ComputeFaces(dims, voxels);
        return ComputeMesh(dims, faces);
    }

    private static NativeArray<Face> ComputeFaces(int3 dims, NativeArray<ushort> voxels)
    {
        var faces = new NativeArray<Face>(dims.Magnitude() * 6, Allocator.Temp);
        var numFaces = dims.Magnitude() * 6;
        for (var i = 0; i < numFaces; i++)
        {
            var side = i % 6;
            var voxelIndex = i / 6; // Calculate the voxel index from the job index
            var voxelXyz = voxelIndex.ToInt3(dims);
            var voxel = voxels[voxelIndex];
            // Debug.Log($"voxel {voxelXyz} = {voxels[voxelIndex]}");

            var adjacentVoxelXyz = voxelXyz + Offsets[side];
            var adjacentVoxelIndex = adjacentVoxelXyz.ToIndex(dims);
            var adjacentIsWithinBounds = IsWithinBounds(adjacentVoxelXyz, dims);
            var adjacentVoxel = (ushort)(adjacentIsWithinBounds ? voxels[adjacentVoxelIndex] : 0);
            // Debug.Log($"adjacentVoxelXyz {adjacentVoxelXyz} = {voxels[voxelIndex]}");

            var block = Blocks.Instance.blocks[voxel];
            var otherBlock = Blocks.Instance.blocks[adjacentVoxel];
            if (IsFaceVisible(block, otherBlock))
            {
                // Debug.Log($"Got face {side} for voxel {voxel} at {voxelXyz}  {(byte)Blocks.Instance.blocks[voxel].SideTextures[side].TextureObject.TextureIndex}");
                var textureObject = Blocks.Instance.blocks[voxel].SideTextures[side].TextureObject;
                faces[i] = new Face
                {
                    TextureIndex = (ushort)textureObject.TextureIndex,
                    TextureType    = Blocks.Instance.blocks[voxel].TextureType,
                    // BlockLight   = adjacentIsWithinBounds ? blockLights[adjacentVoxelIndex] : blockLightDefault,
                    // SkyLight     = adjacentIsWithinBounds ? skyLights[adjacentVoxelIndex] : skyLightDefault,
                    BlockLight   = 0,
                    SkyLight     = 0,
                    Metallic     = adjacentIsWithinBounds ? textureObject.Metallic : (byte)0,
                    Smoothness   = adjacentIsWithinBounds ? textureObject.Smoothness : (byte)0,
                };
            }
        }
        // Debug.Log($"Got {faces.Count(f => !f.Equals(default(Face)))} faces");
        return faces;
    }

    private static Mesh ComputeMesh(int3 dims, NativeArray<Face> faces)
    { 
        // Define MeshData
        var numFaces = faces.Count(f => !f.Equals(default(Face))); // Linq
        var meshDataArray = Mesh.AllocateWritableMeshData(1);
        var meshData = meshDataArray[0];
        meshData.SetVertexBufferParams(numFaces * 4, new[]
        {
            new VertexAttributeDescriptor( VertexAttribute.Position, dimension: 3, stream: 0),
            new VertexAttributeDescriptor( VertexAttribute.Normal, dimension: 3, stream: 0 ),
            new VertexAttributeDescriptor( VertexAttribute.Tangent, VertexAttributeFormat.Float16, dimension: 4, stream: 0 ),
            new VertexAttributeDescriptor( VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, dimension: 2, stream: 1 ),
            new VertexAttributeDescriptor( VertexAttribute.TexCoord1, VertexAttributeFormat.Float16, dimension: 4, stream: 2 ),  // ModelSizeX, ModelSizeY, ModelStartX, ModelStartY
            new VertexAttributeDescriptor( VertexAttribute.Color, VertexAttributeFormat.UNorm8, dimension: 4, stream: 3 ), // TextureIndex, Metallic, Smoothness, 0
        });
        meshData.SetIndexBufferParams(numFaces * 6, IndexFormat.UInt16);
        
        
        // Populate MeshData
        var vertexStream0 = meshData.GetVertexData<VertexStream0>(0);
        var triangles = meshData.GetIndexData<ushort>();
        var texCoords0 = meshData.GetVertexData<half2>(1);
        var texCoords1 = meshData.GetVertexData<half2>(2); // ModelSizeX, ModelSizeY, ModelStartX, ModelStartY
        var colors = meshData.GetVertexData<Color32>(3); // TextureIndex, Metallic, Smoothness, 0

        var opaqueTriangles = new NativeList<ushort>(Allocator.Temp);
        var transparentTriangles = new NativeList<ushort>(Allocator.Temp);
        var alphaClipTriangles = new NativeList<ushort>(Allocator.Temp);
        NativeList<ushort> tris;
        
        var faceCount = 0;
        for (var i = 0; i < faces.Length; i++)
        {
            if (faces[i].Equals(default(Face)))
                continue;

            // Triangles
            if (faces[i].TextureType == TextureType.AlphaClip)
                tris = alphaClipTriangles;
            else if (faces[i].TextureType == TextureType.Transparent)
                tris = transparentTriangles;
            else 
                tris = opaqueTriangles;
            tris.Add((ushort)(faceCount*4 + 0));
            tris.Add((ushort)(faceCount*4 + 1));
            tris.Add((ushort)(faceCount*4 + 2));
            tris.Add((ushort)(faceCount*4 + 2));
            tris.Add((ushort)(faceCount*4 + 3));
            tris.Add((ushort)(faceCount*4 + 0));
                
            // Vertex
            var voxelIndex = i / 6;
            var side = (Side) (i % 6);
            var voxelXyz = voxelIndex.ToInt3(dims);

            if (side == Side.East)
            {
                vertexStream0[faceCount*4 + 0] = new VertexStream0 {Position = new float3(1f, 0f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 1] = new VertexStream0 {Position = new float3(1f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount*4 + 3] = new VertexStream0 {Position = new float3(1f, 0f, 1f) + voxelXyz};
            }
            else if (side == Side.Up)
            {
                vertexStream0[faceCount*4 + 0] = new VertexStream0 {Position = new float3(1f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount*4 + 1] = new VertexStream0 {Position = new float3(1f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 2] = new VertexStream0 {Position = new float3(0f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 3] = new VertexStream0 {Position = new float3(0f, 1f, 1f) + voxelXyz};
            }
            else if (side == Side.North)
            {
                vertexStream0[faceCount*4 + 0] = new VertexStream0 {Position = new float3(1f, 0f, 1f) + voxelXyz};
                vertexStream0[faceCount*4 + 1] = new VertexStream0 {Position = new float3(1f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount*4 + 2] = new VertexStream0 {Position = new float3(0f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount*4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 1f) + voxelXyz};
            }
            else if (side == Side.West)
            {
                vertexStream0[faceCount*4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 1f) + voxelXyz};
                vertexStream0[faceCount*4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount*4 + 2] = new VertexStream0 {Position = new float3(0f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 0f) + voxelXyz};
            }
            else if (side == Side.Down)
            {
                vertexStream0[faceCount*4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 1] = new VertexStream0 {Position = new float3(1f, 0f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 2] = new VertexStream0 {Position = new float3(1f, 0f, 1f) + voxelXyz};
                vertexStream0[faceCount*4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 1f) + voxelXyz};
            }
            else if (side == Side.South)
            {
                vertexStream0[faceCount*4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 3] = new VertexStream0 {Position = new float3(1f, 0f, 0f) + voxelXyz};
            }
            
            // TexCoord0
            texCoords0[faceCount*4 + 0] = (half)0;
            texCoords0[faceCount*4 + 1] = half2((half)0, (half)1);
            texCoords0[faceCount*4 + 2] = (half)1;
            texCoords0[faceCount*4 + 3] = half2((half)1, (half)0);
            
            // TexCoord1; ModelSizeX, ModelSizeY, ModelStartX, ModelStartY
            // texCoords1[faceCount*4 + 0] = 
            // texCoords1[faceCount*4 + 1] =
            // texCoords1[faceCount*4 + 2] = 
            // texCoords1[faceCount*4 + 3] =
            
            
            // Color; TextureIndex, Metallic, Smoothness, 0
            // TODO consider side and blockType to get textureIndex
            var textureIndex = (byte)faces[i].TextureIndex;   // Must *256 in SG to get index
            var metallic = faces[i].Metallic;
            var smoothness = faces[i].Smoothness;
            colors[faceCount * 4 + 0] = new Color32(0, smoothness, metallic, textureIndex);
            colors[faceCount * 4 + 1] = new Color32(0, smoothness, metallic, textureIndex);
            colors[faceCount * 4 + 2] = new Color32(0, smoothness, metallic, textureIndex);
            colors[faceCount * 4 + 3] = new Color32(0, smoothness, metallic, textureIndex); 
            
            faceCount++;
        }
        
        // Triangles
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
        mesh.name = "ChunkMesh";
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

     private static bool IsWithinBounds(int3 voxel, int3 dimensions)
     {
         return voxel.x >= 0 && voxel.x < dimensions.x &&
                voxel.y >= 0 && voxel.y < dimensions.y &&
                voxel.z >= 0 && voxel.z < dimensions.z;
     }
     
    private static bool IsFaceVisible(Block block, Block otherBlock)
    {
        // If cube..check adjacent cube texture type
        if (block.Type == BlockType.Cube && otherBlock.Type == BlockType.Cube)
            return block.TextureType switch
            {
                TextureType.Opaque => otherBlock.TextureType != TextureType.Opaque,
                TextureType.AlphaClip => otherBlock.TextureType != TextureType.Opaque,
                TextureType.Transparent => otherBlock.TextureType != TextureType.Opaque && otherBlock.TextureType != TextureType.Transparent,
                _ => false
            };
        
        return true;
    }
     
     static byte PackValues(byte value1, byte value2)
    {
        // Ensure that values are within the 4-bit range
        value1 = (byte)(value1 & 0x0F);
        value2 = (byte)(value2 & 0x0F);
        
        // Pack the values into a single byte
        byte packedValue = (byte)((value1 << 4) | value2);

        return packedValue;
    }
  
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