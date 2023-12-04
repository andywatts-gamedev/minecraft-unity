using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Blueprint", menuName = "Oasis/Blueprint")]
public class Blueprint : ScriptableObject
{
    public string name;
    public int3 dims;
    public Block[] blocks; // xyz
}