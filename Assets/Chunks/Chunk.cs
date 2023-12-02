using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public int3 xyz;
    public int3 dims;
    
    [Header("Building Sites")]
    public List<Vector3> buildingSites;
    public int[,] heightMap;
    public List<GameObject> models;
    public GameObject modelPrefab;
    
    private void Start()
    {
        models = new List<GameObject>();
        UpdateChunk();
        GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Textures.Instance.opaqueTexture2DArray);
        GetComponent<MeshRenderer>().materials[1].SetTexture("_TextureArray", Textures.Instance.alphaClipTexture2DArray);
        GetComponent<MeshRenderer>().materials[2].SetTexture("_TextureArray", Textures.Instance.transTexture2DArray);
    }

    public void UpdateChunk()
    {
        UpdateMesh();
        UpdateModels();
        
        // TODO Resolve these World methods...
        // GetComponent<NavMeshSurface>().BuildNavMesh();
        // ComputeHeightMap();    // TODO still used?
        // ComputeBuildingSites();  // TODO What to do about Bobs enroute?
    }

    public void UpdateMesh()
    {
        var mesh = WorldMesher.Compute(dims, xyz*dims, World.Instance.dims, World.Instance.voxels);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void UpdateModels()
    {
        // Update models
        foreach (var model in models)
            GameObject.Destroy(model);
        models.Clear();

        for (var x = 0; x < dims.x; x++)
        for (var y = 0; y < dims.y; y++)
        for (var z = 0; z < dims.z; z++)
        {
            var voxelXyz = new int3(x, y, z) + xyz * dims;
            var voxelIndex = voxelXyz.ToIndex(World.Instance.dims);
            var voxel = World.Instance.voxels[voxelIndex];

            var blockState = Blocks.Instance.BlockStates[voxel];
            if (blockState.Block.Type == BlockType.Model)
            {
                var model = GameObject.Instantiate(modelPrefab);
                model.transform.parent = transform;
                model.transform.localPosition = new Vector3(x, y, z);
                models.Add(model);
            }
        }
    }
}