using UnityEngine;
using Unity.Mathematics;
using EzySlice;
using System.Collections;
using System.Linq;
using UnityEditor;
using System.Net;

public class Slicing : MonoBehaviour
{
    public GameObject Marble;
    public GameObject SubtractionCylinder;
    GameObject Sword;

    DualContouring DCScript;

    public float MoveSpeed;
    public float RotateSpeed;
    float CurRotation;
    float CutCount = 0;

    public Material MarbleMat;
    TextureRegion Region;

    public int MoveMode = 1;

    public int WeaponOn = 1; //1 Katana 2 Tanto 3 Naginata 4 Kama


    Vector3 MarbleStartPos;
    Vector3 TantoStartPoint;
    Vector3 TantoEndPoint;
    GameObject NewTantoCut;

    void Start()
    {
        Sword = GameObject.Find("Sword");

        CurRotation = Sword.transform.rotation.z;
        MarbleStartPos = Marble.transform.position;
        DCScript = Marble.GetComponent<DualContouring>();
    }


    void Update()
    {

        if (WeaponOn == 1)
        {
            Sword.GetComponent<MeshRenderer>().enabled = true;
            if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
            {
                //StartCoroutine(KatanaSlice(Sword.transform.position, Sword.transform.right));
            }
        }
        if (WeaponOn == 2)
        {
            Sword.GetComponent<MeshRenderer>().enabled = false;
            if (Input.GetMouseButtonDown(0))
            {
                TantoStartPoint = Sword.transform.position;
                //GameObject Temp = Instantiate(Marble);
                //Temp.transform.position = TantoStartPoint;
                //Temp.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                NewTantoCut = Instantiate(Sword);
                NewTantoCut.GetComponent<MeshRenderer>().enabled = true;
                NewTantoCut.transform.localScale = new Vector3(0.1f, 0.05f, 0.1f);
                NewTantoCut.name = "TantoCut";
                NewTantoCut.GetComponent<BoxCollider>().enabled = true;
                NewTantoCut.AddComponent<Rigidbody>().useGravity = false;
                NewTantoCut.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                NewTantoCut.GetComponent<MeshRenderer>().material = GameObject.Find("TestTarget").GetComponent<MeshRenderer>().material;
            }
            if (Input.GetMouseButton(0))
            {
                TantoControl();
            }
            if (Input.GetMouseButtonUp(0))
            {
                //NewTantoCut.AddComponent<TantoCutCross>().SubtractionCylinder = SubtractionCylinder;
                //NewTantoCut.GetComponent<TantoCutCross>().StartPoint = TantoStartPoint;
                DCScript.ApplyTantoCut(TantoStartPoint, TantoEndPoint);
                //DCScript.ApplyBladeCut(NewTantoCut);
            }
        }


        if (Input.GetKeyDown(KeyCode.V))
        {
            if (MoveMode == 1)
                MoveMode = 2;
            else
                MoveMode = 1;
        }

        //GameObject.Find("Result2").transform.position = Sword.transform.position;
    }

    private void FixedUpdate()
    {
        if (MoveMode == 1)
        {
            float Horizontal = Input.GetAxis("Horizontal"); float Vertical = Input.GetAxis("Vertical"); float Rotation = Input.GetAxisRaw("Rotation");

            if (Vertical != 0 && Horizontal != 0)
            {
                Sword.transform.position += Vector3.up * Vertical * (MoveSpeed / 100) / 1.3f;
                Sword.transform.position += Vector3.right * Horizontal * (MoveSpeed / 100) / 1.3f;
                CurRotation += Rotation * RotateSpeed;
                Sword.transform.rotation = Quaternion.Euler(0, 0, CurRotation);
            }
            else
            {
                Sword.transform.position += Vector3.up * Vertical * (MoveSpeed / 100);
                Sword.transform.position += Vector3.right * Horizontal * (MoveSpeed / 100);

                CurRotation += Rotation * RotateSpeed;
                Sword.transform.rotation = Quaternion.Euler(0, 0, CurRotation);
            }
        }
        else
        {
            Vector3 MousePos = Input.mousePosition;
            MousePos.z = MarbleStartPos.z - Camera.main.transform.position.z;
            MousePos = Camera.main.ScreenToWorldPoint(MousePos);


            if (Input.GetMouseButton(1))
            {
                Vector3 SwordDirection = MousePos - Sword.transform.position;
                Sword.transform.right = SwordDirection;
            }
            else
            {Sword.transform.position = MousePos;}
        }
       

        

    }

   

    public IEnumerator KatanaSlice(Vector3 SwordPos, Vector3 SwordDirection)
    {
        
        GameObject[] CutResults = Marble.SliceInstantiate(SwordPos, SwordDirection, Region, MarbleMat); //Do the cut
        GameObject Debris;
        float SliceDirection;

        if (CutResults == null) //If the cut is off the screen, break (Otherwise fully destroys model & breaks)
        { yield break; }

        Destroy(Marble); //Remove obsolete Gameobject

        if (CutResults[0].GetComponent<Renderer>().bounds.extents.y > CutResults[1].GetComponent<Renderer>().bounds.extents.y || CutResults[0].GetComponent<Renderer>().bounds.extents.x > CutResults[1].GetComponent<Renderer>().bounds.extents.x)
        { Marble = CutResults[0]; Debris = CutResults[1]; } 
        else {Marble = CutResults[1]; Debris = CutResults[0]; } // Checks which side is debris & and which is marble

        SliceDirection = Mathf.Sign(Sword.transform.position.x -Debris.transform.position.x); //Determine which direction to fling debris
        Marble.AddComponent<MeshCollider>().convex = true;
        Debris.AddComponent<MeshCollider>().convex = true;
        Debris.AddComponent<Rigidbody>().AddForce(Vector3.up + Vector3.right * SliceDirection * 10, ForceMode.Impulse);
        Marble.name = "Marble";
        Debris.name = "Debris";
        yield return new WaitForSeconds(2);
        Destroy(Debris); //Clean up debris
    }


    void TantoControl()
    {
        TantoEndPoint = Sword.transform.position;
        NewTantoCut.transform.position = (TantoStartPoint + TantoEndPoint) / 2;
        NewTantoCut.transform.position = new Vector3(NewTantoCut.transform.position.x, NewTantoCut.transform.position.y, -5.2f);
        float Length = Vector3.Distance(TantoStartPoint, TantoEndPoint);
        Vector3 Direction = (TantoEndPoint - TantoStartPoint).normalized;
        if (Direction !=  Vector3.zero)
            NewTantoCut.transform.rotation = Quaternion.LookRotation(Direction);

        NewTantoCut.transform.localScale = new Vector3(2.1f, 0.005f, Length+0.01f);


    }
}
