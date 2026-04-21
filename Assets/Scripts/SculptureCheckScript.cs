using Unity.VisualScripting;
using UnityEngine;

public class SculptureCheckScript : MonoBehaviour
{

    float TargetHitCount = 0;
    GameObject Marble;
    Color GoodColour = Color.darkBlue;
    public float Accuracy = 0;
    public float TotalAccuracy;

    void Start()
    {
        //CheckTarget(LayerMask.GetMask("Xray"));
        Marble = GameObject.Find("Marble");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FullCheck()
    {
        CheckTarget(LayerMask.GetMask("Xray"));
        SecondCheck();
        if (TotalAccuracy != 0)
            TotalAccuracy = (TotalAccuracy + Accuracy) / 2;
        else 
            TotalAccuracy = Accuracy;
    }

    public void SecondCheck()
    {
        if (Marble.GetComponent<BoxCollider>() != null)
            DestroyImmediate(Marble.GetComponent<BoxCollider>());
        if (Marble.GetComponent<MeshCollider>() != null)
            DestroyImmediate(Marble.GetComponent<MeshCollider>());
        Marble.AddComponent<MeshCollider>().convex = true;

        float HitCountGoal = TargetHitCount;
        CheckTarget(LayerMask.GetMask("Water"));


        Accuracy = (Mathf.Min(TargetHitCount, HitCountGoal) / Mathf.Max(TargetHitCount, HitCountGoal)) * 100;
    }


    public void CheckTarget(LayerMask TarLayer)
    {
        TargetHitCount = 0;
        for (int x = 0; x < 20; x++)
            for (int y = 0; y < 20; y++)
            {
                Vector3 StartPos = new Vector3(-0.95f + (x * 0.1f), 3f - (y * 0.14f), Camera.main.transform.position.z);
                RaycastHit hit;
                if (Physics.Raycast(StartPos, Vector3.forward, out hit, Mathf.Infinity, TarLayer))
                {  TargetHitCount++; } //Debug.DrawRay(StartPos, Vector3.forward * hit.distance, GoodColour, 999);
                else
                { }//Debug.DrawRay(StartPos, Vector3.forward, Color.red, 999); }
                
            }
        
        for (int x = 0; x < 20; x++) 
            for (int y = 0; y < 20; y++)
            {
                Vector3 StartPos = new Vector3(-2f , 3f - (y * 0.14f), -6.2f + (x * 0.1f));
                RaycastHit hit;
                if (Physics.Raycast(StartPos, Vector3.right, out hit, Mathf.Infinity, TarLayer))
                {  TargetHitCount++; }
                else
                { }
            }
        for (int x = 0; x < 20; x++) 
            for (int y = 0; y < 20; y++)
            {
                Vector3 StartPos = new Vector3(-0.95f + (x * 0.1f), 5, -4.3f - (y * 0.1f));
                RaycastHit hit;
                if (Physics.Raycast(StartPos, Vector3.down, out hit, Mathf.Infinity, TarLayer))
                {  TargetHitCount++; }
                else
                {  }
            }
        
        GoodColour = Color.green;
    }






}
