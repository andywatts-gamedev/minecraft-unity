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
    private Mesh mesh;
    private MeshCollider meshCollider;

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
        meshCollider = GetComponent<MeshCollider>();
    }
    
    private void Start()
    {
        ComputeVoxels();
        UpdateMesh();
        CreateBobs();
        GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Textures.Instance.opaqueTexture2DArray);
        // GetComponent<MeshRenderer>().materials[1].SetTexture("_TextureArray", Textures.Instance.alphaClipTexture2DArray);
        // GetComponent<MeshRenderer>().materials[2].SetTexture("_TextureArray", Textures.Instance.transTexture2DArray);
    }

    private void Update()
    {
        DebugBuildingSites();
    }

    public void UpdateMesh()
    {
        mesh = Mesher.Compute(dims, voxels);
        GetComponent<MeshFilter>().mesh = mesh;
        meshCollider.sharedMesh = mesh;
        ComputeHeightMap();    // TODO still used?
        GetComponent<NavMeshSurface>().BuildNavMesh();
        ComputeBuildingSites();  // TODO What to do about Bobs enroute?
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
        Debug.Log("Compute Voxels");
        voxels = new NativeArray<ushort>(dims.x * dims.y * dims.z, Allocator.Persistent);
        for (var i = 0; i < voxels.Length; i++)
        {
            var xyz = i.ToInt3(dims);
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
            // Debug.Log(xyz + " = " + voxels[i]);
        }
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
                Debug.Log($"Height: {height} \t Total: {height+(buildingDims.y/2f)}");
                buildingSites.Add(new Vector3(x, height+1f+(buildingDims.y/2f), z) + offset);
                return; // Temp do one building only
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

}