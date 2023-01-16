
using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [Header("Assignables")]
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform orientation;

    private Rigidbody rBody;
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;

    [Header("Movement")]
    [SerializeField] private float moveAcceleration = 4500;
    [SerializeField] private float maxSpeed = 20;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float counterMovement = 0.175f;
    [SerializeField] private float maxSlopeAngle = 35f;
    
    private bool isGrounded;
    private bool isMovingLeft, isMovingRight, isMovingForward, isForwardDown;
    private float movementX, movementY;
    private bool jumping, sprinting, crouching;
    private float startMaxSpeed;
    private float threshold = 0.01f;
    private float desiredX;

    [Header("Crouch & Slide")]
    [SerializeField] private float slideAcceleration = 400;
    [SerializeField] private float slideCounterMovement = 0.2f;
    [SerializeField] private float crouchGravityMultiplier;
    
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 550f;
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
   
    [Header("Wallrunning")]
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private float wallRunAcceleration, maxWallrunTime, maxWallSpeed;
    [SerializeField] private float maxWallRunCameraTilt;
    [SerializeField] private int startDoubleJumps = 1;
    [SerializeField] private float wallRunForwardMultiplier = 5;
    private float wallRunCameraTilt;
    private int doubleJumpsLeft;
    private bool cancellingGrounded;
    private bool isWallRight, isWallLeft;
    private bool isWallRunning;

    [Header("Dash")]
    [SerializeField] private float dashForce;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashTime;
    private bool readyToDash;
    private int wTapTimes = 0;
    private Vector3 dashStartVector;

    [Header("RocketBoost")]
    [SerializeField] private float maxRocketTime;
    [SerializeField] private float rocketForce;
    private bool rocketActive, readyToRocket;
    private bool alreadyInvokedRockedStop;
    private float rocketTimer;

    [Header("SonicSpeed")]
    [SerializeField] private float maxSonicSpeed;
    [SerializeField] private float sonicSpeedForce;
    [SerializeField] private float timeBetweenNextSonicBoost;
    private float timePassedSonic;

    [Header("Flash")]
    [SerializeField] private float flashCooldown, flashRange;
    [SerializeField] private int maxFlashesLeft;
    [SerializeField] private int flashesLeft = 3;
   
    private bool alreadySubtractedFlash;

    [Header("Climbing")]
    [SerializeField] private float climbForce, maxClimbSpeed;
    [SerializeField] private LayerMask whatIsLadder;
    private bool alreadyStoppedAtLadder;


    void Awake() 
    {
        rBody = GetComponent<Rigidbody>();
    }

    void Start() 
    {
        playerScale = transform.localScale;
    }

    private void Update()
    {
        Look();
        CheckForWall();
        CheckDash();
        //CheckRocketFlight();
        //CheckClimb();
        //SonicSpeed();
    }


    private void FixedUpdate()
    {
        Movement();
    }

    #region Inputs
    internal void SetInputDirectional(float horizontal, float vertical)
    {
        movementX = horizontal;
        movementY = vertical;
    }

    internal void SetInputLeft(bool leftKey)
    {
        isMovingLeft = leftKey;
        if (isMovingLeft && isWallLeft) StartWallRun();
    }

    internal void SetInputRight(bool rightKey)
    {
        isMovingRight = rightKey;
        if (isMovingRight && isWallRight) StartWallRun();
    }

    internal void SetInputForward(bool frontKey)
    {
        isMovingForward = frontKey;
    }

    internal void SetInputJump(bool isJumping)
    {
        jumping = isJumping; 
    }

    internal void SetInputKeyCrouch(bool isCrouching)
    {
        crouching = isCrouching;
    }

    internal void SetInputUpCrouch(bool isCrouching)
    {
        if (isCrouching)
        {
            StopCrouch();
        }
    }

    internal void SetInputDownCrouch(bool isCrouching)
    {
        if (isCrouching)
        {
            StartCrouch();
        }
    }
    internal void SetInputDownForward(bool isKeyDown)
    {
        isForwardDown = isKeyDown;
    }
    #endregion Inputs

    private void CheckDash()
    {
        if (isForwardDown && wTapTimes <= 1 )
        { 
            wTapTimes++;
            Invoke(nameof(ResetTapTimes), 0.3f);
        }
        if (wTapTimes == 2 && readyToDash)
        {
            Debug.Log("Dashing");
            Dash();
        }

        //SideFlash
        if (Input.GetKeyDown(KeyCode.Mouse1) && flashesLeft > 0 && movementX > 0) SideFlash(true);
        if (Input.GetKeyDown(KeyCode.Mouse1) && flashesLeft > 0 && movementY < 0) SideFlash(false);
    }

    private void CheckRocketFlight()
    {
        //RocketFlight
        if (crouching && readyToRocket)
        {
            //Dampens velocity
            rBody.velocity = rBody.velocity / 3;
        }
        if (crouching && readyToRocket)
            StartRocketBoost();
    }

    private void CheckClimb()
    {
        //Climbing
        if (Physics.Raycast(transform.position, orientation.forward, 1, whatIsLadder) && movementY > .9f)
            Climb();
        else alreadyStoppedAtLadder = false;
    }

    private void ResetTapTimes()
    {
        wTapTimes = 0;
    }

    private void StartCrouch() {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rBody.velocity.magnitude > 0.5f) {
            if (isGrounded) {
                rBody.AddForce(orientation.transform.forward * slideAcceleration);
            }
        }
    }

    private void StopCrouch() {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement()
    {
        //Extra gravity
        //Needed that the Ground Check works better!
        float gravityMultiplier = 10;

        if (crouching) gravityMultiplier = crouchGravityMultiplier;

        rBody.AddForce(Vector3.down * Time.deltaTime * gravityMultiplier);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(movementX, movementY, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping && isGrounded )
        {
            Jump();
        }
        else if (jumping && !isGrounded && readyToJump && doubleJumpsLeft >= 1)
        {
            Debug.Log("Jumping");
            Jump();
            doubleJumpsLeft--;
        }

        //ResetStuff when touching ground
        if (isGrounded)
        {
            readyToDash = true;
            readyToRocket = true;
            doubleJumpsLeft = startDoubleJumps;
        }

        //Set max speed
        float maxSpeed = this.maxSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && isGrounded && readyToJump)
        {
            rBody.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (movementX > 0 && xMag > maxSpeed) movementX = 0;
        if (movementX < 0 && xMag < -maxSpeed) movementX = 0;
        if (movementY > 0 && yMag > maxSpeed) movementY = 0;
        if (movementY < 0 && yMag < -maxSpeed) movementY = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!isGrounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (isGrounded && crouching) multiplierV = 0f;

        //Apply forces to move player
        rBody.AddForce(orientation.transform.forward * movementY * moveAcceleration * Time.deltaTime * multiplier * multiplierV);
        rBody.AddForce(orientation.transform.right * movementX * moveAcceleration * Time.deltaTime * multiplier);
    }

    private void Jump() 
    {
        if (isGrounded) 
        {
            readyToJump = false;
            rBody.AddForce(Vector2.up * jumpForce * 1.5f);
            rBody.AddForce(normalVector * jumpForce * 0.5f);
            
            //If jumping while falling, reset movementY velocity.
            Vector3 vel = rBody.velocity;
            if (rBody.velocity.y < 0.5f)
                rBody.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rBody.velocity.y > 0) 
                rBody.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if(!isGrounded)
        {
            readyToJump = false;
            rBody.AddForce(orientation.forward * jumpForce * 1);
            rBody.AddForce(Vector2.up * jumpForce * 1.5f);
            rBody.AddForce(normalVector * jumpForce * 0.5f);

            rBody.velocity = Vector3.zero;
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (isWallRunning)
        {
            readyToJump = false;

            //normal jump
            if (isWallLeft && !isMovingRight || isWallRight && !isMovingLeft)
            {
                rBody.AddForce(Vector2.up * jumpForce * 1.5f);
                rBody.AddForce(normalVector * jumpForce * 0.5f);
            }

            //sidwards wallhop
            if (isWallRight || isWallLeft && isMovingLeft || isMovingRight) rBody.AddForce(-orientation.up * jumpForce * 1f);
            if (isWallRight && isMovingLeft) rBody.AddForce(-orientation.right * jumpForce * 3.2f);
            if (isWallLeft && isMovingRight) rBody.AddForce(orientation.right * jumpForce * 3.2f);

            //Always add forward force
            rBody.AddForce(orientation.forward * jumpForce * 1f);

            Invoke(nameof(ResetJump), jumpCooldown);
        }

    }
    
    private void ResetJump() 
    {
        readyToJump = true;
    }
    
    private void Look() 
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;
        
        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, wallRunCameraTilt);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);

        //While Wallrunning
        //Tilts camera in .5 second
        if (Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallRight)
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
        
        if (Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallLeft)
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;

        //Tilts camera back again
        if (wallRunCameraTilt > 0 && !isWallRight && !isWallLeft)
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;
        
        if (wallRunCameraTilt < 0 && !isWallRight && !isWallLeft)
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
    }

    private void CounterMovement(float x, float y, Vector2 mag) {
        if (!isGrounded || jumping) return;

        //Slow down sliding
        if (crouching) {
            rBody.AddForce(moveAcceleration * Time.deltaTime * -rBody.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0)) {
            rBody.AddForce(moveAcceleration * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0)) {
            rBody.AddForce(moveAcceleration * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }
        
        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rBody.velocity.x, 2) + Mathf.Pow(rBody.velocity.z, 2))) > maxSpeed) {
            float fallspeed = rBody.velocity.y;
            Vector3 n = rBody.velocity.normalized * maxSpeed;
            rBody.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    private void StartWallRun()
    {
        rBody.useGravity = false;
        isWallRunning = true;

        if (rBody.velocity.magnitude <= maxWallSpeed)
        {
            Debug.Log("Are you applying this force?");
            rBody.AddForce(orientation.forward * (wallRunAcceleration * wallRunForwardMultiplier) * Time.deltaTime);
            if (isWallRight)
            {
                rBody.AddForce(orientation.right * (wallRunAcceleration / 5) * Time.deltaTime);
            }
            else
            {
                rBody.AddForce(-orientation.right * (wallRunAcceleration / 5) * Time.deltaTime);
            }
        }
    }

    private void StopWallRun()
    {
        rBody.useGravity = true;
        isWallRunning = false;
    }

    private void CheckForWall()
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, 1f, whatIsWall);
        isWallLeft = Physics.Raycast(transform.position, orientation.right * -1, 1f, whatIsWall);

        if (!isWallLeft && !isWallRight)
        {
            StopWallRun();
        }

        if (isWallLeft || isWallRight) doubleJumpsLeft = startDoubleJumps;
    }

    private void Dash()
    {
        //saves current velocity
        dashStartVector = orientation.forward;

        readyToDash = false;
        wTapTimes = 0;

        //Deactivate gravity
        rBody.useGravity = false;

        //Add force
        rBody.velocity = Vector3.zero;
        rBody.AddForce(orientation.forward * dashForce);

        Invoke(nameof(ActivateGravity), dashTime);
    }

    private void ActivateGravity()
    {
        rBody.useGravity = true;

    }

    private void SonicSpeed()
    {
        //If running builds up speed
        if (isGrounded && movementY >= 0.99f)
        {
            timePassedSonic += Time.deltaTime;
        }
        else
        {
            timePassedSonic = 0;
            maxSpeed = startMaxSpeed;
        }

        if (timePassedSonic >= timeBetweenNextSonicBoost)
        {
            if (maxSpeed <= maxSonicSpeed)
            {
                maxSpeed += 5;
                rBody.AddForce(orientation.forward * Time.deltaTime * sonicSpeedForce);
            }
            timePassedSonic = 0;
        }
    }

    private void SideFlash(bool isRight)
    {
        RaycastHit hit;

        //Flash Right
        if (Physics.Raycast(orientation.position, orientation.right, out hit, flashRange) && isRight)
        {
            transform.position = hit.point;
        }
        else if (!Physics.Raycast(orientation.position, orientation.right, out hit, flashRange) && isRight)
            transform.position = new Vector3(transform.position.x + flashRange, transform.position.y, transform.position.z);

        //Flash Left
        if (Physics.Raycast(orientation.position, -orientation.right, out hit, flashRange) && !isRight)
        {
            transform.position = hit.point;
        }
        else if (!Physics.Raycast(orientation.position, -orientation.right, out hit, flashRange) && !isRight)
            transform.position = new Vector3(transform.position.x - flashRange, transform.position.y, transform.position.z);

        //Dampen falldown
        Vector3 vel = rBody.velocity;
        if (rBody.velocity.y < 0.5f && !alreadyStoppedAtLadder)
        {
            rBody.velocity = new Vector3(vel.x, 0, vel.z);
        }

        flashesLeft--;
        if (!alreadySubtractedFlash)
        {
            Invoke(nameof(ResetFlash), flashCooldown);
            alreadySubtractedFlash = true;
        }
    }

    private void ResetFlash()
    {
        alreadySubtractedFlash = false;
        Invoke(nameof(ResetFlash), flashCooldown);

        if (flashesLeft < maxFlashesLeft)
            flashesLeft++;
    }

    private void StartRocketBoost()
    {
        if (!alreadyInvokedRockedStop)
        {
            Invoke(nameof(StopRocketBoost), maxRocketTime);
            alreadyInvokedRockedStop = true;
        }

        rocketTimer += Time.deltaTime;

        rocketActive = true;

        //Boost forwards and upwards
        rBody.AddForce(1f * rocketForce * Time.deltaTime * orientation.forward);
        rBody.AddForce(2f * rocketForce * Time.deltaTime * Vector3.up);

    }

    private void StopRocketBoost()
    {
        alreadyInvokedRockedStop = false;
        rocketActive = false;
        readyToRocket = false;

        if (rocketTimer >= maxRocketTime - 0.2f)
        {
            rBody.AddForce(orientation.forward * rocketForce * -.2f);
            rBody.AddForce(Vector3.up * rocketForce * -.4f);
        }
        else
        {
            rBody.AddForce(orientation.forward * rocketForce * -.2f * rocketTimer);
            rBody.AddForce(Vector3.up * rocketForce * -.4f * rocketTimer);
        }

        rocketTimer = 0;
    }

    private void Climb()
    {
        //Makes possible to climb even when falling down fast
        Vector3 vel = rBody.velocity;
        if (rBody.velocity.y < 0.5f && !alreadyStoppedAtLadder)
        {
            rBody.velocity = new Vector3(vel.x, 0, vel.z);
            //Make sure char get's at wall
            alreadyStoppedAtLadder = true;
            rBody.AddForce(orientation.forward * 500 * Time.deltaTime);
        }

        //Push character up
        if (rBody.velocity.magnitude < maxClimbSpeed)
            rBody.AddForce(orientation.up * climbForce * Time.deltaTime);

        //Doesn't Push into the wall
        if (!Input.GetKey(KeyCode.S)) movementY = 0;
    }

    private void OnCollisionStay(Collision other)
    {
        CheckGround(other);
    }

    private void CheckGround(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        Debug.Log("You ground?");
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;

            if (IsFloor(normal))
            {
                isGrounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        isGrounded = false;
    }

    #region Math
    // Find the velocity relative to where the player is looking
    // Useful for vectors calculations regarding movement and limiting movement
    public Vector2 FindVelRelativeToLook() {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rBody.velocity.x, rBody.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rBody.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }
    #endregion






}
