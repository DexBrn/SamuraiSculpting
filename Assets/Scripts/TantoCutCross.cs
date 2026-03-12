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
        //print($"{StartPoint} :: {OtherStartPoint} :: {Middlepoint}");

        GameObject NewCylinder = Instantiate(SubtractionCylinder);
        

        for (int i = 0; i < collision.contacts.Length; i++)
        {
            //print(collision.GetContact(i).point);
            if (Vector3.Distance(Middlepoint, collision.GetContact(i).point) < Distance)
            { Distance = Vector3.Distance(Middlepoint, collision.GetContact(i).point); BestCollisionPoint = collision.GetContact(i).point; }
        }
        //print(Distance);
        //print(BestCollisionPoint);


        NewCylinder.transform.position = BestCollisionPoint;
        NewCylinder.name = "SubtractionCylinder";
        GameObject[] CutResults1 = NewCylinder.SliceInstantiate(transform.position, transform.up);

        Distance = 9999;

        for (int i = 0; i < CutResults1.Length; i++)
        {
            Transform Tim = (Instantiate(transform));
            Tim.position = Middlepoint;
            var Vertices = CutResults1[i].GetComponent<MeshFilter>().mesh.vertices;
            //print($"THIS :: The Point: {transform.TransformPoint(Vertices[0])}, the point: {transform.position} ");
            //print($"THAT :: The Point: {collision.transform.TransformPoint(Vertices[0])}, the point: {collision.transform.position} ");
            //print($"MID :: The Point: {Tim.TransformPoint(Vertices[0])}, the MidPoint: {Middlepoint} ");

            Vector3 Average = new Vector3(0, 0, 0);
            for (int y = 0; y < Vertices.Length; y++)
            {
                Average += transform.TransformPoint(Vertices[y]);
            }
            
            Average = Average / Vertices.Length;
            print("Average: " + Average);
            print("Distance before compare: " + Distance);
            if (Vector3.Distance(Middlepoint, Average) < Distance)
            { Distance = Vector3.Distance(Middlepoint, Average); print("Distance: " + Distance); CloserSlice = CutResults1[i]; }
            Destroy(Tim.gameObject);   
        }
        Distance = 9999;
        //print(CloserSlice);
        GameObject[] CutResults2 = CloserSlice.SliceInstantiate(collision.transform.position, collision.transform.up);
        for (int i = 0; i < CutResults2.Length; i++)
        {
            if (Vector3.Distance(Middlepoint, CutResults2[i].transform.position) < Distance)
            { Distance = Vector3.Distance(Middlepoint, CutResults2[i].transform.position); CloserSlice = CutResults2[i]; }
        }
        
        GameObject FinalSegment = CloserSlice;
        print(FinalSegment.name);







        /*
        Destroy(NewCylinder);
        Destroy(CutResults1[0]);
        Destroy(CutResults1[1]);
        Destroy(CutResults2[0]);
        */
    }
}
