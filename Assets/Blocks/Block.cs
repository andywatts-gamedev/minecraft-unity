using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Block", menuName = "Oasis/Block")]
public class Block : ScriptableObject
{
    public string BlockName;
    public BlockType Type;
    [SerializeField] public TextureType TextureType;
    public SideTexture[] SideTextures;
    public ModelElement[] ModelElements;
    public Texture2D image;

    public Mesh GenerateMesh()
    {
        var dims   = new int3(1, 1, 1);
        var voxels = new NativeArray<ushort>(1, Allocator.Temp);
        voxels[0]  = (ushort) Blocks.Instance.blocks.FindIndex(b => b == this);
        var mesh   = Mesher.Compute(dims, voxels);
        return mesh;
    }
}

[System.Serializable]
public struct SideTexture
{
    public Side Side;
    public TextureObject TextureObject;
}
// East, Up, North, West, Down, South



[System.Serializable]
public struct ModelElement
{
    public float3 From; // Cross is float
    public float3 To;
    public TextureObject East;
    public TextureObject North;
    public TextureObject Up;
    public TextureObject West;
    public TextureObject South;
    public TextureObject Down; // TODO Cullface
    public int4 UpUvs;
    public int4 DownUvs;
    public int4 NorthUvs;
    public int4 SouthUvs;
    public int4 EastUvs;
    public int4 WestUvs;

    public int3 RotationOrigin;
    public int RotationAxis;
    public int RotationAngle;
    public bool RotationRescale;
}