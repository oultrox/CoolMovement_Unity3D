using UnityEngine;

public class ForcedIllusionManager : MonoBehaviour
{
    [SerializeField] private int lerpFactor = 100;
    [SerializeField] private Material yellowToon;
    [SerializeField] private Material blueToon;
    [SerializeField] private Material redToon;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform targetForTakenObjects;
    [SerializeField] private float rayMaxRange = 1000f;
    [SerializeField] private int initialObjectsLayerID;

    private GameObject takenObject;
    private RaycastHit hit;
    private Ray ray;
    private float distanceMultiplier;
    private Vector3 scaleMultiplier;
    private LayerMask layerMask = ~(1 << 8);
    private float cameraHeight = 0;
    private float cosine;
    private float positionCalculation;
    private float lastPositionCalculation = 0;
    private Vector3 lastHitPoint = Vector3.zero;
    private Vector3 lastRotation = Vector3.zero;
    private bool isRayTouchingSomething = true;
    private float lastRotationY;
    private Vector3 lastHit = Vector3.zero;
    private Vector3 centerCorrection = Vector3.zero;
    private float takenObjSize = 0;
    private int takenObjSizeIndex = 0;

    void Update()
    {
        GrabCast();
        SetUpGrab();
        CheckGrab();
        CheckUnGrab();
    }

    private void GrabCast()
    {
        ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, (Screen.height / 2) + (Screen.height / 10), 0));
        Debug.DrawRay(ray.origin, ray.direction * 200, Color.yellow);

        if (Physics.Raycast(ray, out hit, rayMaxRange, layerMask))
        {
            if (hit.transform.tag == "Getable")
            {
                //Render selection
            }
            else
            {
                //Render deselection
            }
        }

        isRayTouchingSomething = Physics.Raycast(ray, out hit, rayMaxRange, layerMask);

        if (takenObject != null)
        {

        }
        else
        {
            targetForTakenObjects.position = hit.point;
        }
    }

    private void SetUpGrab()
    {
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) && isRayTouchingSomething)
        {
            if (hit.transform.CompareTag("Getable"))
            {
                takenObject = hit.transform.gameObject;

                distanceMultiplier = Vector3.Distance(mainCamera.transform.position, takenObject.transform.position);
                scaleMultiplier = takenObject.transform.localScale;
                lastRotation = takenObject.transform.rotation.eulerAngles;
                lastRotationY = lastRotation.y - mainCamera.transform.eulerAngles.y;
                takenObject.transform.transform.parent = targetForTakenObjects;

                if (takenObject.GetComponent<Rigidbody>() == null)
                {
                    takenObject.AddComponent<Rigidbody>();
                }
                takenObject.GetComponent<Rigidbody>().isKinematic = true;

                foreach (Collider col in takenObject.GetComponents<Collider>())
                {
                    col.isTrigger = true;
                }

                if (takenObject.GetComponent<MeshRenderer>() != null)
                {
                    takenObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    takenObject.GetComponent<MeshRenderer>().receiveShadows = false;
                }
                takenObject.layer = 8;
                foreach (Transform child in takenObject.GetComponentsInChildren<Transform>())
                {
                    takenObject.GetComponent<Rigidbody>().isKinematic = true;
                    takenObject.GetComponent<Collider>().isTrigger = true;
                    child.gameObject.layer = 8;
                }

                takenObjSize = takenObject.GetComponent<Collider>().bounds.size.y;
                takenObjSizeIndex = 1;
                if (takenObject.GetComponent<Collider>().bounds.size.x > takenObjSize)
                {
                    takenObjSize = takenObject.GetComponent<Collider>().bounds.size.x;
                    takenObjSizeIndex = 0;
                }
                if (takenObject.GetComponent<Collider>().bounds.size.z > takenObjSize)
                {
                    takenObjSize = takenObject.GetComponent<Collider>().bounds.size.z;
                    takenObjSizeIndex = 2;
                }
            }
        }
    }

    private void CheckGrab()
    {
        if (Input.GetKey(KeyCode.E) || Input.GetMouseButton(0))
        {
            if (takenObject != null)
            {
                // recenter the object to the center of the mesh regardless  real pivot point
                if (takenObject.GetComponent<MeshRenderer>() != null)
                {
                    centerCorrection = takenObject.transform.position - takenObject.GetComponent<MeshRenderer>().bounds.center;
                }

                takenObject.transform.SetPositionAndRotation(Vector3.Lerp(takenObject.transform.position, targetForTakenObjects.position + centerCorrection, Time.deltaTime * lerpFactor),
                    Quaternion.Lerp(takenObject.transform.rotation, Quaternion.Euler(new Vector3(0, lastRotationY + mainCamera.transform.eulerAngles.y, 0)), Time.deltaTime * lerpFactor));

                cosine = Vector3.Dot(ray.direction, hit.normal);
                cameraHeight = Mathf.Abs(hit.distance * cosine);

                takenObjSize = takenObject.GetComponent<Collider>().bounds.size[takenObjSizeIndex];

                positionCalculation = (hit.distance * takenObjSize / 2) / (cameraHeight);
                if (positionCalculation < rayMaxRange)
                {
                    lastPositionCalculation = positionCalculation;
                }

                // if the wall is more distant then the raycast max range, increase the size only untill the max range
                if (isRayTouchingSomething)
                {
                    lastHitPoint = hit.point;
                }
                else
                {
                    lastHitPoint = mainCamera.transform.position + ray.direction * rayMaxRange;
                }

                targetForTakenObjects.position = Vector3.Lerp(targetForTakenObjects.position, lastHitPoint
                        - (ray.direction * lastPositionCalculation), Time.deltaTime * lerpFactor);

                takenObject.transform.localScale = scaleMultiplier * (Vector3.Distance(mainCamera.transform.position, takenObject.transform.position) / distanceMultiplier);
            }
        }
    }

    private void CheckUnGrab()
    {
        if (Input.GetKeyUp(KeyCode.E) || Input.GetMouseButtonUp(0))
        {
            if (takenObject != null)
            {
                takenObject.GetComponent<Rigidbody>().isKinematic = false;

                foreach (Collider col in takenObject.GetComponents<Collider>())
                {
                    col.isTrigger = false;
                }

                if (takenObject.GetComponent<MeshRenderer>() != null)
                {
                    takenObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    takenObject.GetComponent<MeshRenderer>().receiveShadows = true;
                }
                takenObject.transform.parent = null;
                takenObject.layer = initialObjectsLayerID;
                foreach (Transform child in takenObject.GetComponentsInChildren<Transform>())
                {
                    takenObject.GetComponent<Rigidbody>().isKinematic = false;
                    takenObject.GetComponent<Collider>().isTrigger = false;
                    child.gameObject.layer = initialObjectsLayerID;
                }
                takenObject = null;
            }
        }
    }
}