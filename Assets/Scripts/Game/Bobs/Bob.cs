using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Bob : MonoBehaviour
{
    [Header("Building Site")]
    public Vector3 targetBuildingSite;
    public GameObject buildingSitePrefab;
    public GameObject buildingSiteGO;
    
    [Header("Building")]
    public int3 buildingXYZ;
    public Blueprint buildingBlueprint;
    
    [Header("Character")]
    public CharacterController controller;
    private Vector3 direction;
    private float velocity;
    public float speed;
    private float currentVelocity;
    private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3;
    [SerializeField] private float jumpPower;
    public NavMeshAgent navMeshAgent;
    
    void Start()
    {
        // Find closest building site
        if (World.Instance.buildingSites.Count == 0)
            Debug.LogError("No building sites found!");
        foreach (var buildingSite in World.Instance.buildingSites)
            if (Vector3.Distance(transform.position, buildingSite) < Vector3.Distance(transform.position, targetBuildingSite))
                targetBuildingSite = buildingSite;
        Debug.Log("Found building site " + targetBuildingSite);
        
        // Take closest building site and remove it from the list
        if (targetBuildingSite != Vector3.zero)
        {
            World.Instance.buildingSites.Remove(targetBuildingSite);
            navMeshAgent.SetDestination(targetBuildingSite);
        }

        // Create building site GO
        buildingSiteGO = Instantiate(buildingSitePrefab, targetBuildingSite, Quaternion.identity, transform.parent);
    }
    
    void Update()
    {
        // Debug ray to destination
        if (buildingSiteGO != null)
            Debug.DrawRay(transform.position, targetBuildingSite - transform.position, Color.red);

        // Reached destination
        if (navMeshAgent.remainingDistance < 0.05f)
        {
            navMeshAgent.isStopped = true;
            GetComponent<MeshRenderer>().material.SetColor("Color", Color.green);
        }

        // if arrived
        if (navMeshAgent.isStopped)
        {
            // Jump
            ApplyMovement();
            ApplyGravity();
            if (controller.isGrounded)
                velocity += jumpPower; // Jump
            
            // Destroy debug house
            if (buildingSiteGO != null) GameObject.Destroy(buildingSiteGO);


            if (buildingXYZ.Equals(default))
            {
                StartCoroutine(BuildBlueprint());
            }
        }
    }

    private IEnumerator BuildBlueprint()
    {
        yield return null;
        // yield return new WaitForSeconds(2f);
        buildingXYZ = targetBuildingSite.ToInt3(); // Store building location
        for (var x = 0; x < buildingBlueprint.dims.x; x++)
        for (var y = 0; y < buildingBlueprint.dims.y; y++)
        for (var z = 0; z < buildingBlueprint.dims.z; z++)
        {
            var blueprintXyz = new int3(x, y, z);
            var block = buildingBlueprint.blocks[blueprintXyz.ToIndex(buildingBlueprint.dims)];
            if (block != Blocks.Instance.Air)
            {
                var blockId = (ushort) Blocks.Instance.blocks.FindIndex(b => b == block);
                var voxelXyz = targetBuildingSite.ToInt3() + blueprintXyz - buildingBlueprint.dims / 2;
                World.Instance.voxels[voxelXyz.ToIndex(World.Instance.dims)] = blockId;
            }
            yield return null;
        }

        // World.Instance.UpdateMesh();
    }


    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity < 0)
            velocity = -1;
        else
            velocity += gravity * gravityMultiplier * Time.deltaTime;
        direction.y = velocity;
    }

    private void ApplyMovement()
    {
        controller.Move(direction * speed * Time.deltaTime);
    }
}
