using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class BlockMono : MonoBehaviour
{
    public Block block;

    private void Start()
    {
        GetComponent<MeshFilter>().mesh = block.GenerateMesh();
        GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Textures.Instance.opaqueTexture2DArray);
        GetComponent<MeshRenderer>().materials[1].SetTexture("_TextureArray", Textures.Instance.alphaClipTexture2DArray);
        GetComponent<MeshRenderer>().materials[2].SetTexture("_TextureArray", Textures.Instance.transTexture2DArray);
    }
}