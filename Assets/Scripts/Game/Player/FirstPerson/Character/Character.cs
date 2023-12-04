using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour
{
    public Transform cameraTransform;
    public float speed;
    [SerializeField] private float smoothTime = 0.005f;
    [SerializeField] private float gravityMultiplier = 3;
    [SerializeField] private float jumpPower;
    public float mouseSensitivity = 25f;
    private CharacterController controller;
    private float currentVelocity;
    private Vector3 direction;
    private FirstPersonInputs firstPersonInputs;
    private readonly float gravity = -9.81f;
    private Vector2 input;
    private float velocity;
    private float xRotation;
    private float yRotation;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        firstPersonInputs = new FirstPersonInputs();
        firstPersonInputs.Enable();
    }

    private void Update()
    {
        ApplyMovement();
        ApplyGravity();
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
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
        Highlight.Instance.MoveHighlight();
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