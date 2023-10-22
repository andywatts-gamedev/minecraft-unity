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
    
    void Awake()
    {
        _instance = this;
        opaqueTexture2DArray = new Texture2DArray(16, 16, maxTextures, TextureFormat.DXT1, false);
        alphaClipTexture2DArray = new Texture2DArray(16, 16, maxTextures, TextureFormat.DXT5, false);
        transTexture2DArray = new Texture2DArray(16, 16, maxTextures, TextureFormat.DXT5, false);
        
        for (ushort i=0; i<OpaqueTextures.Count; i++)
            Graphics.CopyTexture(OpaqueTextures[i].Texture, 0, 0, opaqueTexture2DArray, i, 0);

        for (ushort i = 0; i < AlphaClipTextures.Count; i++)
            Graphics.CopyTexture(AlphaClipTextures[i].Texture, 0, 0, alphaClipTexture2DArray, i, 0);

        for (ushort i = 0; i < TransparentTextures.Count; i++)
            Graphics.CopyTexture(TransparentTextures[i].Texture, 0, 0, transTexture2DArray, i, 0);
    }
    
}

