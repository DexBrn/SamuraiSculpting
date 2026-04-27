using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{

    public float CurrentTime;
    public bool TimerOn;

    public TMP_Text TimerText;
    
    void Start()
    {
        
    }

    
    void Update()
    {
        if (TimerOn)
        {

            CurrentTime += Time.deltaTime;
        }

        TimeSpan TimeSpan = TimeSpan.FromSeconds(CurrentTime);
        if (TimeSpan.Minutes > 0)
            TimerText.text = $"{TimeSpan.Minutes:D2}:{TimeSpan.Seconds:D2}:{TimeSpan.Milliseconds / 10:D2}";
        else
            TimerText.text = $"{TimeSpan.Seconds:D2}:{TimeSpan.Milliseconds / 10:D2}";

    }
}
