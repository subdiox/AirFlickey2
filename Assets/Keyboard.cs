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
    int lastDirection = 0;
    string confirmedText = "";
    string unconfirmedText = "";
    bool cursorDisplayed = false;
    public TextMeshPro textField;
    public GameObject dummy;

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
    void GetCurrentDirection() {
        if (keyPressed && hiragana.ContainsKey(keyPressed.name)) {
            foreach(var source in CoreServices.InputSystem.DetectedInputSources)
            {
                // Ignore anything that is not a hand because we want articulated hands
                if (source.SourceType == InputSourceType.Hand)
                {
                    foreach (var p in source.Pointers)
                    {
                        if (p is PokePointer) {
                            int direction;
                            Vector3 position = p.Position;
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
                            string inputString = hiragana[keyPressed.name][direction].ToString();
                            Debug.Log($"hiragana: {keyPressed.name}, {direction}, {hiragana[keyPressed.name][direction]}");
                            if (lastDirection != direction) {
                                if (clonedKey) {
                                    Destroy(clonedKey);
                                }
                                clonedKey = CloneKey(direction);
                                SetKeyName(clonedKey, inputString);
                                lastDirection = direction;
                            }
                            if (relativePosition.z < -0.02) { // キーから手を離したと判定する基準
                                unconfirmedText += inputString;
                                keyPressed = null;
                                if (clonedKey) {
                                    Destroy(clonedKey);
                                    clonedKey = null;
                                }
                                lastDirection = 0;
                                keyPosition = new Vector3();
                            }
                            return;
                        }
                    }
                }
            }
        }
    }

    void SetKeyName(GameObject key, string name) {
        key.GetComponentInChildren<TextMeshPro>().text = name;
    }

    GameObject CloneKey(int direction) {
        Vector3 relative = new Vector3();
        if (direction == 1) {
            relative = new Vector3(-0.032f, 0, -0.005f);
        } else if (direction == 2) {
            relative = new Vector3(0, 0.032f, -0.005f);
        } else if (direction == 3) {
            relative = new Vector3(0.032f, 0, -0.005f);
        } else if (direction == 4) {
            relative = new Vector3(0, -0.032f, -0.005f);
        } else {
            return null;
        }
        var clone = GameObject.Instantiate(dummy) as GameObject;
        clone.transform.parent = keyPressed.transform.parent;
        clone.transform.localPosition = keyPressed.transform.localPosition + relative;
        clone.transform.localScale = keyPressed.transform.localScale;
        clone.transform.rotation = keyPressed.transform.rotation; 
        clone.SetActive(true);
        return clone;
    }

    void UpdateCursor() {
        cursorDisplayed = !cursorDisplayed;
    }

    char CutOutText(ref string text) {
        char lastChar = text[text.Length - 1];
        text = text.Remove(text.Length - 1);
        return lastChar;
    }

    char ConvertChar(char c) {
        string[] dict = new string[] {
            "あぁ",
            "いぃ",
            "うぅゔ",
            "えぇ",
            "おぉ",
            "かが",
            "きぎ",
            "くぐ",
            "けげ",
            "こご",
            "さざ",
            "しじ",
            "すず",
            "せぜ",
            "そぞ",
            "ただ",
            "ちぢ",
            "つっづ",
            "てで",
            "とど",
            "はばぱ",
            "ひびぴ",
            "ふぶぷ",
            "へべぺ",
            "ほぼぽ",
            "やゃ",
            "ゆゅ",
            "よょ",
            "わゎ"
        };
        foreach (string text in dict) {
            for (int i = 0; i < text.Length; i ++) {
                if (c == text[i]) {
                    if (i == text.Length - 1) {
                        return text[0];
                    } else {
                        return text[i + 1];
                    }
                }
            }
        }
        return c;
    }

    /* --- Callbacks --- */
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateCursor", 0.5f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        GetCurrentDirection();
        if (cursorDisplayed) {
            textField.text = $"{confirmedText}<u>{unconfirmedText}</u>|";
        } else {
            textField.text = $"{confirmedText}<u>{unconfirmedText}</u>";
        }
    }

    public void OnKeyTouchStarted(GameObject key, HandTrackingInputEventData eventData)
    {
        if (keyPressed == null) {
            keyPressed = key;
            keyPosition = eventData.InputData;
        }
    }

    public void OnKeyTouchCompleted(GameObject key, HandTrackingInputEventData eventData) {
        if (!hiragana.ContainsKey(keyPressed.name)) {
            if (keyPressed.name == "Space") {
                if (unconfirmedText.Length > 0) {
                    // 変換処理
                } else {
                    confirmedText += "　";
                }
            } else if (keyPressed.name == "Return") {
                if (unconfirmedText.Length > 0) {
                    confirmedText += unconfirmedText;
                    unconfirmedText = "";
                } else {
                    confirmedText += "\n";
                }
            } else if (keyPressed.name == "Delete") {
                if (unconfirmedText.Length > 0) {
                    CutOutText(ref unconfirmedText);
                } else {
                    CutOutText(ref confirmedText);
                }
            } else if (keyPressed.name == "Special") {
                if (unconfirmedText.Length > 0) {
                    char lastChar = CutOutText(ref unconfirmedText);
                    unconfirmedText += ConvertChar(lastChar);
                }
            }
            keyPressed = null;
            keyPosition = new Vector3();
        }
    }

    public void OnKeyTouchUpdated(GameObject key, HandTrackingInputEventData eventData) {
    }
}
