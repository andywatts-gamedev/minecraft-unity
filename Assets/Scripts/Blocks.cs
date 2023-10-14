using System.Collections.Generic;
using UnityEngine;


public class Blocks : MonoBehaviour
{
    private static Blocks _instance;
    public static Blocks Instance => _instance;
    public List<Block> blocksList;
    public Dictionary<int, Block> blocks;
    public Texture2DArray opaqueTexture2DArray;
    public Texture2DArray transTexture2DArray;
    public int maxTextures = 32;

    void Awake()
    {
        _instance = this;
        blocks = new Dictionary<int, Block>();
    
        
        opaqueTexture2DArray = new Texture2DArray(16, 16, maxTextures, TextureFormat.DXT1, false);
        transTexture2DArray = new Texture2DArray(16, 16, maxTextures, TextureFormat.DXT5, false);
        var opaqueTextureCount = 0;
        var transTextureCount = 0;
        
        
        for (int i=0; i<blocksList.Count; i++)
        {
            var block = blocksList[i];
            blocks[i] = block;
            Debug.Log($"{i} {block.name}");
            if (block.texture == null) continue;

            var pixels = blocksList[i].texture.GetPixels();
            if (block.texture.format == TextureFormat.DXT1 && block.type == BlockType.Opaque)
            {
                Debug.Log($"{block.name} {block.type} {block.texture.format} {opaqueTexture2DArray.format}");
                Graphics.CopyTexture(block.texture, 0, 0, opaqueTexture2DArray, opaqueTextureCount, 0);
                // opaqueTexture2DArray.SetPixels(pixels, opaqueTextureCount);
                block.textureArrayIndex = opaqueTextureCount;
                opaqueTextureCount++;
            }
            else if (block.texture.format == TextureFormat.DXT5 && block.type == BlockType.Transparent)
            {
                // transTexture2DArray.SetPixels(pixels, transTextureCount);
                Graphics.CopyTexture(block.texture, 0, 0, transTexture2DArray, transTextureCount, 0);
                block.textureArrayIndex = transTextureCount;
                transTextureCount++;
            }
            else
            {
                Debug.LogError($"Block {block.name} has {block.texture.format} texture, but type {block.type}");
            }
            Debug.Log($"{block.name} {block.type} added to textureArray");
            
        }
    }
    
    void Start()
    {
    }
    
}