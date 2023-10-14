using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Block", menuName = "Oasis/Block")]
public class Block : ScriptableObject
{
    public string name;
    public BlockType type;
    public Texture2D texture;
    public int textureArrayIndex;
}
