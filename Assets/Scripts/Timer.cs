using UnityEngine;


public class Timer : MonoBehaviour
{

    public float CurrentTime;
    public bool TimerOn;

    
    void Start()
    {
        
    }

    
    void Update()
    {
        if (TimerOn)
        {

            CurrentTime += Time.deltaTime;
        }
    }
}
