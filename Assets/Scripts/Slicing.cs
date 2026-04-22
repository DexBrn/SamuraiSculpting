using UnityEngine;
using Unity.Mathematics;
using EzySlice;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.Net;

public class Slicing : MonoBehaviour
{
    public GameObject Marble;
    public GameObject SubtractionCylinder;
    public GameObject ViewModel;
    GameObject Sword;
    public GameObject KatanaModel;
    public GameObject TantoModel;


    DualContouring DCScript;
    Timer Timer;

    public float MoveSpeed;
    public float RotateSpeed;
    float CurRotation;
    public float CutCount = 0;

    bool CanAttack = true;

    public Material MarbleMat;
    TextureRegion Region;

    public int MoveMode = 1;

    public int WeaponOn = 1; //1 Katana 2 Tanto 3 Naginata 4 Kama


    Vector3 MarbleStartPos;
    Vector3 TantoStartPoint;
    Vector3 TantoEndPoint;
    GameObject NewTantoCut;

    Vector3 OriginalViewModelPos;




    void Start()
    {
        Sword = GameObject.Find("Sword");

        CurRotation = Sword.transform.rotation.z;
        MarbleStartPos = Marble.transform.position;
        DCScript = Marble.GetComponent<DualContouring>();
        Timer = GetComponent<Timer>();
        OriginalViewModelPos = ViewModel.transform.position;
    }


    void Update()
    {

        if (WeaponOn == 1)
        {
            Sword.GetComponent<MeshRenderer>().enabled = true;
            if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
            {
                StartCoroutine(SliceVisual());
                
                CutCount++;
                Timer.TimerOn = true;
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
               
                Vector3 voxelStart = WorldToVoxel(TantoStartPoint);
                Vector3 voxelEnd = WorldToVoxel(TantoEndPoint);

                Vector3 camForward = Marble.transform.InverseTransformDirection(Camera.main.transform.forward);

                Vector3 screenDir = Vector3.ProjectOnPlane(voxelEnd - voxelStart, camForward).normalized;
                Vector3 PlaneNormal = Vector3.Cross(screenDir, camForward).normalized;


                Vector3 direction = (voxelEnd - voxelStart).normalized;
                float length = Vector3.Distance(voxelStart, voxelEnd);

                // Stable rotation
                Vector3 up = Camera.main.transform.forward;
                if (Vector3.Dot(direction, up) > 0.99f)
                    up = Vector3.up;

                // Z axis = along the slice (blade direction)
                Vector3 forward = direction;

                // Y axis = plane normal (this makes cut face camera correctly)
                Vector3 upAxis = PlaneNormal;

                // X axis = perpendicular (completes orthonormal basis)
                Vector3 right = Vector3.Cross(upAxis, forward).normalized;

                // Rebuild a perfectly aligned rotation
                Quaternion rotation = Quaternion.LookRotation(forward, upAxis);

                // Proper sizes
                float thickness = 0.75f;
                Vector3 MDims = DCScript.MDims;
                float depth = Mathf.Max(MDims.x, MDims.y, MDims.z);

                Vector3 halfSize = new Vector3(
                    depth,
                    thickness,
                    length *0.5f
                );

                // Accurate centre
                Vector3 centre = (voxelStart + voxelEnd) * 0.5f;
                //print(centre);
                // Apply
                DCScript.ApplyTantoCut(centre, halfSize, rotation);
                /*
                /var islands = DCScript.FindIslands();
                //print(islands);
                List<Vector3Int> mainIsland = islands[0];

                foreach (var island in islands)
                {
                    if (island.Count > mainIsland.Count)
                        mainIsland = island;

                }
                foreach (var island in islands)
                {
                    if (island == mainIsland) continue;

                    DCScript.CreateDebris(island);
                }
                */
                CutCount++;
                Timer.TimerOn = true;
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
        { WeaponOn = 1; KatanaModel.SetActive(true); TantoModel.SetActive(false); }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        { WeaponOn = 2; KatanaModel.SetActive(false); TantoModel.SetActive(true); }

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
                Sword.transform.up = SwordDirection;
            }
            else
            {
                Vector2 MInput = MousePos;
                MInput.x = Mathf.Clamp(MInput.x, 0.09f, 1.09f);
                MInput.y = Mathf.Clamp(MInput.y, 1.09f, 3.09f);

                Sword.transform.position = MousePos; 
                ViewModel.transform.localPosition = Vector3.Slerp(ViewModel.transform.localPosition, new Vector3(MInput.x, MInput.y-2, 0) +OriginalViewModelPos, 3f * Time.deltaTime);

            }
        }



    }

   
    /*
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
    */

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
        Vector3 local = Marble.transform.InverseTransformPoint(worldPos);
        //print(local);
        Vector3 MDims = DCScript.MDims;
        // Convert local  voxel space
        return local;

    }

    IEnumerator SliceVisual()
    {
        if (CanAttack)
        {
            CanAttack = false;
            ViewModel.GetComponent<Animator>().enabled = true;
            ViewModel.GetComponent<Animator>().SetBool("IsCutting", true);
            yield return new WaitForSeconds(0.22f);
            DCScript.Slice(Sword.transform.position - Sword.transform.up * 2, Sword.transform.position + Sword.transform.up * 2);
            yield return new WaitForSeconds(0.09f);
            ViewModel.GetComponent<Animator>().SetBool("IsCutting", false);
            yield return new WaitForSeconds(.59f);
            ViewModel.GetComponent<Animator>().enabled = false;
            CanAttack = true;
        }
        

    }

}
