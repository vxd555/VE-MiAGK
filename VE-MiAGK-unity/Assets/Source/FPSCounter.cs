using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public Text fpsCounter = null;
    public float timer, refresh, avgFramerate, timelapse;

    void Update()
    {
        timelapse = Time.smoothDeltaTime;
        timer = timer <= 0 ? refresh : timer -= timelapse;

        if(timer <= 0) avgFramerate = (int)(1f / timelapse);

        fpsCounter.text = $"{avgFramerate} FPS";
    }
}
