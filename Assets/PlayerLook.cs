using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform camHolder;
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    private float xRotation;
    private float yRotation;
    private PlayerInput _playerInput;
    private float mouseX;
    private float mouseY;

    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        Look();
    }

    private void Look()
    {
        mouseX = _playerInput.GetAimHorizontal() * Time.fixedDeltaTime * sensX;
        mouseY = _playerInput.GetAimVertical() * Time.fixedDeltaTime * sensY;

        //Find current look rotation
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    } 
    
    public void DoFov(float value)
    {
        GetComponent<Camera>().DOFieldOfView(value, 0.25f);
    }

    public void DoTilt(float value)
    {
        transform.DOLocalRotate(new Vector3(0, 0, value), 0.25f);
    }
}
