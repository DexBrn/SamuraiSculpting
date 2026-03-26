using Unity.VisualScripting;
using UnityEngine;

public class SculptureCheckScript : MonoBehaviour
{

    float TargetHitCount = 0;



    void Start()
    {
        CheckTarget(GameObject.Find("Marble"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void CheckTarget(GameObject Target)
    {
        for (int x = 0; x < 10; x++) 
            for (int y = 0; y < 10; y++)
            {
                //Vector3 StartPos = new Vector3(x * 0.2f - 0.75f, y * 0.33f, Camera.main.transform.position.z);

                Vector3 Offset = Target.transform.right * (Target.transform.localScale.x / 2f) * -1f;
                


                Vector3 StartPos = new Vector3(-0.97f + (x * 0.2f), Target.transform.position.y + Offset.y + (y * 0.33f), Camera.main.transform.position.z);
                RaycastHit hit;
                if (Physics.Raycast(StartPos, Vector3.forward, out hit, Mathf.Infinity))
                { print(hit.point); Debug.DrawRay(StartPos, Vector3.forward * hit.distance, Color.darkBlue, 999); }
                else
                { print("hi"); Debug.DrawRay(StartPos, Vector3.forward, Color.red, 999); }
            }
    }






}
