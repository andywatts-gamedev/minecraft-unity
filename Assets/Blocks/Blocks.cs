using System;
using System.Collections.Generic;
using UnityEngine;

public class Blocks : MonoBehaviour
{
    private static Blocks _instance;
    public static Blocks Instance => _instance;
    public List<Block> blocks;
    public List<BlockState> BlockStates;
    public Block Air;
    public ushort AirIndex;
    
    public bool debug;
    public GameObject prefab;

    void Awake()
    {
        _instance = this;
        AirIndex = (ushort) blocks.FindIndex(b => b == Air);
    }

    private void Start()
    {
        if (debug) DebugBlocks();
        
        // Create initial blockStates
        BlockStates = new List<BlockState>();
        foreach (var block in blocks)
            BlockStates.Add(new BlockState { Block = block });
    }

    
    private void DebugBlocks()
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var go = GameObject.Instantiate(prefab);
            go.transform.parent = transform;
            go.name = block.name;
            go.GetComponent<BlockMono>().block = blocks[i];
            go.transform.position = new Vector3(i*2, 0, 0);
        }
    }
    
}