using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class Block : ScriptableObject
{
    public BlockType Type;
    [SerializeField] public TextureType TextureType;
    public Texture2D Image;
    public BlockState[] BlockStates;
}
