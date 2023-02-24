using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerClimbing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] LayerMask whatIsWall;

    [Header("Climbing")]
    [SerializeField] private float climbSpeed;
    [SerializeField] private float maxClimbTime;
     
    [Range(0.2f, 10f)]
    [SerializeField] private float climbSmoothFactor = 1f;

    [Header("Detection")]
    [SerializeField] private float detectionLength;
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private float maxWallLookAngle;

    [Header("ClimbJumping")]
    [SerializeField] private float climbJumpForce;
    [SerializeField] private float climbJumpBackForce;
    [SerializeField] private int climbJumpsAmount;

    [Header("Exiting")]
    [SerializeField] private float exitWallTime = 0.2f;
    private bool isExitingWall;
    private float exitWallTimer;
    private int climbJumpsLeft;
    private KeyCode jumpKey = KeyCode.Space;
    private Transform lastWall;
    private Vector3 lastWallNormal;
    private readonly float minWallNormalAngleChange = 5;
    private float wallLookAngle;
    private float climbTimer;
    private PlayerMovementController movementController;
    private RaycastHit frontwallHit;
    private bool isClimbing;
    private bool isFrontWall;
    private Rigidbody rBody;
    private bool isNewWall;
    private Vector3 forceToApply;

    private void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        movementController = GetComponent<PlayerMovementController>();
    }

    void Update()
    {
        WallCheck();
        CheckState();
        ClimbingMovement();
    }

    private void CheckState()
    {
        if (isFrontWall && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle)
        {
            if (!isClimbing && climbTimer > 0) StartClimbing();
            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimbing();
        }
        else if(isExitingWall)
        {
            if (isClimbing) StopClimbing();
            
            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0) isExitingWall = false;
            movementController.IsExitingWall = isExitingWall;
        }
        else
        {
            if (isClimbing) StopClimbing();
        }

        if (isFrontWall && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0) ClimbJump();
    }



    
    private void WallCheck()
    {
        isFrontWall = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontwallHit, 
            detectionLength, whatIsWall);
        
        wallLookAngle = Vector3.Angle(orientation.forward, -frontwallHit.normal);
        
        isNewWall = frontwallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontwallHit.normal)) > minWallNormalAngleChange;
        
        if((isFrontWall && isNewWall) || movementController.IsGrounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumpsAmount;
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;
        movementController.IsClimbing = isClimbing;

        lastWall = frontwallHit.transform;
        lastWallNormal = frontwallHit.normal;
    }

    private void ClimbingMovement()
    {
        if (isClimbing == false || isExitingWall)
            return;

        rBody.velocity = Vector3.Lerp(rBody.velocity, new Vector3(rBody.velocity.x, climbSpeed, rBody.velocity.z),
            Time.deltaTime * climbSmoothFactor);
    }

    private void StopClimbing()
    {
        isClimbing = false;
        movementController.IsClimbing = isClimbing;
    }

    
    private void ClimbJump()
    {
        isExitingWall = true;
        exitWallTimer = exitWallTime;
        movementController.IsExitingWall = true;

        forceToApply = transform.up * climbJumpForce + frontwallHit.normal * climbJumpBackForce;
        rBody.velocity = new Vector3(rBody.velocity.x, 0, rBody.velocity.z);
        rBody.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}
