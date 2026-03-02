using System.Collections;
using TMPro;
using UnityEngine;

public class FrameRateSettings : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] bool debugMode;
    float counter;
    float startTime;
  
    [SerializeField]  int frameRate = 60;

    private void Awake()
    {
        Application.targetFrameRate = frameRate;
        if (text == null) TryGetComponent(out text);
        startTime = Time.time;
    }
    private void Update()
    {
        counter++;

        if(counter>250)
        {
            startTime = Time.time;
            counter = 0;
        }

        if (debugMode)
        {
            if (text == null) return;
            if (Input.GetKeyDown(KeyCode.P))
            {
               text.text = text.text == "" ? "Frame Rate: " + counter / (Time.time - startTime) : "";
            }
            
        }
    }
}
