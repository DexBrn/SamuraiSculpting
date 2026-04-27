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
    public Material TantoCutMat;
    TextureRegion Region;

    public int MoveMode = 1;

    public int WeaponOn = 1; //1 Katana 2 Tanto 3 Naginata 4 Kama


    Vector3 MarbleStartPos;
    Vector3 TantoStartPoint;
    Vector3 TantoEndPoint;
    GameObject NewTantoCut;

    Vector3 OriginalViewModelPos;

    public bool DoingWalkOut;


    void Start()
    {
        Sword = GameObject.Find("Sword");

        CurRotation = Sword.transform.rotation.z;
        MarbleStartPos = Marble.transform.position;
        DCScript = Marble.GetComponent<DualContouring>();
        Timer = GetComponent<Timer>();
        OriginalViewModelPos = ViewModel.transform.position;
        StartCoroutine(KatanaBeginning());
    }


    void Update()
    {

        if (WeaponOn == 1)
        {
            Sword.GetComponent<MeshRenderer>().enabled = true;
            if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
            {
                StartCoroutine(SliceVisual());
                
                
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
                NewTantoCut.GetComponent<MeshRenderer>().material = TantoCutMat;
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

                Vector3 up = Camera.main.transform.forward;
                if (Vector3.Dot(direction, up) > 0.99f)
                    up = Vector3.up;

                Quaternion rotation = Quaternion.LookRotation(direction, PlaneNormal);

                float thickness = 0.75f;
                Vector3 MDims = DCScript.MDims;
                float depth = Mathf.Max(MDims.x, MDims.y, MDims.z);
                Vector3 halfSize = new Vector3(depth, thickness, length * 0.5f);
                Vector3 centre = (voxelStart + voxelEnd) * 0.5f;

                StartCoroutine(ApplyCutAndSpawnDebris(centre, halfSize, rotation));

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

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            if (DoingWalkOut)
                StartCoroutine(SkipOpening());


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
        float Length = Mathf.Clamp(Vector3.Distance(TantoStartPoint, TantoEndPoint), 0, 1.5f);

        

        Gradient TantoGradient = new Gradient();
        TantoGradient.SetKeys(
            new GradientColorKey[] {
        new GradientColorKey(Color.green, 0f),
        new GradientColorKey(Color.yellow, 0.5f),
        new GradientColorKey(Color.red, 1f)
            },
            new GradientAlphaKey[] {
        new GradientAlphaKey(1f, 1f),
        new GradientAlphaKey(1f, 1f)
            }
        );
        Color TantoColour = TantoGradient.Evaluate(Length /1.5f);

        NewTantoCut.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", TantoColour);
        NewTantoCut.GetComponent<MeshRenderer>().material.color = TantoColour;

        if (Length == 1.5f)
            TantoEndPoint = TantoStartPoint + (TantoEndPoint - TantoStartPoint).normalized * 1.5f;
        NewTantoCut.transform.position = (TantoStartPoint + TantoEndPoint) / 2;
        NewTantoCut.transform.position = new Vector3(NewTantoCut.transform.position.x, NewTantoCut.transform.position.y, -5.2f);
        
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
            CutCount++;
            Timer.TimerOn = true;
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


    IEnumerator ApplyCutAndSpawnDebris(Vector3 centre, Vector3 halfSize, Quaternion rotation)
    {
        // Carve the density field
        for (int x = 0; x < DCScript.MDims.x; x++)
        {
            for (int y = 0; y < DCScript.MDims.y; y++)
                for (int z = 0; z < DCScript.MDims.z; z++)
                {
                    Vector3 Local = Quaternion.Inverse(rotation) *
                                    (new Vector3(x, y, z) - centre);
                    if (Mathf.Abs(Local.x) < halfSize.x &&
                        Mathf.Abs(Local.y) < halfSize.y &&
                        Mathf.Abs(Local.z) < halfSize.z)
                    {
                        DCScript.Density[x, y, z] = -100f;
                    }
                }

            if (x % 8 == 0) yield return null; // spread carve across frames
        }

        // Find islands in the affected Y region
        int MinY = Mathf.Clamp(Mathf.FloorToInt(centre.y - halfSize.z - 2f), 0, DCScript.MDims.y - 1);
        int MaxY = Mathf.Clamp(Mathf.CeilToInt(centre.y + halfSize.z + 2f), 0, DCScript.MDims.y - 1);

        var islands = DCScript.FindIslands(0, DCScript.MDims.x - 1, MinY, MaxY, 0, DCScript.MDims.z - 1);
        yield return null;

        if (islands.Count > 0)
        {
            // Find the largest island (main marble)
            List<Vector3Int> mainIsland = islands[0];
            foreach (var island in islands)
                if (island.Count > mainIsland.Count)
                    mainIsland = island;

            // Spawn debris for everything else, max 2 pieces
            int debrisMade = 0;
            foreach (var island in islands)
            {
                if (island == mainIsland) continue;
                if (debrisMade >= 2) break;
                DCScript.CreateDebris(island);
                debrisMade++;
                yield return null;
            }
        }

        // Rebuild mesh async
        DCScript.GenerateMesh();

        Destroy(GameObject.Find("TantoCut"));
    }


    IEnumerator KatanaBeginning()
    {
        Sword.SetActive(false);
        CanAttack = false;
        DoingWalkOut = true;
        yield return new WaitForSeconds(6.0f);
        ViewModel.GetComponent<Animator>().enabled = false;
        Sword.SetActive(true);
        DoingWalkOut = false;
        CanAttack = true;
    }

    IEnumerator SkipOpening()
    {
        Time.timeScale = 100;
        yield return new WaitForSeconds(50);
        Time.timeScale = 1;
    }
}
