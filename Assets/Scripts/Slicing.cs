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
                
                Vector3 Midpoint = (TantoStartPoint + TantoEndPoint) / 2;
                //Vector3 HalfSize = (TantoEndPoint - Midpoint); HalfSize = new Vector3(Mathf.Abs(HalfSize.x) * 32, Mathf.Abs(HalfSize.y) * 32 + 0.01f, 128f);
                float length = Vector3.Distance(TantoStartPoint, TantoEndPoint);

                // constant thickness (THIS fixes diagonal issue)
                float thickness = 0.75f;

                Vector3 MDims = DCScript.MDims;
                // guaranteed to go through whole marble
                float depth = Mathf.Max(MDims.x, MDims.y, MDims.z);

                Vector3 HalfSize = new Vector3(
                    thickness,        // X  blade thickness (constant!)
                    depth,            // Y  goes THROUGH the marble
                    length * 10f     // Z  along the cut direction
                );

                float TantoX = 16;
                float TantoY = 24;

                if (true)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        if (Midpoint.x <= -0.97f + ((0.060625f * i) + 0.0303125) && Midpoint.x >= -0.97f + ((0.060625f * i) - 0.0303125))
                        { TantoX = i + 16; }
                    }
                    if (Midpoint.x > 0.97f)
                        TantoX = 48;

                    for (int i = 0; i < 48; i++)
                    {
                        if (Midpoint.y <= 0.215f + ((0.060625f * i) + 0.0303125) && Midpoint.y >= 0.215f + ((0.060625f * i) - 0.0303125))
                        { TantoY = i + 24; }
                    }
                    if (Midpoint.y > 3.125f)
                        TantoY = 72;
                }

                Vector3 Direction = (TantoEndPoint - TantoStartPoint).normalized;

                // Use camera forward to stabilize orientation
                Vector3 Up = Camera.main.transform.forward;

                // Prevent degenerate case (when direction ~~ up)
                if (Vector3.Dot(Direction, Up) > 0.99f)
                    Up = Vector3.up;


                DCScript.ApplyBoxCut(new Vector3(TantoX, TantoY, 16), HalfSize, Quaternion.LookRotation(Direction, Up));
                //DCScript.ApplyBoxCut(new Vector3(16, 24, 16), new Vector3(8, 8, 8), Quaternion.identity);
                
                /*
                Vector3 voxelStart = WorldToVoxel(TantoStartPoint);
                Vector3 voxelEnd = WorldToVoxel(TantoEndPoint);

                Vector3 direction = (voxelEnd - voxelStart).normalized;
                float length = Vector3.Distance(voxelStart, voxelEnd);

                // Stable rotation
                Vector3 up = Camera.main.transform.forward;
                if (Vector3.Dot(direction, up) > 0.99f)
                    up = Vector3.up;

                Quaternion rotation = Quaternion.LookRotation(direction, up);

                // Proper sizes
                float thickness = 0.75f;
                Vector3 MDims = DCScript.MDims;
                float depth = Mathf.Max(MDims.x, MDims.y, MDims.z);

                Vector3 halfSize = new Vector3(
                    thickness,
                    depth,
                    length *10f
                );

                // Accurate centre
                Vector3 centre = (voxelStart + voxelEnd) * 0.5f;
                print(centre);
                // Apply
                DCScript.ApplyBoxCut(centre, halfSize, rotation);
                */
            }   
        }


        if (Input.GetKeyDown(KeyCode.V))
        {
            if (MoveMode == 1)
                MoveMode = 2;
            else
                MoveMode = 1;
        }



        if (Input.GetKeyDown(KeyCode.Alpha1))
                WeaponOn = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2))
                WeaponOn = 2;




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

    Vector3 WorldToVoxel(Vector3 worldPos)
    {
        // Convert world  local (relative to marble)
        Vector3 local = transform.InverseTransformPoint(worldPos) * 0.05f;
        Vector3 MDims = DCScript.MDims;
        // Convert local  voxel space
        return new Vector3(
            (local.x + 1f) * 0.5f * MDims.x,
            (local.y + 1f) * 0.5f * MDims.y,
            (local.z + 1f) * 0.5f * MDims.z
        );
    }



}
