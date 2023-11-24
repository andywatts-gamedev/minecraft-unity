using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class BlueprintMono : MonoBehaviour
{
    public Blueprint blueprint;

    private void Start()
    {
        // GetComponent<MeshFilter>().mesh = blueprint.GenerateMesh();
        GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Textures.Instance.opaqueTexture2DArray);
        gameObject.name = blueprint.name;
        
        // Compute mesh for blueprint voxels
        
        
        
        
    }
}