using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FloatEventListener : MonoBehaviour
{
    [SerializeField] private FloatEvent _event;
    public Action<float> _response;

    private void OnEnable()
    {
        _event.RegisterListener(this);
    }
    
    private void OnDisable()
    {
        _event.UnRegisterListener(this);
    }

    public void OnRaiseEvent(float data)
    {
        _response?.Invoke(data);
    }
}
