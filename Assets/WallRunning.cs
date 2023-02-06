using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{

    [Header("Reference")]
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerLook _playerLook;
    private PlayerMovementController movementController;
    private Rigidbody rBody;

    [Header("Basic")]
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float maxWallRunTime;
    [SerializeField] private float wallClimbSpeed;
    [SerializeField] private float wallJumpForce;
    [SerializeField] private float wallJumpSideRoce;
    [SerializeField] private float exitingWallTime = 0.2f;
    [Range(0.2f,10f)]
    [SerializeField] private float smoothWallRunningTransition = 1f;

    [Header("Detection")]
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;

    private float horizontalInput;
    private float verticalInput;
    private PlayerInput playerInput;
    private RaycastHit leftWallHit, rightWallHit;
    private bool wallLeft, wallRight;
    private bool upWardsRunning, downWardsRunning;
    private bool exitingWall;
    private float exitingWallTimer;
    private Vector3 vectorVelocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        movementController = GetComponent<PlayerMovementController>();
        playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWall();
        CheckState();
    }

    private void FixedUpdate()
    {
        if (movementController.IsWallRunning)
        {
            WallRunningMovement();
        }
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void CheckState()
    {
        horizontalInput = playerInput.GetHorizontalInput();
        verticalInput = playerInput.GetVerticalInput();

        upWardsRunning = playerInput.GetInputLeftShift();
        downWardsRunning = playerInput.GetInputCrouch();


        // State 1 - wallrunning
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!movementController.IsWallRunning)
                StartWallRun();

            // Wall jump
            if(playerInput.GetInputJump())
                WallJump();

        }
        // State 3 - None
        else if(exitingWall)
        {
            if(movementController.IsWallRunning)
                StopWallRun();

            if (exitingWallTimer > 0)
                exitingWallTimer -= Time.deltaTime;

            if (exitingWallTimer <= 0)
                exitingWall = false;

        }
        // State 3 - None
        else
        {
            StopWallRun();
        }
    }

    private void StartWallRun()
    {
        movementController.IsWallRunning = true;
        rBody.velocity = Vector3.Lerp(rBody.velocity, new Vector3(rBody.velocity.x, 0, rBody.velocity.z), 
            Time.deltaTime * smoothWallRunningTransition);

        _playerLook.DoFov(90);
        if (wallLeft) _playerLook.DoTilt(-5);
        if (wallRight) _playerLook.DoTilt(5);
    }

    private void StopWallRun()
    {
        if (!movementController.IsWallRunning)
            return;

        movementController.IsWallRunning = false;
        _playerLook.DoFov(80);
        _playerLook.DoTilt(0);
    }

    private void WallRunningMovement()
    {
        rBody.useGravity = false;
        rBody.velocity = new Vector3(rBody.velocity.x, 0, rBody.velocity.z);
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        //forward force
        rBody.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //Upwards/Downwards force
        if(upWardsRunning)
            rBody.velocity = new Vector3(rBody.velocity.x, wallClimbSpeed, rBody.velocity.z);
        if(downWardsRunning)
            rBody.velocity = new Vector3(rBody.velocity.x, -wallClimbSpeed, rBody.velocity.z);

        //Push player towards the wall
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rBody.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    private void WallJump()
    {
        exitingWall = true;
        exitingWallTimer = exitingWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpForce + wallNormal * wallJumpSideRoce;

        rBody.velocity = new Vector3(rBody.velocity.x, 0, rBody.velocity.z);
        rBody.AddForce(forceToApply, ForceMode.Impulse);
    }
}