using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UIElements;
using EzySlice;
using UnityEngine.TestTools;

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
        GameObject NewCylinder = Instantiate(SubtractionCylinder);
        


        //Find best contact point between two cuts
        for (int i = 0; i < collision.contacts.Length; i++)
        {
            if (Vector3.Distance(Middlepoint, collision.GetContact(i).point) < Distance)
            { Distance = Vector3.Distance(Middlepoint, collision.GetContact(i).point); BestCollisionPoint = collision.GetContact(i).point; }
        }

        //Create new cylinder to be cut into a segment and then taken away from the marble
        NewCylinder.transform.position = BestCollisionPoint;
        NewCylinder.name = "SubtractionCylinder";
        GameObject[] CutResults1 = NewCylinder.SliceInstantiate(transform.position, transform.up,  Region, GameObject.Find("Marble").GetComponent<MeshRenderer>().material);

        Distance = 9999;

        //Cut the cylinder using the first slice object, then find the side closer the midpoint
        for (int i = 0; i < CutResults1.Length; i++)
        {
            var Vertices = CutResults1[i].GetComponent<MeshFilter>().mesh.vertices;

            Vector3 Average = new Vector3(0, 0, 0);
            for (int y = 0; y < Vertices.Length; y++)
                Average += CutResults1[i].transform.TransformPoint(Vertices[y]);
            
            Average = Average / Vertices.Length;

            if (Vector3.Distance(Middlepoint, Average) < Distance)
            { Distance = Vector3.Distance(Middlepoint, Average); CloserSlice = CutResults1[i]; }
        }

        Distance = 9999;

        print(GameObject.Find("Marble").GetComponent<MeshRenderer>().material);

        //Cut the slice again using the other slice object to get the segment, then check which one is closer to the midpoint
        GameObject[] CutResults2 = CloserSlice.SliceInstantiate(collision.transform.position, collision.transform.up, Region, GameObject.Find("Marble").GetComponent<MeshRenderer>().material);
        for (int i = 0; i < CutResults2.Length; i++)
        {
            var Vertices = CutResults2[i].GetComponent<MeshFilter>().mesh.vertices;

            Vector3 Average = new Vector3(0, 0, 0);
            for (int y = 0; y < Vertices.Length; y++)
                Average += CutResults2[i].transform.TransformPoint(Vertices[y]);

            Average = Average / Vertices.Length;

            if (Vector3.Distance(Middlepoint, CutResults2[i].transform.position) < Distance)
            { Distance = Vector3.Distance(Middlepoint, Average); CloserSlice = CutResults2[i]; }
        }
        
        GameObject FinalSegment = CloserSlice;
        FinalSegment = Instantiate(FinalSegment);
        Destroy(CutResults1[0]); Destroy(CutResults1[1]); Destroy(CutResults2[0]); Destroy(CutResults2[1]); //Destroy(NewCylinder); Destroy(this);
        NewCylinder.transform.localScale = new Vector3(0, 0, 0);
        print(FinalSegment.name);

        GameObject Marble = GameObject.Find("Marble");


        Model result = CSG.Perform(CSG.BooleanOp.Subtraction, Marble, FinalSegment);
        var composite = new GameObject();
        composite.AddComponent<MeshFilter>().sharedMesh = result.mesh;
        result.materials.Add(Marble.GetComponent<MeshRenderer>().material);
        composite.AddComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();

        GameObject.Find("ScriptHolder").GetComponent<Slicing>().Marble = composite;

        Destroy(Marble);
        composite.name = "Marble";

        Destroy(FinalSegment);
        Destroy(collision.gameObject);
        Destroy(NewCylinder);
        Destroy(gameObject);

    }



    /*
    public void Perform()
    {
        Model result = CSG.Perform(Operation, lhs, rhs);
        var composite = new GameObject();
        composite.AddComponent<MeshFilter>().sharedMesh = result.mesh;
        result.materials.Add(material);
        composite.AddComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();
        composite.name = Operation.ToString() + " Object";
    }
    */
}
