using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(Rigidbody))]
public abstract class PlayerComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Transform orientation;
    protected PlayerInput playerInput;
    protected Rigidbody rBody;
    protected PlayerMovementController playerController;

    #region Properties
    public Rigidbody RBody { get => rBody; set => rBody = value; }
    public PlayerMovementController PlayerController { get => playerController; set => playerController = value; }
    public Transform Orientation { get => orientation; set => orientation = value; }
    protected PlayerInput PlayerInput { get => playerInput; set => playerInput = value; }
    #endregion


    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rBody = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerMovementController>();
    }

}
