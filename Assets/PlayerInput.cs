using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(PlayerMovement))]
public class PlayerInput : MonoBehaviour
{

    private PlayerMovement movementController;

    private void Awake()
    {
        movementController = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        movementController.SetInputDirectional(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        movementController.SetInputJump(Input.GetKey(KeyCode.Space));
        
        movementController.SetInputKeyCrouch(Input.GetKey(KeyCode.LeftControl));
        
        movementController.SetInputUpCrouch(Input.GetKeyUp(KeyCode.LeftControl));
        
        movementController.SetInputDownCrouch(Input.GetKeyDown(KeyCode.LeftControl));
        
        movementController.SetInputLeft(Input.GetKey(KeyCode.A));
        
        movementController.SetInputRight(Input.GetKey(KeyCode.D));
        
        movementController.SetInputForward(Input.GetKey(KeyCode.W));
    }
}
