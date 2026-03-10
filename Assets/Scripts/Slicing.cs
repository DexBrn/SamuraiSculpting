using UnityEngine;
using Unity.Mathematics;
using LibCSG;
using EzySlice;
using System.Collections;
using System.Linq;

public class Slicing : MonoBehaviour
{
    public GameObject Marble;
    public GameObject SubtractionCylinder;
    GameObject Sword;

    public float MoveSpeed;
    public float RotateSpeed;
    float CurRotation;
    float CutCount = 0;

    public Material MarbleMat;
    TextureRegion Region;

    public int MoveMode = 1;

    public int WeaponOn = 1; //1 Katana 2 Tanto 3 Naginata 4 Kama


    //public Vector2 MousePos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Sword = GameObject.Find("Sword");

        CurRotation = Sword.transform.rotation.z;
    }

    // Update is called once per frame
    void Update()
    {

        if (WeaponOn == 1)
        {
            if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
            {
                StartCoroutine(KatanaSlice(Sword.transform.position, Sword.transform.right));
            }
        }
        if (WeaponOn == 2)
        {
            if (Input.GetMouseButtonDown(0))
            {
                print("Slash Started");
                //Start Point Here
            }
            if (Input.GetMouseButton(0))
            {
                print("Slash Continuing");
                //End Point Here
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(TantoCut(Sword.transform.position, Sword.transform.right));
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
            MousePos.z = Marble.transform.position.z - Camera.main.transform.position.z;
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


    public void SliceOld()
    {
        CutCount++;

        GameObject SliceResult = new GameObject();
        SliceResult.name = $"SliceResult{CutCount}";
        SliceResult.transform.localScale = new Vector3(1, 1, 1);
        SliceResult.transform.position = Sword.transform.position;
        SliceResult.AddComponent<MeshFilter>();
        SliceResult.AddComponent<MeshRenderer>();
        SliceResult.GetComponent<MeshRenderer>().material = MarbleMat;
        SliceResult.AddComponent<DebrisCleanup>();

        // Create the CSGBrushOperation
        CSGBrushOperation CSGOp = new CSGBrushOperation();
        CSGBrushOperation CSGOp2 = new CSGBrushOperation();
        // Create the brush to contain the result of the operation
        // Give a GameObject allow to have the mesh result link with the GameObject Transform link
        // if you don't give a GameObject the Brush create a new GameObject
        CSGBrush cube_sub_cylinder = new CSGBrush(GameObject.Find("Marble"));
        CSGBrush cube_sub_cylinder2 = new CSGBrush(GameObject.Find($"SliceResult{CutCount}"));


        // Create the Brush for the cube
        CSGBrush MarbleBrush = new CSGBrush(GameObject.Find("Marble"));
        // Set-up the mesh in the Brush
        MarbleBrush.build_from_mesh(GameObject.Find("Marble").GetComponent<MeshFilter>().mesh);

        // Create the Brush for the cylinder
        CSGBrush LeftSideBrush = new CSGBrush(GameObject.Find("LeftSide"));
        // Set-up the mesh in the Brush
        LeftSideBrush.build_from_mesh(GameObject.Find("LeftSide").GetComponent<MeshFilter>().mesh);

        // Create the Brush for the cylinder
        CSGBrush RightSideBrush = new CSGBrush(GameObject.Find("RightSide"));
        // Set-up the mesh in the Brush
        RightSideBrush.build_from_mesh(GameObject.Find("RightSide").GetComponent<MeshFilter>().mesh);

        // Do the operation subtration between the cube and the cylinder 
        CSGOp.merge_brushes(Operation.OPERATION_SUBTRACTION, MarbleBrush, LeftSideBrush, ref cube_sub_cylinder);
        CSGOp2.merge_brushes(Operation.OPERATION_SUBTRACTION, MarbleBrush, RightSideBrush, ref cube_sub_cylinder2);

        GameObject.Find("Marble").GetComponent<MeshFilter>().mesh.Clear(); //Was 'Result'
        Destroy(GameObject.Find("Marble").GetComponent<MeshCollider>());
        GameObject.Find($"SliceResult{CutCount}").GetComponent<MeshFilter>().mesh.Clear();

        //Destroy(GameObject.Find("Marble"));
        //Destroy(GameObject.Find("Cube"));





        // Put the mesh result in the mesh give in parameter if you don't give a mesh he return a new mesh with the result

        
        

        cube_sub_cylinder.getMesh(GameObject.Find("Marble").GetComponent<MeshFilter>().mesh); //Was 'Result'
        cube_sub_cylinder2.getMesh(GameObject.Find($"SliceResult{CutCount}").GetComponent<MeshFilter>().mesh);

        print($"{GameObject.Find("Marble").GetComponent<MeshFilter>().mesh.vertexCount}");
        print($"{GameObject.Find($"SliceResult{CutCount}").GetComponent<MeshFilter>().mesh.vertexCount}");

        if (GameObject.Find($"SliceResult{CutCount}").GetComponent<MeshFilter>().mesh.vertexCount > GameObject.Find("Marble").GetComponent<MeshFilter>().mesh.vertexCount)
        {
            cube_sub_cylinder.getMesh(GameObject.Find($"SliceResult{CutCount}").GetComponent<MeshFilter>().mesh);
            cube_sub_cylinder2.getMesh(GameObject.Find("Marble").GetComponent<MeshFilter>().mesh); //Was 'Result'
            
        }
        

        GameObject.Find("Marble").AddComponent<MeshCollider>().convex = true; //Was 'Result'
        GameObject.Find("Marble").transform.localScale = new Vector3(1, 1, 1);
        GameObject.Find($"SliceResult{CutCount}").AddComponent<MeshCollider>().convex = true;
        GameObject.Find($"SliceResult{CutCount}").AddComponent<Rigidbody>();
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


    public IEnumerator TantoCut(Vector3 SwordPos, Vector3 SwordDirection)
    {

        /*Plan


        Create a cylinder, centre on intersection point of two cuts
        Then cut the cylinder with using the two cuts
        Then csg remove the cylinder from the marble 
        ---->>> WE NEED FIND ANOTHER CSG IMPLEMENTATION AHHHHHH

        So we need to create planes using the mouse?
        When you start holding down click, create start pos, then drag to end pos. 
        Create new cube gameobject, maybe a prefab, set direction to mouse, pos to midpoint between start point and mouse, and scale to fill in the space

        Need to make it so we keep the side that is closer to the other plane!
        */

        GameObject NewLivingCut = Instantiate(Sword);
        NewLivingCut.transform.position = SwordPos;
        NewLivingCut.AddComponent<TantoCutCross>().SubtractionCylinder = SubtractionCylinder;
        NewLivingCut.name = "TantoCut";
        NewLivingCut.GetComponent<BoxCollider>().enabled = true;
        NewLivingCut.AddComponent<Rigidbody>().useGravity = false;
        NewLivingCut.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        yield return new WaitForSeconds(2);
    }

    public IEnumerator CreateCircleSegment(GameObject Plane1, GameObject Plane2)
    {


        yield return new WaitForSeconds(2);
    }


}
