using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocks : MonoBehaviour
{
    private static Blocks _instance;
    public static Blocks Instance => _instance;
    
    void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
    }

    
    // private IEnumerator DebugBlocks()
    // {
    //     for (int i = 0; i < blocks.Count; i++)
    //     {
    //         var block = blocks[i];
    //         if (block.Type == BlockType.Air) continue;
    //         var go = GameObject.Instantiate(block.Type == BlockType.Model ? modelPrefab : blockPrefab);
    //         go.transform.parent = transform;
    //         go.name = block.name;
    //         go.transform.position = new Vector3(i*2, 0, 0);
    //         yield return null;
    //     }
    // }
    
}