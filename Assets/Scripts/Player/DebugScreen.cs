using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugScreen : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;

    private float frameRate;
    private float timer;
    
    private void Start() {
        
    }

    private void Update() {
        DebugScreenUpdates();
    }

    private void DebugScreenUpdates() {
        string debugText;
        
        debugText = (
            "0.30"
        );

        text.text = debugText;


        if(timer > 1.0f) {
            frameRate = (int)(1.0f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else {
            timer += Time.deltaTime;
        }
    }
}
