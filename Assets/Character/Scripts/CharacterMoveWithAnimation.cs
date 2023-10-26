using UnityEngine;
using UnityEngine.AI;

public class CharacterMoveWithAnimation : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;
    private CharacterController character;

    
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;


    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private Animator _animator;
    private CharacterController _controller;
    private GameObject _mainCamera;
    
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        character = GetComponent<CharacterController>();
        // agent.SetDestination(target.position);
        _animator = GetComponent<Animator>();
        AssignAnimationIDs();
    }

    private void Update()
    {
        // Calculate the direction from the character's current position to the target's position
        Vector3 moveDirection = (target.position - transform.position).normalized;

        // Calculate the target rotation to face the destination
        float targetRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

        // Smoothly rotate the character towards the target rotation
        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref _rotationVelocity, RotationSmoothTime);
        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

        // Calculate the distance to the target position
        float distanceToDestination = Vector3.Distance(transform.position, target.position);

        // Check if the character has reached the destination
        if (distanceToDestination > agent.stoppingDistance)
        {
            // Move the character towards the target position
            character.SimpleMove(moveDirection * MoveSpeed);
        
            // Set the animator parameters for animation blending
            float animationBlend = Mathf.Lerp(_animationBlend, MoveSpeed, Time.deltaTime * SpeedChangeRate);
            _animationBlend = Mathf.Clamp(animationBlend, 0f, MoveSpeed);
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, 1.0f);
            
        }
        else
        {
            // Stop the character when it reaches the destination
            character.SimpleMove(Vector3.zero);
        
            // Set the animator parameters for animation blending when stopped
            _animationBlend = 0f;
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, 0f);
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

}