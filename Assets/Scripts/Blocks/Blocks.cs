using System.Collections.Generic;
using UnityEngine;


public class Blocks : MonoBehaviour
{
    private static Blocks _instance;
    public static Blocks Instance => _instance;
    public List<Block> blocksList;
    public Dictionary<ushort, Block> blocks;

    void Awake()
    {
        _instance = this;
        blocks = new Dictionary<ushort, Block>();
        
        for (ushort i=0; i<blocksList.Count; i++)
            blocks[i] = blocksList[i];
    }
    
}