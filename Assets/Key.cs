using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

public class Key : MonoBehaviour, IMixedRealityTouchHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTouchStarted(HandTrackingInputEventData eventData)
    {
        Keyboard keyboard = gameObject.GetComponentInParent<Keyboard>();
        keyboard.OnKeyTouchStarted(gameObject, eventData);
    }

    public void OnTouchCompleted(HandTrackingInputEventData eventData) {
        Keyboard keyboard = gameObject.GetComponentInParent<Keyboard>();
        keyboard.OnKeyTouchCompleted(gameObject, eventData);
    }

    public void OnTouchUpdated(HandTrackingInputEventData eventData) {
        Keyboard keyboard = gameObject.GetComponentInParent<Keyboard>();
        keyboard.OnKeyTouchUpdated(gameObject, eventData);
    }
}
