using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerInput : MonoBehaviour
{

    private Vector2 _movementDirection;
    private float _verticalInput;
    private float _horizontalInput; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    internal bool GetInputForward()
    {
        return Input.GetKey(KeyCode.W);
    }

    internal bool GetInputRight()
    {
        return Input.GetKey(KeyCode.D);
    }

    internal bool GetInputLeft()
    {
        return (Input.GetKey(KeyCode.A));
    }

    internal bool GetInputUpCrouch()
    {
        return (Input.GetKeyUp(KeyCode.LeftControl));
    }

    internal bool GetInputCrouch()
    {
        return (Input.GetKey(KeyCode.LeftControl));
    }

    internal bool GetInputDownCrouch()
    {
        return (Input.GetKeyDown(KeyCode.LeftControl));
    }

    internal bool GetInputLeftShift()
    {
        return (Input.GetKey(KeyCode.LeftShift));
    }

    internal bool GetInputJump()
    {
        return (Input.GetKey(KeyCode.Space));
    }

    internal float GetHorizontalInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        return _horizontalInput;
    }

    internal float GetVerticalInput()
    {
        _verticalInput = Input.GetAxisRaw("Vertical");
        return _verticalInput;
    }

    internal float GetAimHorizontal()
    {
        return Input.GetAxis("Mouse X");
    }

    internal float GetAimVertical()
    {
        return Input.GetAxis("Mouse Y");
    }


}
