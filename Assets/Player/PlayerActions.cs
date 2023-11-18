using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerActions : MonoBehaviour
{
    public Transform cameraTransform;
    private PlayerInputs playerInputs;
    private Vector2 input;
    private Vector3 direction;
    public float speed;
    private CharacterController controller;
    [SerializeField] private float smoothTime = 0.005f;
    private float currentVelocity;
    private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3;
    [SerializeField] private float jumpPower;
    private float velocity;
    public float mouseSensitivity = 25f;
    private float xRotation = 0f;
    private float yRotation = 0f;
    public Transform highlight;
        

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInputs = new PlayerInputs();
        playerInputs.Enable();
    }

    private void Update()
    {
        ApplyMovement();
        ApplyGravity();
    }

    public void Look(InputAction.CallbackContext context)
    {
        var mouseX = context.ReadValue<Vector2>().x * mouseSensitivity * Time.deltaTime;
        var mouseY = context.ReadValue<Vector2>().y * mouseSensitivity * Time.deltaTime;
    
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0.0f, 0.0f); // xRotation
        
        yRotation += mouseX;
        transform.rotation = Quaternion.Euler(0.0f, yRotation, 0.0f); // yRotation
        Highlight();
    }

    public void Move(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
        direction = new Vector3(input.x, 0.0f, input.y);
        direction = Quaternion.Euler(0.0f, yRotation, 0.0f) * direction;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started && controller.isGrounded)
            velocity += jumpPower;
    }
    
    public void Highlight()
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var hitVector3  = hit.point - (hit.normal * 0.05f);
            var floor = new float3(Mathf.Floor(hitVector3.x), Mathf.Floor(hitVector3.y), Mathf.Floor(hitVector3.z));
            var offset = new float3(0.5f, 0.5f, 0.5f);
            if (!highlight.transform.position.Equals(floor + offset))
                highlight.transform.position = floor + offset;
        }
    }
    
    public void Place(InputAction.CallbackContext context)
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var floor = new float3(Mathf.Floor(hit.point.x), Mathf.Floor(hit.point.y), Mathf.Floor(hit.point.z));
            var offset = new float3(0.5f, 0.5f, 0.5f);
            var xyz = floor + offset;
            // Instantiate(Torch, xyz, Quaternion.identity);
            Debug.Log("Got place " + xyz);
        }
    }
    
    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
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
