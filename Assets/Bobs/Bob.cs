using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Bob : MonoBehaviour
{
    Vector3 targetBuildingSite;
    public GameObject housePrefab;
    public GameObject house;
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
        foreach (var buildingSite in World.Instance.buildingSites)
        {
            if (targetBuildingSite == Vector3.zero)
                targetBuildingSite = buildingSite;
            else
                if (Vector3.Distance(transform.position, buildingSite) < Vector3.Distance(transform.position, targetBuildingSite))
                    targetBuildingSite = buildingSite;
        }

        // Take closest building site and remove it from the list
        if (targetBuildingSite != Vector3.zero)
        {
            World.Instance.buildingSites.Remove(targetBuildingSite);
            navMeshAgent.SetDestination(targetBuildingSite);
        }

        house = Instantiate(housePrefab, targetBuildingSite, Quaternion.identity, transform.parent);
    }
    
    void Update()
    {
        if (house != null)
            Debug.DrawRay(transform.position, targetBuildingSite - transform.position, Color.red);

        if (navMeshAgent.remainingDistance < 0.05f)
        {
            navMeshAgent.isStopped = true;
            GetComponent<MeshRenderer>().material.SetColor("Color", Color.green);
        }

        // if arrived...jump
        if (navMeshAgent.isStopped)
        {
            ApplyMovement();
            ApplyGravity();
            if (controller.isGrounded)
                velocity += jumpPower; // Jump
        }
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
