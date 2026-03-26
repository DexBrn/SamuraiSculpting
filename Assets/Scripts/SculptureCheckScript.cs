using Unity.VisualScripting;
using UnityEngine;

public class SculptureCheckScript : MonoBehaviour
{

    float TargetHitCount = 0;
    GameObject Marble;


    void Start()
    {
        CheckTarget(LayerMask.GetMask("Xray"));
        Marble = GameObject.Find("Marble");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            DestroyImmediate(Marble.GetComponent<BoxCollider>());
            Marble.AddComponent<MeshCollider>().convex = true;

            float HitCountGoal = TargetHitCount;
            CheckTarget(LayerMask.GetMask("Default"));

            print($"Goal: {HitCountGoal} :: Attempt: {TargetHitCount}");
        }
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
                { print(hit.point);  Debug.DrawRay(StartPos, Vector3.forward * hit.distance, Color.darkBlue, 999); TargetHitCount++; }
                else
                { print("hi"); Debug.DrawRay(StartPos, Vector3.forward, Color.red, 999); }
                
            }
        /*
        for (int x = 0; x < 20; x++) 
            for (int y = 0; y < 20; y++)
            {
                Vector3 StartPos = new Vector3(-2f , 3f - (y * 0.14f), -6.2f + (x * 0.1f));
                RaycastHit hit;
                if (Physics.Raycast(StartPos, Vector3.right, out hit, Mathf.Infinity, TarLayer))
                { print(hit.point); Debug.DrawRay(StartPos, Vector3.right * hit.distance, Color.darkBlue, 999); TargetHitCount++; }
                else
                { print("hi"); Debug.DrawRay(StartPos, Vector3.right, Color.red, 999); }
            }
        for (int x = 0; x < 20; x++) 
            for (int y = 0; y < 20; y++)
            {
                Vector3 StartPos = new Vector3(-0.95f + (x * 0.1f), 5, -4.3f - (y * 0.1f));
                RaycastHit hit;
                if (Physics.Raycast(StartPos, Vector3.down, out hit, Mathf.Infinity, TarLayer))
                { print(hit.point); Debug.DrawRay(StartPos, Vector3.down * hit.distance, Color.darkBlue, 999); TargetHitCount++; }
                else
                { print("hi"); Debug.DrawRay(StartPos, Vector3.down, Color.red, 999); }
            }
        */
        print(TargetHitCount);
    }






}
