using UnityEngine;
using Unity;
using System.Collections;
using System.Collections.Generic;
using EzySlice;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEditor.PlayerSettings;
using Unity.VisualScripting;
using System.Net.Http.Headers;


public class VoxelMarble : MonoBehaviour
{

    public Vector3 MDims = new Vector3(32, 48, 32); //Marble Dimensions

    bool[,,] Voxels;

    MeshFilter MeshFilter;

    public GameObject TempBall;

    void Start()
    {

        //Create Solid Block of Marble with X Y Z dimensions determined above

        Voxels = new bool[Mathf.RoundToInt(MDims.x), Mathf.RoundToInt(MDims.y), Mathf.RoundToInt(MDims.z)];
        MeshFilter = GetComponent<MeshFilter>();

        for(int x=0;  x<MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                    Voxels[x, y, z] = true;
        
        GenerateMesh();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            CarveSphere(TempBall.transform.position, 5);
        }
    }

    void GenerateMesh()
    {

        List<Vector3> Vertices = new List<Vector3>();
        List<int> Triangles = new List<int>();

        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {
                    if (!Voxels[x, y, z]) continue;

                    AddVoxelFaces(x, y, z, Vertices, Triangles);

                }


        Mesh Mesh = new Mesh();
        Mesh.vertices = Vertices.ToArray();
        Mesh.triangles = Triangles.ToArray();
        Mesh.RecalculateNormals();

        MeshFilter.mesh = Mesh;

    }

    void AddVoxelFaces(int x, int y, int z, List<Vector3> Vertices, List<int> Triangles)
    {

        Vector3 Pos = new Vector3(x, y, z);

        //Add faces on outside of model
        if (IsEmpty(x+1, y, z))
            AddFace(Pos, Vector3.right, Vertices, Triangles);

        if (IsEmpty(x - 1, y, z))
            AddFace(Pos, Vector3.left, Vertices, Triangles);

        if (IsEmpty(x, y + 1, z))
            AddFace(Pos, Vector3.up, Vertices, Triangles);

        if (IsEmpty(x, y - 1, z))
            AddFace(Pos, Vector3.down, Vertices, Triangles);

        if (IsEmpty(x, y, z + 1))
            AddFace(Pos, Vector3.forward, Vertices, Triangles);

        if (IsEmpty(x, y, z - 1))
            AddFace(Pos, Vector3.back, Vertices, Triangles);


    }

    void AddFace(Vector3 Pos, Vector3 Dir, List<Vector3> Vertices, List<int> Triangles)
    {

        int Index = Vertices.Count;
        Vector3 v0, v1, v2, v3;

        //Find which direction we need to make a triangle
        if (true) 
        {
            if (Dir == Vector3.up)
            {
                v0 = Pos + new Vector3(0, 0, 0);
                v1 = Pos + new Vector3(0, 0, 1);
                v2 = Pos + new Vector3(1, 0, 0);
                v3 = Pos + new Vector3(1, 0, 1);
            }
            else if (Dir == Vector3.down)
            {
                v0 = Pos + new Vector3(0, 1, 0);
                v1 = Pos + new Vector3(1, 1, 0);
                v2 = Pos + new Vector3(0, 1, 1);
                v3 = Pos + new Vector3(1, 1, 1);
            }
            else if (Dir == Vector3.forward)
            {
                v0 = Pos + new Vector3(0, 0, 1);
                v1 = Pos + new Vector3(1, 0, 1);
                v2 = Pos + new Vector3(0, 1, 1);
                v3 = Pos + new Vector3(1, 1, 1);
            }
            else if (Dir == Vector3.back)
            {
                v0 = Pos + new Vector3(0, 0, 0);
                v1 = Pos + new Vector3(0, 1, 0);
                v2 = Pos + new Vector3(1, 0, 0);
                v3 = Pos + new Vector3(1, 1, 0);
            }
            else if (Dir == Vector3.right)
            {
                v0 = Pos + new Vector3(1, 0, 0);
                v1 = Pos + new Vector3(1, 1, 0);
                v2 = Pos + new Vector3(1, 0, 1);
                v3 = Pos + new Vector3(1, 1, 1);
            }
            else // left
            {
                v0 = Pos + new Vector3(0, 0, 0);
                v1 = Pos + new Vector3(0, 0, 1);
                v2 = Pos + new Vector3(0, 1, 0);
                v3 = Pos + new Vector3(0, 1, 1);
            }
        }
        
        //Make Vertices at all of the points determined above
        Vertices.Add(v0);
        Vertices.Add(v1);
        Vertices.Add(v2);
        Vertices.Add(v3);

        //Make Triangles at the positions
        Triangles.Add(Index);
        Triangles.Add(Index + 1);
        Triangles.Add(Index + 2); 

        Triangles.Add(Index + 2);
        Triangles.Add(Index + 1);
        Triangles.Add(Index + 3);




    }
    
    bool IsEmpty(int x, int y, int z)
    {
        if (x<0 || y<0 || z<0 || x>=MDims.x || y >= MDims.y || z >= MDims.z) //If coord is outside of bounds of size then we know it IS empty
            return true;

        return !Voxels[x, y, z]; //Otherwise it is fine
    }

    

    void CarveSphere(Vector3 Centre, float Radius)
    {
        Centre = transform.InverseTransformPoint(Centre);
        print($"CENTRE POS :: {Centre}");

        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {
                    Vector3 Pos = new Vector3(x, y, x); // - new Vector3(MDims.x, MDims.y, MDims.z) / 2;
                    //print($" {x} {y} {z} POS ::: {Pos}  " );
                    //print(Vector3.Distance(Pos, Centre));

                    if (Vector3.Distance(Pos, Centre) < Radius)
                    { Voxels[x, y, z] = false; }

                }
        GenerateMesh();

    }




}
