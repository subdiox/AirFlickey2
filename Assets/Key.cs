using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

public class Key : MonoBehaviour, IMixedRealityTouchHandler, IMixedRealityPointerHandler
{
    bool isKey;
    Keyboard keyboard;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"gameObject.name: {gameObject.name}");
        if (gameObject.name == "1" || gameObject.name == "2" || gameObject.name == "3") {
            isKey = false;
            Playspace playspace = gameObject.GetComponentInParent<Playspace>();
            Debug.Log($"playspace: {playspace}");
            keyboard = playspace.GetComponentInChildren<Keyboard>();
        } else {
            isKey = true;
            keyboard = gameObject.GetComponentInParent<Keyboard>();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTouchStarted(HandTrackingInputEventData eventData) {
        if (isKey && !keyboard.grabMode) {
            keyboard.OnKeyTouchStarted(gameObject, eventData);
        }
    }

    public void OnTouchCompleted(HandTrackingInputEventData eventData) {
        Debug.Log($"isKey: {isKey}");
        if (isKey && !keyboard.grabMode) {
            keyboard.OnKeyTouchCompleted(gameObject, eventData);
        } else if (!isKey) {
            Debug.Log($"OnModeSelected: {keyboard}, {gameObject}");
            keyboard.OnModeSelected(gameObject);
        }
    }

    public void OnTouchUpdated(HandTrackingInputEventData eventData) {
        if (isKey && !keyboard.grabMode) {
            keyboard.OnKeyTouchUpdated(gameObject, eventData);
        }
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData) {
        if (isKey && keyboard.grabMode) {
            keyboard.OnKeyPointerDown(gameObject, eventData);
        }
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) {
        if (isKey && keyboard.grabMode) {
            keyboard.OnKeyPointerClicked(gameObject, eventData);
        }
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData) {
        if (isKey && keyboard.grabMode) {
            keyboard.OnKeyPointerDragged(gameObject, eventData);
        }
    }
    
    public void OnPointerUp(MixedRealityPointerEventData eventData) {
        if (isKey && keyboard.grabMode) {
            keyboard.OnKeyPointerUp(gameObject, eventData);
        }
    }
}
