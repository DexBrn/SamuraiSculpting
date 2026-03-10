using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UIElements;
using EzySlice;

public class TantoCutCross : MonoBehaviour
{
    public GameObject SubtractionCylinder;

    TextureRegion Region;

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        print("Collided");
        if (other.gameObject.name == "TantoCut")
        {
            GameObject NewCylinder = Instantiate(SubtractionCylinder);
            

        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        print("YOOO");
        if (collision.gameObject.name == "TantoCut")
        {
            if (!GameObject.Find("SubtractionCylinder"))
            {
                GameObject NewCylinder = Instantiate(SubtractionCylinder);
                NewCylinder.transform.position = collision.GetContact(0).point;
                NewCylinder.name = "SubtractionCylinder";
                GameObject[] CutResults1 = NewCylinder.SliceInstantiate(transform.position, transform.right);
                GameObject[] CutResults2 = CutResults1[1].SliceInstantiate(collision.transform.position, collision.transform.right);
                GameObject FinalSegment = CutResults2[1];
                /*
                Destroy(NewCylinder);
                Destroy(CutResults1[0]);
                Destroy(CutResults1[1]);
                Destroy(CutResults2[0]);
                */
            }
                

            print(collision.GetContact(0).point);
            Destroy(gameObject);
            Destroy(this);
        }
        
    }


}
