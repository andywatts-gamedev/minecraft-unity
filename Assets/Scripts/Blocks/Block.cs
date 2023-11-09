using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Block", menuName = "Oasis/Block")]
public class Block : ScriptableObject
{
    public string Name;
    public BlockType Type;
    public SideTexture[] SideTextures;
    public ModelElement[] ModelElements;
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
    public TextureObject Down; // TODO Cullface
    public TextureObject Up;
    public TextureObject North;
    public TextureObject South;
    public TextureObject East;
    public TextureObject West;
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