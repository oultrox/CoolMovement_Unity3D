using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Float Event")]
public class FloatEvent : ScriptableObject
{
    private List<FloatEventListener> _listeners;


    public void Raise(float number)
    {
        for (int i = 0; i < _listeners.Count; i++)
        {
            _listeners[i].OnRaiseEvent(number);
        }
    }

    public void RegisterListener(FloatEventListener listener)
    {
        _listeners.Add(listener);
    }
    
    public void UnRegisterListener(FloatEventListener listener)
    {
        _listeners.Remove(listener);
    }
}
