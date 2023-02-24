
using System;
using UnityEngine;

public enum MovementState
{
    walking,
    sprinting,
    wallRunning,
    climbing,
    crouching,
    air
}

public class PlayerMovementController : MonoBehaviour {

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    [SerializeField] private float wallRunningSpeed;
    [SerializeField] private float climbSpeed;
    [SerializeField] private FloatEventChannelSO _getPlayerSpeed;

    private float horizontalInput;
    private float verticalInput;
    private bool isMovingLeft, isMovingRight, isMovingForward; 
    private bool isJumping, isCrouching, isWallRunning, isClimbing;
    private bool isExitingWall;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    private bool isGrounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    private PlayerInput playerInput;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private float _currentVelocity;
    public MovementState state;

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
        MyInput();
        ControlSpeed();
        CheckMovementState();
        HandleDrag();

        //Just for debug.
        _currentVelocity = rb.velocity.magnitude;
        _getPlayerSpeed.OnEventRaised.Invoke(_currentVelocity);
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

    private void MyInput()
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
