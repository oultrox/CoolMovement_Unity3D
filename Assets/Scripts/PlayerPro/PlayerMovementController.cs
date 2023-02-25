
using System;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour {

    [Header("Movement")]
    private float moveSpeed;
    [SerializeField] private float walkSpeed = 20;
    [SerializeField] private float sprintSpeed = 20;
    [SerializeField] private float groundDrag = 5;
    [SerializeField] private float wallRunningSpeed;
    [SerializeField] private float climbSpeed;
    [SerializeField] private FloatEventChannelSO playerSpeedChannel;

    private float horizontalInput;
    private float verticalInput;
    private bool isMovingLeft, isMovingRight, isMovingForward;
    private bool isJumping, isCrouching, isWallRunning, isClimbing;
    private bool isExitingWall;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 35;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float airMultiplier = 0.4f;
    private bool readyToJump;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 1.8f;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 40;
    [SerializeField] private Transform orientation;
    
    //Just for showcasing the state debug
    [SerializeField] private MovementState state;

    private RaycastHit slopeHit;
    private bool exitingSlope;
    private PlayerInput playerInput;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private float _currentVelocity;
    

    #region Properties
    public bool IsWallRunning { get => isWallRunning; set => isWallRunning = value; }
    public bool IsGrounded { get => isGrounded; set => isGrounded = value; }
    public bool IsClimbing { get => isClimbing; set => isClimbing = value; }
    public bool IsExitingWall { get => isExitingWall; set => isExitingWall = value; }
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        rb.freezeRotation = true;
        readyToJump = true;
    }

    private void Update()
    {
        CheckGround();
        GetInputs();
        ControlSpeed();
        CheckMovementState();
        HandleDrag();

        //Just for debug.
        _currentVelocity = rb.velocity.magnitude;
        playerSpeedChannel.OnEventRaised.Invoke(_currentVelocity);
    }

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
    }

    private void HandleDrag()
    {
        // handle drag
        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void GetInputs()
    {
        horizontalInput = playerInput.GetHorizontalInput();
        verticalInput = playerInput.GetVerticalInput();

        // when to jump
        if (playerInput.GetInputJump() && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void CheckMovementState()
    {
        // Climbing 
        if(isClimbing)
        {
            state = MovementState.climbing;
            moveSpeed = climbSpeed;
        }

        // Wallruning
        else if(isWallRunning)
        {
            state = MovementState.wallRunning;
            moveSpeed = wallRunningSpeed;
        }

        // Sprinting
        else if (isGrounded && playerInput.GetInputLeftShift())
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        // Walking
        else if (isGrounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        // Air
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        if (isExitingWall) return;

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void ControlSpeed()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
