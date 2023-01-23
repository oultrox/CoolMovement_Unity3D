using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GrapplingGun : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Transform gunTip;
    [SerializeField] private int maxDistance = 100;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask grappleLayer;


    [Header("Hook settings")]
    [SerializeField] private float damper = 7;
    [SerializeField] private float spring = 4;
    [SerializeField] private float massScale = 4.5f;

    private LineRenderer lineRenderer;
    private Vector3 grapplePoint;
    private Transform playerTransform;
    private SpringJoint joint;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartGrapple();
        }
        else if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            FinishGrapple();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxDistance, grappleLayer))
        {
            grapplePoint = hit.point;
            joint = playerTransform.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(playerTransform.position, grapplePoint);
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;
            lineRenderer.positionCount = 2;

        }
    }

    private void FinishGrapple()
    {
        lineRenderer.positionCount = 0;
        Destroy(joint);
    }

    private void DrawRope()
    {
        if (!joint) return;

        lineRenderer.SetPosition(0, gunTip.position);
        lineRenderer.SetPosition(1, grapplePoint);

    }
}
