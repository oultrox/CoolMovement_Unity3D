using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventListener : MonoBehaviour
{
    [SerializeField] private FloatEventChannelSO _getPlayerSpeed;
    private TMP_Text _text;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }
    private void Start()
    {
        _getPlayerSpeed.OnEventRaised += SetGUISpeed;
    }

    private void SetGUISpeed(float value)
    {
        _text.text = "Speed: " + value;
    }
}
