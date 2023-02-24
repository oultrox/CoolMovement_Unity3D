using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Channels/Int Event Channel So")]
public class FloatEventChannelSO : ScriptableObject
{
    public UnityAction<float> OnEventRaised;

    public void RaiseEvent(float value)
    {
        if (OnEventRaised == null)
        {
            return;
        }
        OnEventRaised.Invoke(value);
    }

}
