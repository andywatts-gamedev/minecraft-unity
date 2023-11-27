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

public class BuildingSites : MonoBehaviour
{
    private static BuildingSites _instance;
    public static BuildingSites Instance => _instance;
    
    [Header("Building Sites")]
    public List<Vector3> buildingSites;
    public GameObject house4x3x4;
    public bool buildingSitesDebug;
    public List<GameObject> buildingSiteDebugGOs;
    
    private void Awake()
    {
        _instance = this;
        buildingSiteDebugGOs = new List<GameObject>();
    }
    
    private void Start()
    {
        ComputeBuildingSites();
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
                buildingSiteDebugGOs.Add( Instantiate(house4x3x4, buildingSite, Quaternion.identity, transform) );
        
        // else destroy buildingSiteDebugGOs
        else if (!buildingSitesDebug && buildingSiteDebugGOs.Count > 0)
        {
            foreach (var buildingSiteDebugGO in buildingSiteDebugGOs)
                GameObject.Destroy(buildingSiteDebugGO);
            buildingSiteDebugGOs.Clear();
        }
    }
    
    void ComputeBuildingSites()
    {
        World.Instance.ComputeHeightMap();
        buildingSites = new List<Vector3>();
        var buildingDims = new int3(5, 3, 5); // Assuming no overhangs
        // loop over x and z in heightmap and look for flat areas of 4x4
        for (var x = 0; x < World.Instance.dims.x; x++)
        for (var z = 0; z < World.Instance.dims.z; z++)
        {
            var height = World.Instance.heightMap[x, z];
            if (height == 0)
                continue;
            
            var isFlat = true;
            for (var dx = 0; dx < buildingDims.x; dx++)
            for (var dz = 0; dz < buildingDims.z; dz++)
            {
                if (x + dx >= World.Instance.dims.x || z + dz >= World.Instance.dims.z || World.Instance.heightMap[x + dx, z + dz] != height)
                {
                    isFlat = false;
                    break;
                }
            }

            if (isFlat)
            {
                var offset = new Vector3(2f, 0f, 2f); // Building is 4x3x4
                buildingSites.Add(new Vector3(x, height+1f+(buildingDims.y/2f), z) + offset);
            }
        }
    }

}