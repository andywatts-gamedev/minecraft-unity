using System;
using System.Collections;
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
    public GameObject blockPrefab;
    public GameObject modelPrefab;

    void Awake()
    {
        _instance = this;
        AirIndex = (ushort) blocks.FindIndex(b => b == Air);
    }

    private void Start()
    {
        if (debug) StartCoroutine(DebugBlocks());
        
        // Create initial blockStates
        BlockStates = new List<BlockState>();
        foreach (var block in blocks)
            BlockStates.Add(new BlockState { Block = block });
    }

    
    private IEnumerator DebugBlocks()
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            if (block.Type == BlockType.Air) continue;
            var go = GameObject.Instantiate(block.Type == BlockType.Model ? modelPrefab : blockPrefab);
            go.transform.parent = transform;
            go.name = block.name;
            go.transform.position = new Vector3(i*2, 0, 0);

            // if (block.Type == BlockType.Model)
            //     go.GetComponent<ModelMono>().block = blocks[i];
            //     // go.GetComponent<ModelMono>().blockState = new BlockState {Block = blocks[i]};
            // else
            //     go.GetComponent<BlockMono>().block = blocks[i];
            
            yield return null;
        }
    }
    
}