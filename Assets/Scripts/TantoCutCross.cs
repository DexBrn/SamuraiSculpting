using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UIElements;
using EzySlice;

public class TantoCutCross : MonoBehaviour
{
    public GameObject SubtractionCylinder;
    GameObject CloserSlice;

    TextureRegion Region;

    public Vector3 StartPoint;
    Vector3 OtherStartPoint;
    Vector3 BestCollisionPoint;

    void Start()
    {
        print("Placed");
    }


    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.GetComponent<TantoCutCross>() && !GameObject.Find("SubtractionCylinder") )
        { OtherStartPoint = collision.gameObject.GetComponent<TantoCutCross>().StartPoint; CombineCut(collision); }
    }

    void CombineCut(Collision collision)
    {

        

        Vector3 Middlepoint = (StartPoint + OtherStartPoint) / 2;
        float Distance = 9999;
        print($"{StartPoint} :: {OtherStartPoint} :: {Middlepoint}");

        GameObject NewCylinder = Instantiate(SubtractionCylinder);
        

        for (int i = 0; i < collision.contacts.Length; i++)
        {
            if (Vector3.Distance(Middlepoint, collision.GetContact(i).point) < Distance)
            { Distance = Vector3.Distance(Middlepoint, collision.GetContact(0).point); BestCollisionPoint = collision.GetContact(i).point; }
        }

        NewCylinder.transform.position = BestCollisionPoint;
        NewCylinder.name = "SubtractionCylinder";
        GameObject[] CutResults1 = NewCylinder.SliceInstantiate(transform.position, transform.up);



        for (int i = 0; i < CutResults1.Length; i++)
        {
            if (Vector3.Distance(Middlepoint, CutResults1[i].transform.position) < Distance)
            { Distance = Vector3.Distance(Middlepoint, CutResults1[i].transform.position); CloserSlice = CutResults1[i]; }
        }
        GameObject[] CutResults2 = CloserSlice.SliceInstantiate(collision.transform.position, collision.transform.up);
        for (int i = 0; i < CutResults2.Length; i++)
        {
            if (Vector3.Distance(Middlepoint, CutResults2[i].transform.position) < Distance)
            { Distance = Vector3.Distance(Middlepoint, CutResults2[i].transform.position); CloserSlice = CutResults2[i]; }
        }

        GameObject FinalSegment = CloserSlice;
        print(FinalSegment);








        /*
        Destroy(NewCylinder);
        Destroy(CutResults1[0]);
        Destroy(CutResults1[1]);
        Destroy(CutResults2[0]);
        */
    }
}
