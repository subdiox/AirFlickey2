using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using TMPro;

public class Keyboard : MonoBehaviour
{
    GameObject keyPressed = null;
    GameObject clonedKey = null;
    Vector3 keyPosition = new Vector3();
    public TextMeshPro textField;

    Dictionary<string, string> hiragana = new Dictionary<string, string>() {
        {"A", "あいうえお"},
        {"Ka", "かきくけこ"},
        {"Sa", "さしすせそ"},
        {"Ta", "たちつてと"},
        {"Na", "なにぬねの"},
        {"Ha", "はひふへほ"},
        {"Ma", "まみむめも"},
        {"Ya", "や「ゆ」よ"},
        {"Ra", "らりるれろ"},
        {"Wa", "わをんー～"},
        {"Mark", "、。？！…"}
    };

    

    /*
       現在のフリック方向(direction)を取得する関数
       [direction]
        0 : 現在位置がキーをタップした位置から0.02未満しかずれていないとき
        1 : 現在位置がキーをタップした位置から左方向にずれたと判定されるとき
        2 : 現在位置がキーをタップした位置から上方向にずれたと判定されるとき
        3 : 現在位置がキーをタップした位置から右方向にずれたと判定されるとき
        4 : 現在位置がキーをタップした位置から下方向にずれたと判定されるとき
    */
    int GetCurrentDirection() {
        int direction = -1;
        foreach(var source in CoreServices.InputSystem.DetectedInputSources)
        {
            // Ignore anything that is not a hand because we want articulated hands
            if (source.SourceType == InputSourceType.Hand)
            {
                foreach (var p in source.Pointers)
                {
                    if (p is PokePointer) {
                        Vector3 position = p.Position;
                        if (keyPressed != null) {
                            Vector3 relativePosition = position - keyPosition;
                            Vector2 relativePosition2 = relativePosition;
                            float angle = Vector2.Angle(Vector2.right, relativePosition2);
                            if (relativePosition2.magnitude < 0.02) { // フリックしなかったと判定する基準
                                direction = 0;
                            } else if (angle < 45) {
                                direction = 3;
                            } else if (angle > 135) {
                                direction = 1;
                            } else if (relativePosition2.y > 0) {
                                direction = 2;
                            } else {
                                direction = 4;
                            }
                            if (clonedKey == null) {
                                clonedKey = CloneKey(direction);
                            }
                            if (relativePosition.z < -0.02) { // キーから手を離したと判定する基準
                                // INPUT
                                if (keyPressed && hiragana.ContainsKey(keyPressed.name)) {
                                    textField.text += hiragana[keyPressed.name][direction];
                                    keyPressed = null;
                                }
                                if (clonedKey) {
                                    Destroy(clonedKey);
                                    clonedKey = null;
                                }
                                keyPosition = new Vector3();
                            }
                        }
                        break;
                    }
                }
                break;
            }
        }
        return direction;
    }

    public GameObject CloneKey(int direction) {
        Vector3 relative = new Vector3();
        if (direction == 1) {
            relative = new Vector3(-0.032f, 0, -0.002f);
        } else if (direction == 2) {
            relative = new Vector3(0, 0.032f, -0.002f);
        } else if (direction == 3) {
            relative = new Vector3(0.032f, 0, -0.002f);
        } else if (direction == 4) {
            relative = new Vector3(0, -0.032f, -0.002f);
        } else {
            return null;
        }
        var clone = GameObject.Instantiate(keyPressed) as GameObject;
        if (hiragana.ContainsKey(keyPressed.name)) {
            var tmp = clone.GetComponentInChildren<TextMeshPro>();
            tmp.text = hiragana[keyPressed.name][direction].ToString();
        }
        clone.transform.parent = keyPressed.transform.parent;
        clone.transform.localPosition = keyPressed.transform.localPosition + relative;
        clone.transform.localScale = keyPressed.transform.localScale;
        clone.transform.rotation = keyPressed.transform.rotation;
        return clone;
    }

    /* --- Callbacks --- */
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetCurrentDirection();
    }

    public void OnKeyTouchStarted(GameObject key, HandTrackingInputEventData eventData)
    {
        if (keyPressed == null) {
            keyPressed = key;
            keyPosition = eventData.InputData;
        }
    }

    public void OnKeyTouchCompleted(GameObject key, HandTrackingInputEventData eventData) {
    }

    public void OnKeyTouchUpdated(GameObject key, HandTrackingInputEventData eventData) {
    }
}
