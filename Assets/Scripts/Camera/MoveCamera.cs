using UnityEngine;

public class MoveCamera : MonoBehaviour {

    public Transform _target;

    void Update() {
        transform.position = _target.transform.position;
    }
}
