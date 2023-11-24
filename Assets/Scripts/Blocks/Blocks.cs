using System.Collections.Generic;
using UnityEngine;


public class Blocks : MonoBehaviour
{
    private static Blocks _instance;
    public static Blocks Instance => _instance;
    public List<Block> blocks;
    public Block Air;

    void Awake()
    {
        _instance = this;
    }
    
}