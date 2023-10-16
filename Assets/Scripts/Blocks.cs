using System.Collections.Generic;
using UnityEngine;


public class Blocks : MonoBehaviour
{
    private static Blocks _instance;
    public static Blocks Instance => _instance;
    public List<Block> blocksList;
    public Dictionary<int, Block> blocks;
    public Texture2DArray opaqueTexture2DArray;
    public Texture2DArray alphaClipTexture2DArray;
    public Texture2DArray transTexture2DArray;
    public int maxTextures = 32;

    void Awake()
    {
        _instance = this;
        blocks = new Dictionary<int, Block>();
        opaqueTexture2DArray = new Texture2DArray(16, 16, maxTextures, TextureFormat.DXT1, false);
        alphaClipTexture2DArray = new Texture2DArray(16, 16, maxTextures, TextureFormat.DXT5, false);
        transTexture2DArray = new Texture2DArray(16, 16, maxTextures, TextureFormat.DXT5, false);
        var opaqueTextureCount = 0;
        var transTextureCount = 0;
        var alphaClipTextureCount = 0;
        
        for (int i=0; i<blocksList.Count; i++)
        {
            var block = blocksList[i];
            blocks[i] = block;
            if (block.texture == null) continue;
            
            if (block.texture.format == TextureFormat.DXT1 && block.type == BlockType.Opaque)
            {
                Graphics.CopyTexture(block.texture, 0, 0, opaqueTexture2DArray, opaqueTextureCount, 0);
                block.textureArrayIndex = opaqueTextureCount;
                opaqueTextureCount++;
            }
            else if (block.texture.format == TextureFormat.DXT5 && block.type == BlockType.AlphaClip)
            {
                Graphics.CopyTexture(block.texture, 0, 0, alphaClipTexture2DArray, alphaClipTextureCount, 0);
                block.textureArrayIndex = alphaClipTextureCount;
                alphaClipTextureCount++;
            }
            else if (block.texture.format == TextureFormat.DXT5 && block.type == BlockType.Transparent)
            {
                Graphics.CopyTexture(block.texture, 0, 0, transTexture2DArray, transTextureCount, 0);
                block.textureArrayIndex = transTextureCount;
                transTextureCount++;
            }
            else
            {
                Debug.LogError($"Block {block.name} has {block.texture.format} texture, but type {block.type}");
            }
            Debug.Log($"{block.name} {block.type} added to textureArray with index {block.textureArrayIndex}");
            
        }
    }
    
    void Start()
    {
    }
    
}