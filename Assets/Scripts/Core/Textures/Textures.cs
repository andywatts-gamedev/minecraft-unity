using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Textures : MonoBehaviour
{
    private static Textures _instance;
    public static Textures Instance => _instance;

    
    public int maxTextures = 32;
    
    public List<TextureObject> OpaqueTextures;
    public List<TextureObject> TransparentTextures;
    public List<TextureObject> AlphaClipTextures;
    
    public Texture2DArray opaqueTexture2DArray;
    public Texture2DArray alphaClipTexture2DArray;
    public Texture2DArray transTexture2DArray;

    public Material[] LitMaterials;
    public Material[] UnlitMaterials;
    
    void Awake()
    {
        _instance = this;
        opaqueTexture2DArray = new Texture2DArray(16, 16, maxTextures, TextureFormat.DXT1, false);
        alphaClipTexture2DArray = new Texture2DArray(16, 16, maxTextures, TextureFormat.DXT5, false);
        transTexture2DArray = new Texture2DArray(16, 16, maxTextures, TextureFormat.DXT5, false);
        
        for (ushort i = 0; i < OpaqueTextures.Count; i++)
        {
            OpaqueTextures[i].TextureIndex = i;
            Graphics.CopyTexture(OpaqueTextures[i].Texture, 0, 0, opaqueTexture2DArray, i, 0);
        }

        for (ushort i = 0; i < AlphaClipTextures.Count; i++)
        {
            AlphaClipTextures[i].TextureIndex = i;
            Graphics.CopyTexture(AlphaClipTextures[i].Texture, 0, 0, alphaClipTexture2DArray, i, 0);
        }
        
        for (ushort i = 0; i < TransparentTextures.Count; i++)
        {
            TransparentTextures[i].TextureIndex = i;
            Graphics.CopyTexture(TransparentTextures[i].Texture, 0, 0, transTexture2DArray, i, 0);
        }

        LitMaterials[0].SetTexture("_TextureArray", opaqueTexture2DArray);
        LitMaterials[1].SetTexture("_TextureArray", alphaClipTexture2DArray);
        LitMaterials[2].SetTexture("_TextureArray", transTexture2DArray);

        UnlitMaterials[0].SetTexture("_TextureArray", opaqueTexture2DArray);
        UnlitMaterials[1].SetTexture("_TextureArray", alphaClipTexture2DArray);
        UnlitMaterials[2].SetTexture("_TextureArray", transTexture2DArray);
    }
   
}

