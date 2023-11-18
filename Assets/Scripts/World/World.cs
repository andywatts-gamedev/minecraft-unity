using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using static Unity.Mathematics.math;

public class World : MonoBehaviour
{
    private static World _instance;
    public static World Instance => _instance;
    
    public int3 dims;
    public NativeArray<ushort> voxels;
    public NativeArray<byte> blockLights;
    public NativeArray<byte> skyLights;
    public NativeArray<Face> faces;
    public ChunkType chunkType;
    public NavMeshSurface navMeshSurface;

    [Header("Blocks")]
    public Block dirt;
    public Block grassBlock;
    public Block air;
    
    [Header("WIP")]
    // public GameObject torchPrefab;
    public byte blockLightDefault;
    public byte skyLightDefault;
    
    [Header("Building Sites")]
    public List<Vector3> buildingSites;
    public int[,] heightMap;
    public GameObject house4x3x4;
    public Transform houses;
    public bool buildingSitesDebug;
    public List<GameObject> buildingSiteDebugGOs;
    
    [Header("Bobs")]
    public int numBobs;
    public Transform bobs;
    public GameObject bobPrefab;

    private void Awake()
    {
        _instance = this;
        buildingSiteDebugGOs = new List<GameObject>();
    }
    
    private void Start()
    {
        ComputeVoxels();
        ComputeLights();
        ComputeFaces();
        ComputeMesh();
        
        GetComponent<NavMeshSurface>().BuildNavMesh();
        
        ComputeHeightMap();
        ComputeBuildingSites();
        
        CreateBobs();
        
        GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Textures.Instance.opaqueTexture2DArray);
        // GetComponent<MeshRenderer>().materials[1].SetTexture("_TextureArray", Textures.Instance.alphaClipTexture2DArray);
        // GetComponent<MeshRenderer>().materials[2].SetTexture("_TextureArray", Textures.Instance.transTexture2DArray);
    }

    private void Update()
    {
        DebugBuildingSites();
    }

    private void DebugBuildingSites()
    {
        // If enabling building site debug...create buildingSiteDebugGOs
        if (buildingSitesDebug && buildingSiteDebugGOs.Count == 0)
            foreach (var buildingSite in buildingSites)
                buildingSiteDebugGOs.Add( Instantiate(house4x3x4, buildingSite, Quaternion.identity, houses) );
        
        // else destroy buildingSiteDebugGOs
        else if (!buildingSitesDebug && buildingSiteDebugGOs.Count > 0)
        {
            foreach (var buildingSiteDebugGO in buildingSiteDebugGOs)
                GameObject.Destroy(buildingSiteDebugGO);
            buildingSiteDebugGOs.Clear();
        }
    }

    private void CreateBobs()
    {
        for (var i = 0; i < numBobs; i++)
        {
            var x = UnityEngine.Random.Range(0, dims.x);
            var z = UnityEngine.Random.Range(0, dims.z);
            Instantiate(bobPrefab, new Vector3(x, 5f, z), Quaternion.identity, bobs);
        }
    }

    private void ComputeVoxels()
    {
        voxels = new NativeArray<ushort>(dims.x * dims.y * dims.z, Allocator.Temp);
        for (var i = 0; i < voxels.Length; i++)
        {
            var xyz = IndexToXYZ(i, dims);
            if (chunkType == ChunkType.Flat)
                voxels[i] = (ushort)(xyz.y < 1 ? (ushort)Blocks.Instance.blocks.FindIndex(b => b == grassBlock) : 0); 
            
            else if (chunkType == ChunkType.Terrain)
            {
                var scale = 0.1f;
                var amplitude = 5.0f;
                var xCoord = (float)xyz.x * scale;
                var zCoord = (float)xyz.z * scale;
                var height = Mathf.PerlinNoise(xCoord, zCoord) * amplitude;

                if (xyz.y < height-1f)
                    voxels[i] = (ushort)Blocks.Instance.blocks.FindIndex(b => b == dirt);
                else if (xyz.y < height)
                    voxels[i] = (ushort)Blocks.Instance.blocks.FindIndex(b => b == grassBlock);
                else
                    voxels[i] = (ushort)Blocks.Instance.blocks.FindIndex(b => b == air);
            }
            else
                voxels[i] = 1;
        }
    }

    private void ComputeLights()
    {
        blockLights = new NativeArray<byte>(dims.x * dims.y * dims.z, Allocator.Temp);
        skyLights = new NativeArray<byte>(dims.x * dims.y * dims.z, Allocator.Temp);
        var torchPosition = new float3(1f, 1f, 1f);
        
        if (chunkType == ChunkType.Flat)
        {
            var index = XYZToIndex((int3) torchPosition, dims);
            blockLights[index] = (byte) 14;
            Debug.Log(index);
            Debug.Log(IndexToXYZ(index, dims));
            
            // var torch = GameObject.Instantiate(torchPrefab);
            // torch.transform.position = torchPosition + new float3(0.5f, 0.5f, 0.5f);
        }
    }
    
    private void ComputeFaces()
    {
        var numFaces = dims.x * dims.y * dims.z * 6;
        faces = new NativeArray<Face>(numFaces, Allocator.Temp);
        for (var i = 0; i < numFaces; i++)
        {
            var side = i % 6;
            var voxelIndex = i / 6; // Calculate the voxel index from the job index
            var voxelXyz = IndexToXYZ(voxelIndex, dims);
            var voxel = voxels[voxelIndex];
            // Debug.Log($"voxel {voxelXyz}");

            var adjacentVoxelXyz = voxelXyz + Offsets[side];
            var adjacentVoxelIndex = XYZToIndex(adjacentVoxelXyz, dims);
            var adjacentIsWithinBounds = IsWithinBounds(adjacentVoxelXyz, dims);
            var adjacentVoxel = (ushort)(adjacentIsWithinBounds ? voxels[adjacentVoxelIndex] : 0);

            if (IsFaceVisible(Blocks.Instance.blocks[voxel].Type, Blocks.Instance.blocks[adjacentVoxel].Type))
            {
                // Debug.Log($"Got face {side} for voxel {voxel} at {voxelXyz}  {(byte)Blocks.Instance.blocks[voxel].SideTextures[side].TextureObject.TextureIndex}");
                var textureObject = Blocks.Instance.blocks[voxel].SideTextures[side].TextureObject;
                faces[i] = new Face
                {
                    TextureIndex = (ushort)textureObject.TextureIndex,
                    BlockType    = Blocks.Instance.blocks[voxel].Type,
                    BlockLight   = adjacentIsWithinBounds ? blockLights[adjacentVoxelIndex] : blockLightDefault,
                    SkyLight     = adjacentIsWithinBounds ? skyLights[adjacentVoxelIndex] : skyLightDefault,
                    Metallic     = adjacentIsWithinBounds ? textureObject.Metallic : (byte)0,
                    Smoothness   = adjacentIsWithinBounds ? textureObject.Smoothness : (byte)0,
                };
            }
        }
        Debug.Log($"Got {faces.Count(f => !f.Equals(default(Face)))} faces");
    }

    private void ComputeMesh()
    { 
        // Define MeshData
        var numFaces = faces.Count(f => !f.Equals(default(Face)));
        var meshDataArray = Mesh.AllocateWritableMeshData(1);
        var meshData = meshDataArray[0];
        meshData.SetVertexBufferParams(numFaces * 4, new[]
        {
            new VertexAttributeDescriptor( VertexAttribute.Position, dimension: 3, stream: 0),
            new VertexAttributeDescriptor( VertexAttribute.Normal, dimension: 3, stream: 0 ),
            new VertexAttributeDescriptor( VertexAttribute.Tangent, VertexAttributeFormat.Float16, dimension: 4, stream: 0 ),
            new VertexAttributeDescriptor( VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, dimension: 2, stream: 1 ),
            new VertexAttributeDescriptor( VertexAttribute.Color, VertexAttributeFormat.UNorm8, dimension: 4, stream: 2 ),
        });
        meshData.SetIndexBufferParams(numFaces * 6, IndexFormat.UInt16);
        
        
        // Populate MeshData
        var vertexStream0 = meshData.GetVertexData<VertexStream0>(0);
        var triangles = meshData.GetIndexData<ushort>();
        var texCoords = meshData.GetVertexData<half2>(1);
        var colors = meshData.GetVertexData<Color32>(2);


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
            if (faces[i].BlockType == BlockType.AlphaClip)
                tris = alphaClipTriangles;
            else if (faces[i].BlockType == BlockType.Transparent)
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
            var voxelXyz = IndexToXYZ(voxelIndex, dims);

            if (side == Side.East)
            {
                vertexStream0[faceCount*4 + 0] = new VertexStream0 {Position = new float3(1f, 0f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 1] = new VertexStream0 {Position = new float3(1f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount*4 + 3] = new VertexStream0 {Position = new float3(1f, 0f, 1f) + voxelXyz};
            }
            else if (side == Side.Up)
            {
                vertexStream0[faceCount*4 + 0] = new VertexStream0 {Position = new float3(0f, 1f, 0f) + voxelXyz};
                vertexStream0[faceCount*4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount*4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 1f) + voxelXyz};
                vertexStream0[faceCount*4 + 3] = new VertexStream0 {Position = new float3(1f, 1f, 0f) + voxelXyz};
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
            
            
            // Texcoords
            texCoords[faceCount*4 + 0] = (half)0;
            texCoords[faceCount*4 + 1] = half2((half)0, (half)1);
            texCoords[faceCount*4 + 2] = (half)1;
            texCoords[faceCount*4 + 3] = half2((half)1, (half)0);
            
            // Colors
            // TODO consider side and blockType to get textureIndex
            var textureIndex = (byte)faces[i].TextureIndex;   // Must * 256 in SG to get index
            var blockLight = faces[i].BlockLight;
            var skyLight = faces[i].SkyLight;
            var metallicSmoothness = PackValues(faces[i].Metallic, faces[i].Smoothness);
            colors[faceCount * 4 + 0] = new Color32(blockLight, skyLight, metallicSmoothness,textureIndex);
            colors[faceCount * 4 + 1] = new Color32(blockLight, skyLight, metallicSmoothness,textureIndex);
            colors[faceCount * 4 + 2] = new Color32(blockLight, skyLight, metallicSmoothness,textureIndex);
            colors[faceCount * 4 + 3] = new Color32(blockLight, skyLight, metallicSmoothness,textureIndex); 
            
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
        GetComponent<MeshFilter>().mesh = mesh;
        
        var collider = gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
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

    void ComputeBuildingSites()
    {
        buildingSites = new List<Vector3>();
        var buildingDims = new int3(4, 3, 4); // Assuming no overhangs
        // loop over x and z in heightmap and look for flat areas of 4x4
        for (var x = 0; x < dims.x; x++)
        for (var z = 0; z < dims.z; z++)
        {
            var height = heightMap[x, z];
            if (height == 0)
                continue;
            
            var isFlat = true;
            for (var dx = 0; dx < buildingDims.x; dx++)
            for (var dz = 0; dz < buildingDims.z; dz++)
            {
                if (x + dx >= dims.x || z + dz >= dims.z || heightMap[x + dx, z + dz] != height)
                {
                    isFlat = false;
                    break;
                }
            }

            if (isFlat)
            {
                // Debug.Log($"Found flat area at {x}, {z} with height {height}");
                var offset = new Vector3(2f, 0f, 2f); // Building is 4x3x4
                buildingSites.Add(new Vector3(x, height, z) + offset);
            }
        }
    }

    private void ComputeHeightMap()
    {
        heightMap = new int[dims.x, dims.z];
        for (var x = 0; x < dims.x; x++)
        for (var z = 0; z < dims.z; z++)
        for (var y = dims.y-1; y >= 0; y--)
        {
            var voxelIndex = XYZToIndex(new int3(x, y, z), dims);
            if (voxels[voxelIndex] != 0)
            {
                heightMap[x, z] = y;
                break;
            }
        }
    }

    // --- Static Helpers ---
    private static readonly int3[] Offsets =
    {
        new(1, 0, 0), // east
        new(0, 1, 0), // up
        new(0, 0, 1), // north
        new(-1, 0, 0), // west
        new(0, -1, 0), // down
        new(0, 0, -1) // south
    };

    static int XYZToIndex(int3 xyz, int3 dims)
    {
        return xyz.x + xyz.y * dims.x + xyz.z * dims.x * dims.y;
    }

    static int3 IndexToXYZ(int voxelIndex, int3 dims)
    {
        var z = voxelIndex / (dims.x * dims.y);
        var y = (voxelIndex - z * dims.x * dims.y) / dims.x;
        var x = voxelIndex - z * dims.x * dims.y - y * dims.x;
        return new int3(x, y, z);
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

}