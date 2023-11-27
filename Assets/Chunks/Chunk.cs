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
    
    private void Start()
    {
        UpdateMesh();
        GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Textures.Instance.opaqueTexture2DArray);
        GetComponent<MeshRenderer>().materials[1].SetTexture("_TextureArray", Textures.Instance.alphaClipTexture2DArray);
        GetComponent<MeshRenderer>().materials[2].SetTexture("_TextureArray", Textures.Instance.transTexture2DArray);
    }

    public void UpdateMesh()
    {
        var mesh = WorldMesher.Compute(dims, xyz*dims, World.Instance.dims, World.Instance.voxels);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        
        // TODO Resolve these World methods...
        // GetComponent<NavMeshSurface>().BuildNavMesh();
        // ComputeHeightMap();    // TODO still used?
        // ComputeBuildingSites();  // TODO What to do about Bobs enroute?
    }
    
}