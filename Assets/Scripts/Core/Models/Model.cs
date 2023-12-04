using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Model", menuName = "Oasis/Model")]
public class Model : ScriptableObject
{
    public ModelElement[] ModelElements;

    public int ComputeNumFaces()
    {
        return ModelElements.Sum(modelElement => modelElement.ModelFaces.Length);
    }
}

[System.Serializable]
public struct ModelTexture
{
    public string Name;
    public TextureObject Texture;
}

[System.Serializable]
public struct ModelElement
{
    public float3 From; // Cross is float
    public float3 To;
    public bool NoShadows;
    public ModelFace[] ModelFaces;
}

[System.Serializable]
public struct ModelFace
{
    public Side Side;
    public int4 UV;
    public TextureObject Texture;
    // public bool Cullface;
}








// public TextureObject East;
//     public TextureObject North;
//     public TextureObject Up;
//     public TextureObject West;
//     public TextureObject South;
//     public TextureObject Down; // TODO Cullface
//     public int4 UpUvs;
//     public int4 DownUvs;
//     public int4 NorthUvs;
//     public int4 SouthUvs;
//     public int4 EastUvs;
//     public int4 WestUvs;
//
//     public int3 RotationOrigin;
//     public int RotationAxis;
//     public int RotationAngle;
//     public bool RotationRescale;
// }