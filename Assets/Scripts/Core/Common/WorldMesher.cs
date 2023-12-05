using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

public class WorldMesher
{
    private static readonly int3[] Offsets =
    {
        new(1, 0, 0), // east
        new(0, 1, 0), // up
        new(0, 0, 1), // north
        new(-1, 0, 0), // west
        new(0, -1, 0), // down
        new(0, 0, -1) // south
    };

    public static Mesh Compute(int3 chunkDims, int3 chunkStart, int3 worldDims, NativeArray<ushort> voxels)
    {
        var faces = ComputeFaces(chunkDims, chunkStart, worldDims, voxels);
        Debug.Log("---");
        Debug.Log(faces.Length);
        return ComputeMesh(chunkDims, chunkStart, worldDims, faces);
    }

    private static NativeArray<Face> ComputeFaces(int3 chunkDims, int3 chunkStart, int3 worldDims, NativeArray<ushort> voxels)
    {
        var faces = new NativeArray<Face>(chunkDims.Magnitude() * 6, Allocator.Temp);
        var numFaces = chunkDims.Magnitude() * 6;

        // Loop over chunk in world
        for (var x = 0; x < chunkDims.x; x++)
        for (var y = 0; y < chunkDims.y; y++)
        for (var z = 0; z < chunkDims.z; z++)
        {
            var voxelXyz = new int3(x, y, z) + chunkStart;
            var voxelIndex = voxelXyz.ToIndex(worldDims);
            var voxel = voxels[voxelIndex];

            for (var side = 0; side < 6; side++)
            {
                var adjacentVoxelXyz = voxelXyz + Offsets[side];
                var adjacentVoxelIndex = adjacentVoxelXyz.ToIndex(worldDims);
                var adjacentIsWithinBounds = IsWithinBounds(adjacentVoxelXyz, worldDims);
                var adjacentVoxel = (ushort) (adjacentIsWithinBounds ? voxels[adjacentVoxelIndex] : 0);

                var block = World.Instance.BlockStates[voxel];
                var otherBlock = World.Instance.BlockStates[adjacentVoxel];
                if (IsFaceVisible(block, otherBlock))
                {
                    
                    var modelFaces = World.Instance.BlockStates[voxel].Model.ModelElements[0].ModelFaces;
                    var modelFace = modelFaces.FirstOrDefault(mf => mf.Side == (Side)side);

                    var i = new int3(x, y, z).ToIndex(chunkDims) * 6 + side;
                    faces[i] = new Face
                    {
                        TextureIndex = (ushort) modelFace.Texture.TextureIndex,
                        TextureType = World.Instance.BlockStates[voxel].Block.TextureType,
                        // BlockLight   = adjacentIsWithinBounds ? blockLights[adjacentVoxelIndex] : blockLightDefault,
                        // SkyLight     = adjacentIsWithinBounds ? skyLights[adjacentVoxelIndex] : skyLightDefault,
                        BlockLight = 0,
                        SkyLight = 0,
                        Metallic = adjacentIsWithinBounds ? modelFace.Texture.Metallic : (byte) 0,
                        Smoothness = adjacentIsWithinBounds ? modelFace.Texture.Smoothness : (byte) 0
                    };
                }
            }
        }

        return faces;
    }

    private static Mesh ComputeMesh(int3 chunkDims, int3 chunkStart, int3 worldDims, NativeArray<Face> faces)
    {
        // Define MeshData
        var numFaces = faces.Count(f => !f.Equals(default(Face))); // Linq
        var meshDataArray = Mesh.AllocateWritableMeshData(1);
        var meshData = meshDataArray[0];
        meshData.SetVertexBufferParams(numFaces * 4, new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3, stream: 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3, stream: 0), 
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4, 0),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2, 1), 
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4, 2)
        });
        meshData.SetIndexBufferParams(numFaces * 6, IndexFormat.UInt16);


        // Populate MeshData
        var vertexStream0 = meshData.GetVertexData<VertexStream0>();
        var triangles = meshData.GetIndexData<ushort>();
        var texCoords0 = meshData.GetVertexData<half2>(1);
        var colors = meshData.GetVertexData<Color32>(2);


        var opaqueTriangles = new NativeList<ushort>(Allocator.Temp);
        var alphaClipTriangles = new NativeList<ushort>(Allocator.Temp);
        var transparentTriangles = new NativeList<ushort>(Allocator.Temp);
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
            tris.Add((ushort) (faceCount * 4 + 0));
            tris.Add((ushort) (faceCount * 4 + 1));
            tris.Add((ushort) (faceCount * 4 + 2));
            tris.Add((ushort) (faceCount * 4 + 2));
            tris.Add((ushort) (faceCount * 4 + 3));
            tris.Add((ushort) (faceCount * 4 + 0));


            // Vertex
            var voxelIndex = i / 6;
            var side = (Side) (i % 6);
            var voxelXyz = voxelIndex.ToInt3(chunkDims);

            if (side == Side.East)
            {
                vertexStream0[faceCount * 4 + 0] = new VertexStream0 {Position = new float3(1f, 0f, 0f) + voxelXyz};
                vertexStream0[faceCount * 4 + 1] = new VertexStream0 {Position = new float3(1f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount * 4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount * 4 + 3] = new VertexStream0 {Position = new float3(1f, 0f, 1f) + voxelXyz};
            }
            else if (side == Side.Up)
            {
                vertexStream0[faceCount * 4 + 0] = new VertexStream0 {Position = new float3(0f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount * 4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount * 4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount * 4 + 3] = new VertexStream0 {Position = new float3(1f, 1f, 0f) + voxelXyz};
            }
            else if (side == Side.North)
            {
                vertexStream0[faceCount * 4 + 0] = new VertexStream0 {Position = new float3(1f, 0f, 1f) + voxelXyz};
                vertexStream0[faceCount * 4 + 1] = new VertexStream0 {Position = new float3(1f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount * 4 + 2] = new VertexStream0 {Position = new float3(0f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount * 4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 1f) + voxelXyz};
            }
            else if (side == Side.West)
            {
                vertexStream0[faceCount * 4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 1f) + voxelXyz};
                vertexStream0[faceCount * 4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount * 4 + 2] = new VertexStream0 {Position = new float3(0f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount * 4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 0f) + voxelXyz};
            }
            else if (side == Side.Down)
            {
                vertexStream0[faceCount * 4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 0f) + voxelXyz};
                vertexStream0[faceCount * 4 + 1] = new VertexStream0 {Position = new float3(1f, 0f, 0f) + voxelXyz};
                vertexStream0[faceCount * 4 + 2] = new VertexStream0 {Position = new float3(1f, 0f, 1f) + voxelXyz};
                vertexStream0[faceCount * 4 + 3] = new VertexStream0 {Position = new float3(0f, 0f, 1f) + voxelXyz};
            }
            else if (side == Side.South)
            {
                vertexStream0[faceCount * 4 + 0] = new VertexStream0 {Position = new float3(0f, 0f, 0f) + voxelXyz};
                vertexStream0[faceCount * 4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount * 4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount * 4 + 3] = new VertexStream0 {Position = new float3(1f, 0f, 0f) + voxelXyz};
            }
 
            // TexCoord0
            texCoords0[faceCount*4 + 0] = (half)0;
            texCoords0[faceCount*4 + 1] = half2((half)0, (half)1);
            texCoords0[faceCount*4 + 2] = (half)1;
            texCoords0[faceCount*4 + 3] = half2((half)1, (half)0);
            
            // Colors
            // TODO consider side and blockType to get textureIndex
            var textureIndex = (byte) faces[i].TextureIndex; // Must *256 in SG to get index
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
            triangles[opaqueTriangles.Length + i] = alphaClipTriangles[i];
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


    // ---- Statics

    private static bool IsWithinBounds(int3 voxel, int3 dimensions)
    {
        return voxel.x >= 0 && voxel.x < dimensions.x &&
               voxel.y >= 0 && voxel.y < dimensions.y &&
               voxel.z >= 0 && voxel.z < dimensions.z;
    }

    private static bool IsFaceVisible(BlockState blockState, BlockState otherBlockState)
    {
        // If cube..check adjacent cube texture type
        if (blockState.Block.Type == BlockType.Cube && otherBlockState.Block.Type == BlockType.Cube)
            return blockState.Block.TextureType switch
            {
                TextureType.Opaque => otherBlockState.Block.TextureType != TextureType.Opaque,
                TextureType.AlphaClip => otherBlockState.Block.TextureType != TextureType.Opaque,
                TextureType.Transparent => otherBlockState.Block.TextureType != TextureType.Opaque && otherBlockState.Block.TextureType != TextureType.Transparent,
                _ => false
            };
        
        if (blockState.Block.Type == BlockType.Air)
            return false;
        
        return true;
    }

    private static byte PackValues(byte value1, byte value2)
    {
        // Ensure that values are within the 4-bit range
        value1 = (byte) (value1 & 0x0F);
        value2 = (byte) (value2 & 0x0F);

        // Pack the values into a single byte
        var packedValue = (byte) ((value1 << 4) | value2);

        return packedValue;
    }
}