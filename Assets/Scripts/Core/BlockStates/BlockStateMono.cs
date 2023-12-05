using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class BlockStateMono : MonoBehaviour
{
    public BlockState BlockState;

    private void Start()
    {
        GetComponent<MeshFilter>().mesh = BlockState.ComputeMesh();
        GetComponent<MeshRenderer>().materials = Textures.Instance.LitMaterials;
    
        // rotate if state contains "y" key
        if (BlockState.States != null)
            foreach (var state in BlockState.States)
                if (state.Key == "y")
                {
                    var angle = int.Parse(state.Value);
                    transform.rotation = Quaternion.Euler(0, angle, 0);
                }
    }
}