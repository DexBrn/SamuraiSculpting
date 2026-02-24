using UnityEngine;
using System.Collections;

public class DebrisCleanup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DestroyAfterTime());   
    }


    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
        Destroy(this);
    }

}
