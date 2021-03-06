using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.Networking;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.Utilities;

using TMPro;
using Newtonsoft.Json.Linq; 

using System.IO;  
using System.Linq;  
using System.Net;  
using System.Text;  
using System.Web;  
public class Keyboard : MonoBehaviour
{
    GameObject keyPressed = null;
    Vector3 keyPosition;
    GameObject clonedKey = null;
    int lastDirection = 0;
    string confirmedText = "";
    string unconfirmedText = "";
    bool cursorDisplayed = false;
    string[] candidates = {};
    int candidateIndex = -1;
    public TextMeshPro textField;
    public TextMeshPro candidatesField;
    public GameObject dummy;
    public GameObject buttonCollection;
    public bool grabMode;

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

    void SetKeyName(GameObject key, string name) {
        key.GetComponentInChildren<TextMeshPro>().text = name;
    }

    GameObject GetKey(string name) {
        return buttonCollection.transform.Find(name).gameObject;
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

    char? CutOutText(ref string text) {
        if (text.Length > 0) {
            char lastChar = text[text.Length - 1];
            text = text.Remove(text.Length - 1);
            return lastChar;
        }
        return null;
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

    IEnumerator GetCandidates() {
        string escapedText = UnityWebRequest.EscapeURL(unconfirmedText);
        UnityWebRequest req = UnityWebRequest.Get($"http://www.google.com/transliterate?langpair=ja-Hira|ja&text={escapedText}");
        yield return req.SendWebRequest();

        if (req.isNetworkError || req.isHttpError) {
            Debug.Log(req.error);
        } else if (req.responseCode == 200) {
            Debug.Log(req.downloadHandler.text);
            JArray jar = JArray.Parse(req.downloadHandler.text);
            foreach (JToken jt in jar) {  
                candidates = jt[1].Values<string>().ToArray();
                candidateIndex = -1;
                break;
            }
        }
    }

    /*
       現在のフリック方向(direction)を取得する関数
       [direction]
        0 : 現在位置がキーをタップした位置から0.02未満しかずれていないとき
        1 : 現在位置がキーをタップした位置から左方向にずれたと判定されるとき
        2 : 現在位置がキーをタップした位置から上方向にずれたと判定されるとき
        3 : 現在位置がキーをタップした位置から右方向にずれたと判定されるとき
        4 : 現在位置がキーをタップした位置から下方向にずれたと判定されるとき
    */
    int GetDirection(Vector3 position) {
        Vector3 relativePosition = position - keyPosition;
        Vector2 relativePosition2 = relativePosition;
        float angle = Vector2.Angle(Vector2.right, relativePosition2);
        if (relativePosition2.magnitude < 0.02) { // フリックしなかったと判定する基準
            return 0;
        } else if (angle < 45) {
            return 3;
        } else if (angle > 135) {
            return 1;
        } else if (relativePosition2.y > 0) {
            return 2;
        } else {
            return 4;
        }
    }

    void KeyStart(GameObject key, Vector3 position) {
        if (keyPressed == null) {   
            keyPressed = key;
            keyPosition = position;
        }
    }

    void KeyUpdate(Vector3 position) {
        int direction = GetDirection(position);
        
        Debug.Log($"direction: {direction}");
        string inputString = hiragana[keyPressed.name][direction].ToString();
        if (lastDirection != direction) {
            if (clonedKey) {
                Destroy(clonedKey);
            }
            clonedKey = CloneKey(direction);
            SetKeyName(clonedKey, inputString);
            lastDirection = direction;
        }
        Vector3 relativePosition = position - keyPosition;
        if (!grabMode && relativePosition.z < -0.02) { // キーから手を離したと判定する基準
            KeyEnd(position);
        }
    }

    void KeyEnd(Vector3 position) {
        if (hiragana.ContainsKey(keyPressed.name)) {
            int direction = GetDirection(position);
            string inputString = hiragana[keyPressed.name][direction].ToString();
            unconfirmedText += inputString;
            StartCoroutine("GetCandidates");
            keyPressed = null;
            if (clonedKey) {
                Destroy(clonedKey);
                clonedKey = null;
            }
            lastDirection = 0;
        } else {
            if (keyPressed.name == "Space") {
                if (unconfirmedText.Length > 0) {
                    if (candidates.Length - 1 == candidateIndex) {
                        candidateIndex = 0;
                    } else {
                        candidateIndex += 1;
                    }
                } else {
                    confirmedText += "　";
                }
            } else if (keyPressed.name == "Return") {
                if (unconfirmedText.Length > 0) {
                    if (candidateIndex > 0 && candidateIndex < candidates.Length) {
                        confirmedText += candidates[candidateIndex];
                    } else {
                        confirmedText += unconfirmedText;
                    }
                    candidateIndex = -1;
                    candidates = new string[]{};
                    unconfirmedText = "";
                } else {
                    confirmedText += "\n";
                }
            } else if (keyPressed.name == "Delete") {
                if (unconfirmedText.Length > 1) {
                    CutOutText(ref unconfirmedText);
                    StartCoroutine("GetCandidates");
                } else if (unconfirmedText.Length == 1) {
                    CutOutText(ref unconfirmedText);
                    candidateIndex = -1;
                    candidates = new string[]{};
                } else {
                    CutOutText(ref confirmedText);
                    candidateIndex = -1;
                    candidates = new string[]{};
                }
            } else if (keyPressed.name == "Special") {
                char? lastChar = CutOutText(ref unconfirmedText);
                if (lastChar.HasValue) {
                    unconfirmedText += ConvertChar(lastChar.Value);
                    StartCoroutine("GetCandidates");
                }
            }
            keyPressed = null;
        }
    }

    /* --- Callbacks --- */
    // Start is called before the first frame update
    void Start() {
        InvokeRepeating("UpdateCursor", 0.5f, 0.5f);
    }

    void UpdateInput() {
        foreach(var source in CoreServices.InputSystem.DetectedInputSources) {
            // Ignore anything that is not a hand because we want articulated hands
            if (source.SourceType == InputSourceType.Hand) {
                foreach (var p in source.Pointers) {
                    if (p is PokePointer) {
                        KeyUpdate(p.Position);
                        return;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {
        if (!grabMode && keyPressed && hiragana.ContainsKey(keyPressed.name)) {
            UpdateInput();
        }
        if (cursorDisplayed) {
            textField.text = $"{confirmedText}<u>{unconfirmedText}</u>|";
        } else {
            textField.text = $"{confirmedText}<u>{unconfirmedText}</u>";
        }
        if (unconfirmedText.Length > 0) {
            SetKeyName(GetKey("Space"), "変換");
            SetKeyName(GetKey("Return"), "確定");
        } else {
            SetKeyName(GetKey("Space"), "空白");
            SetKeyName(GetKey("Return"), "改行");
        }
        if (candidateIndex < 0 || candidates.Length <= candidateIndex) {
            candidatesField.text = string.Join(" ", candidates);
        } else {
            var beforeCandidate = new ArraySegment<string>(candidates, 0, candidateIndex);
            var candidate = candidates[candidateIndex];
            var afterCandidate = new ArraySegment<string>(candidates, candidateIndex + 1, candidates.Length - candidateIndex - 1);
            candidatesField.text = string.Join(" ", beforeCandidate) + $" <mark=#22f300aa>{candidate}</mark> " + string.Join(" ", afterCandidate);
        }
    }

    public void OnModeSelected(GameObject key) {
        SolverHandler solverHandler = gameObject.GetComponent<SolverHandler>();
        RadialView radialView = gameObject.GetComponent<RadialView>();
        Debug.Log($"keyName: {key.name}");
        if (key.name == "1") {
            Debug.Log($"keyName: 1, solverHandler: {solverHandler}");
            solverHandler.TrackedHandness = Handedness.None;
            solverHandler.TrackedHandJoint = TrackedHandJoint.None;
            solverHandler.TrackedTargetType = TrackedObjectType.Head;
            solverHandler.RegisterSolver(radialView);
            grabMode = false;
        } else if (key.name == "2") {
            Debug.Log($"keyName: 2, solverHandler: {solverHandler}");
            solverHandler.TrackedHandness = Handedness.Left;
            solverHandler.TrackedHandJoint = TrackedHandJoint.Palm;
            solverHandler.TrackedTargetType = TrackedObjectType.HandJoint;
            solverHandler.UnregisterSolver(radialView);
            grabMode = false;
        } else if (key.name == "3") {
            Debug.Log($"keyName: 3, solverHandler: {solverHandler}");
            solverHandler.TrackedHandness = Handedness.None;
            solverHandler.TrackedHandJoint = TrackedHandJoint.None;
            solverHandler.TrackedTargetType = TrackedObjectType.Head;
            solverHandler.RegisterSolver(radialView);
            grabMode = true;
        }
        solverHandler.RefreshTrackedObject();
    }

    public void OnKeyTouchStarted(GameObject key, HandTrackingInputEventData eventData) {
        KeyStart(key, eventData.InputData);
    }

    public void OnKeyTouchCompleted(GameObject key, HandTrackingInputEventData eventData) {
        if (!hiragana.ContainsKey(keyPressed.name)) {
            KeyEnd(eventData.InputData);
        }
    }

    public void OnKeyTouchUpdated(GameObject key, HandTrackingInputEventData eventData) {
    }

    public void OnKeyPointerDown(GameObject key, MixedRealityPointerEventData eventData) {
        KeyStart(key, eventData.Pointer.Position);
    }

    public void OnKeyPointerClicked(GameObject key, MixedRealityPointerEventData eventData) {}
    public void OnKeyPointerDragged(GameObject key, MixedRealityPointerEventData eventData) {
        KeyUpdate(eventData.Pointer.Position);
    }
    public void OnKeyPointerUp(GameObject key, MixedRealityPointerEventData eventData) {
        KeyEnd(eventData.Pointer.Position);
    }
}
