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
        // Copy blocks to voxels array
        var voxels = new NativeArray<ushort>(blueprint.blocks.Length, Allocator.Temp);
        for (int i = 0; i < blueprint.blocks.Length; i++)
            voxels[i] = (ushort) Blocks.Instance.blocks.FindIndex(b => b == blueprint.blocks[i]);
        
        var mesh = Mesher.Compute(blueprint.dims, voxels);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Textures.Instance.opaqueTexture2DArray);
        gameObject.name = ((Object) blueprint).name;
    }
}