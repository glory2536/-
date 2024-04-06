using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class Timer
{
    public float leftTime;

    public IEnumerator TimerCo(float timeLeft, System.Action<bool> callback)
    {
        leftTime = timeLeft;

        while (true)
        {
            //Debug.Log(timeLeft);
            if (leftTime > 0)
            {
                Debug.Log((int)leftTime);
                leftTime -= Time.deltaTime;
            }
            else
            {
                Debug.Log("Timer => 0");
                leftTime = 0;
                callback(true);
                break;
            }
            yield return null;
        }
    }


    void UpdateTimerUI(float currentTime)
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        //text.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }
}