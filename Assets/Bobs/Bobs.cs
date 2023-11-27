using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using static Unity.Mathematics.math;

public class Bobs : MonoBehaviour
{
    private static Bobs _instance;
    public static Bobs Instance => _instance;
    
    [Header("Bobs")]
    public int numBobs;
    public GameObject bobPrefab;

    private void Awake()
    {
        _instance = this;
    }
    
    private void Start()
    {
        // CreateBobs();
    }

    private void Update()
    {
    }

    private void CreateBobs()
    {
        for (var i = 0; i < numBobs; i++)
        {
            var x = UnityEngine.Random.Range(0, World.Instance.dims.x);
            var z = UnityEngine.Random.Range(0, World.Instance.dims.z);
            Instantiate(bobPrefab, new Vector3(x, 5f, z), Quaternion.identity, transform);
        }
    }

}