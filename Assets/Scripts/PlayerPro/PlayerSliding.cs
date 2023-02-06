using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSliding : PlayerComponent
{
    [SerializeField] private Transform playerTransform;
    
    [Header("Sliding")]
    [SerializeField] private float maxSlideTime = 0.75f;
    [SerializeField] private float slideForce = 200;
    [SerializeField] private float slideYScale = 0.5f;

    private bool isSliding;
    private float slideTimer;
    private float startYScale;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 playerScale;
    private Vector3 inputDirection;

    private void Start()
    {
        startYScale = playerTransform.localScale.y;
    }

    private void Update()
    {
        horizontalInput = playerInput.GetHorizontalInput();
        verticalInput = playerInput.GetVerticalInput();

        if (playerInput.GetInputDownCrouch() && (horizontalInput != 0 || verticalInput != 0))
            StartSlide();

        if (playerInput.GetInputUpCrouch() && isSliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (isSliding)
            SlideMovement();
    }

    private void StartSlide()
    {
        isSliding = true;
        playerScale = playerTransform.localScale;
        playerScale.y = slideYScale;
        playerTransform.localScale = playerScale;
        rBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        slideTimer = maxSlideTime;
    }

    private void StopSlide()
    {
        isSliding = false;
        playerScale = playerTransform.localScale;
        playerScale.y = startYScale;
        playerTransform.localScale = playerScale;
    }

    private void SlideMovement()
    {
        inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rBody.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
        slideTimer -= Time.deltaTime;

        if (slideTimer <= 0) StopSlide();

    }
}
