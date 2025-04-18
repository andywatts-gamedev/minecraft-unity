using UnityEngine;

[CreateAssetMenu(fileName = "Texture", menuName = "Oasis/Texture")]
public class TextureObject : ScriptableObject
{
    public string Name;
    public Texture2D Texture;
    public TextureType Type;
    public int TextureIndex;
    public byte Metallic;
    public byte Smoothness;
}