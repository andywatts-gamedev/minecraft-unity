using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Block", menuName = "Oasis/Block")]
public class Block : ScriptableObject
{
    public string Name;
    public BlockType Type;
    public SideTexture[] SideTextures;
}


[System.Serializable]
public struct SideTexture
{
    public Side Side;
    // public Texture2D Texture;
    // public int TextureIndex;
    public TextureObject TextureObject;
}
// East, Up, North, West, Down, South


