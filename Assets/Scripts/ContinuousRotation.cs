using UnityEngine;

public class ContinuousRotation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gameObject.layer == LayerMask.GetMask("Xray"))
            gameObject.layer = LayerMask.GetMask("Default");
        else if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
            if (transform.GetChild(0).childCount > 0)
                transform.GetChild(0).GetChild(0).gameObject.layer = LayerMask.GetMask("Default");
                
        }
            
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0, 1, 0);
    }
}
