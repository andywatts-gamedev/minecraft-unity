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
    
    // Action for broadcasting voxel changes
    public Action<int, ushort> OnVoxelChanged;  // index, blockIndex
    
    public int3 dims = new int3(64, 64, 64);
    public NativeArray<ushort> voxels;
    public NativeArray<byte> blockLights;
    public NativeArray<byte> skyLights;
    public NativeArray<Face> faces;
    public ChunkType chunkType;
    public NavMeshSurface navMeshSurface;
    private Mesh mesh;
    private MeshCollider meshCollider;

    [Header("Chunks")]
    public Transform chunksParent;
    public GameObject chunkPrefab;
    public int3 chunkDims;
    public Dictionary<int3, Chunk> chunks = new Dictionary<int3, Chunk>();
    
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

    [Header("BlockStates")]
    public bool debug;
    public List<BlockState> BlockStates;

    private ushort AirIndex;
    
    private void Awake()
    {
        _instance = this;
        buildingSiteDebugGOs = new List<GameObject>();
        meshCollider = GetComponent<MeshCollider>();
    }
    
    private void Start()
    {
        AirIndex = (ushort)BlockStates.FindIndex(b => b.Block.Type == BlockType.Air);
        ComputeVoxels();
        
        for(var x=0; x<dims.x/chunkDims.x; x++)
        for (var z=0; z < dims.z / chunkDims.z; z++)
        {
            var chunkGO = GameObject.Instantiate(chunkPrefab, chunksParent);
            chunkGO.transform.position = new Vector3(x*chunkDims.x, 0f, z*chunkDims.z);
            var chunk = chunkGO.GetComponent<Chunk>();
            chunk.dims = chunkDims;
            chunk.xyz = new int3(x, 0, z);
            chunks[chunk.xyz] = chunk;
            chunkGO.name = chunk.xyz.ToString();
        }
    }

    private void ComputeVoxels()
    {
        Debug.Log("Compute Voxels");
        voxels = new NativeArray<ushort>(dims.x * dims.y * dims.z, Allocator.Persistent);
        for (var i = 0; i < voxels.Length; i++)
        {
            var xyz = i.ToInt3(dims);
            if (chunkType == ChunkType.Flat)
                voxels[i] = (ushort)(xyz.y < 1 ? (ushort)BlockStates.FindIndex(b => b.Block.name == "Grass Block") : 0); 
            
            else if (chunkType == ChunkType.Terrain)
            {
                var scale = 0.1f;
                var amplitude = 5.0f;
                var xCoord = (float)xyz.x * scale;
                var zCoord = (float)xyz.z * scale;
                var height = Mathf.PerlinNoise(xCoord, zCoord) * amplitude;
                
                if (xyz.y < height-1f)
                    voxels[i] = (ushort)BlockStates.FindIndex(b => b.Block.name == "Dirt");
                else if (xyz.y < height)
                    voxels[i] = (ushort) BlockStates.FindIndex(b => b.Block.name == "Grass Block");
                else
                    voxels[i] = AirIndex;
            }
            else
                voxels[i] = 1;
            // Debug.Log(xyz + " = " + voxels[i]);
        }
    }

    public void ComputeHeightMap()
    {
        heightMap = new int[dims.x, dims.z];
        for (var x = 0; x < dims.x; x++)
        for (var z = 0; z < dims.z; z++)
        for (var y = dims.y-1; y >= 0; y--)
        {
            var voxelIndex = new int3(x, y, z).ToIndex(dims);
            if (voxels[voxelIndex] != 0)
            {
                heightMap[x, z] = y;
                break;
            }
        }
    }

    private void OnDestroy()
    {
        voxels.Dispose();
    }

    public void Place(int voxelIndex, ushort blockIndex)
    {
        voxels[voxelIndex] = blockIndex;
        UpdateChunkMeshes(voxelIndex);
        OnVoxelChanged?.Invoke(voxelIndex, blockIndex);
    }

    public void Remove(int voxelIndex)
    {
        voxels[voxelIndex] = AirIndex;
        UpdateChunkMeshes(voxelIndex);
        OnVoxelChanged?.Invoke(voxelIndex, AirIndex);
    }
    
    private void UpdateChunkMeshes(int voxelIndex)
    {
        var voxelXyz = voxelIndex.ToInt3(dims);
        var chunkXyz = voxelXyz / chunkDims;
        chunks[chunkXyz].UpdateChunk();
        
        if (voxelXyz.x % chunkDims.x == 0 && chunks.ContainsKey(chunkXyz - new int3(1, 0, 0)))
            chunks[chunkXyz - new int3(1, 0, 0)].UpdateChunk();
        else if (voxelXyz.x % chunkDims.x == chunkDims.x - 1 && chunks.ContainsKey(chunkXyz + new int3(1, 0, 0)))
            chunks[chunkXyz + new int3(1, 0, 0)].UpdateChunk();
        else if (voxelXyz.y % chunkDims.y == 0 && chunks.ContainsKey(chunkXyz - new int3(0, 1, 0)))
            chunks[chunkXyz - new int3(0, 1, 0)].UpdateChunk();
        else if (voxelXyz.y % chunkDims.y == chunkDims.y - 1 && chunks.ContainsKey(chunkXyz + new int3(0, 1, 0)))
            chunks[chunkXyz + new int3(0, 1, 0)].UpdateChunk();
        else if (voxelXyz.z % chunkDims.z == 0 && chunks.ContainsKey(chunkXyz - new int3(0, 0, 1)))
            chunks[chunkXyz - new int3(0, 0, 1)].UpdateChunk();
        else if (voxelXyz.z % chunkDims.z == chunkDims.z - 1 && chunks.ContainsKey(chunkXyz + new int3(0, 0, 1)))
            chunks[chunkXyz + new int3(0, 0, 1)].UpdateChunk();
    }
    
}