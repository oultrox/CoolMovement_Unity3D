using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GUITextEventSetter : MonoBehaviour
{
    [SerializeField] private FloatEventListener _speedPlayerListener;
    private TMP_Text _text;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        _speedPlayerListener._response += SetGUISpeed;
    }

    private void SetGUISpeed(float value)
    {
        _text.text = "Speed: " + value;
    }
}
