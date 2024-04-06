using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS_Check : MonoBehaviour
{
    [Range(1,100)]
    public int fFont_Size = 50;

    [Range(0,1)]
    public float Red, Green, Blue;

    float deltaTime = 0.1f;

    private void Awake()
    {
        AndroidFrameRate();
    }

    private void Start()
    {
        fFont_Size = fFont_Size == 0 ? 50 : fFont_Size;
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        ApplicationQuit();
    }

    /// <summary> 어플리케이션 종료 </summary>
    private void ApplicationQuit()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    /// <summary> 안드로이드 프레임 고정 </summary>
    private void AndroidFrameRate()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("Android => 60Frame");
            Application.targetFrameRate = 60;//60프레임고정
        }
    }

    private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / fFont_Size;
        style.normal.textColor = new Color(Red, Green, Blue, 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps", msec, (int)fps);
        GUI.Label(rect, text, style);
    }
}
