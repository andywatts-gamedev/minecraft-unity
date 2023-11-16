using System;
using System.Collections;
using System.Collections.Generic;
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
    
        playerInputs.Player.Look.performed += context => {
            var mouseX = context.ReadValue<Vector2>().x * mouseSensitivity * Time.deltaTime;
            var mouseY = context.ReadValue<Vector2>().y * mouseSensitivity * Time.deltaTime;
        
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0.0f, 0.0f); // xRotation
            
            
            yRotation += mouseX;
            transform.rotation = Quaternion.Euler(0.0f, yRotation, 0.0f); // yRotation
        };
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
