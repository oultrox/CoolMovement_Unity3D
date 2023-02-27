
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovementController : MonoBehaviour {

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 20;
    [SerializeField] private float sprintSpeed = 20;
    [SerializeField] private float groundDrag = 5;
    [SerializeField] private float groundDecceleration = 30f;
    [SerializeField] private float wallRunningSpeed;
    [SerializeField] private float climbSpeed;
    [SerializeField] private FloatEventChannelSO playerSpeedChannel;
    private float horizontalInput;
    private float verticalInput;
    private bool isWallRunning, isClimbing;
    private bool isExitingWall;
    private float moveSpeed;
    private Vector3 moveDirection;
    private Vector3 deccelerationForce;

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
    private Rigidbody rBody;
    private PlayerInput playerInput;
    private bool exitingSlope;
    private float currentVelocity;
    private bool isNoInput;
    private const float MOVE_FACTOR = 10;
    private const float SLOPE_FACTOR = 100;
    
    #region Properties
    public bool IsWallRunning { get => isWallRunning; set => isWallRunning = value; }
    public bool IsGrounded { get => isGrounded; set => isGrounded = value; }
    public bool IsClimbing { get => isClimbing; set => isClimbing = value; }
    public bool IsExitingWall { get => isExitingWall; set => isExitingWall = value; }
    public MovementState State { get => state; set => state = value; }
    #endregion

    private void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        rBody.freezeRotation = true;
        readyToJump = true;
    }

    private void Update()
    {
        CheckGround();
        GetInputs();
        LimitSpeed();
        CheckMovementState();

        //Just for debug.
        currentVelocity = rBody.velocity.magnitude;
        playerSpeedChannel.OnEventRaised.Invoke(currentVelocity);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
    }

    private void GetInputs()
    {
        horizontalInput = playerInput.GetHorizontalInput();
        verticalInput = playerInput.GetVerticalInput();

        if (playerInput.GetInputJump() && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    // TODO: Doing switch case directly.
    private void CheckMovementState()
    {
        if(isClimbing)
        {
            state = MovementState.climbing;
            moveSpeed = climbSpeed;
        }

        else if(isWallRunning)
        {
            state = MovementState.wallRunning;
            moveSpeed = wallRunningSpeed;
        }

        else if (isGrounded && playerInput.GetInputLeftShift())
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        else if (isGrounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        if (isExitingWall) return;

        // Apply movement force based on direction and preparing in case there's no input.
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        deccelerationForce = -rBody.velocity.normalized * groundDecceleration;
        isNoInput = moveDirection.magnitude == 0;

        // Slope movement
        if (OnSlope() && !exitingSlope)
        {
            if (isNoInput)
                rBody.AddForce(deccelerationForce, ForceMode.Acceleration);
            else
                rBody.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);
            
            if (rBody.velocity.y > 0)
                rBody.AddForce(Vector3.down * SLOPE_FACTOR, ForceMode.Force);
        }

        // Ground movement
        else if (isGrounded)
        {
            if (isNoInput) 
                rBody.AddForce(deccelerationForce, ForceMode.Acceleration); 
            else
                rBody.AddForce(moveDirection.normalized * moveSpeed * MOVE_FACTOR, ForceMode.Force);
        }

        // Air movement
        else if (!isGrounded)
            rBody.AddForce(moveDirection.normalized * moveSpeed * MOVE_FACTOR * airMultiplier, ForceMode.Force);

        // Turn gravity off while on slope
        rBody.useGravity = !OnSlope();
    }

    private void LimitSpeed()
    {
        // Limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rBody.velocity.magnitude > moveSpeed)
                rBody.velocity = rBody.velocity.normalized * moveSpeed;
        }

        // Limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);

            // Limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rBody.velocity = new Vector3(limitedVel.x, rBody.velocity.y, limitedVel.z);
            }
        }

        if (rBody.velocity.magnitude < 1f)
        {
            rBody.velocity = Vector3.zero;
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rBody.velocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);
        rBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
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
