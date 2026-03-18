using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class DualContouring : MonoBehaviour
{
    public Vector3Int MDims = new Vector3Int(32, 48, 32); //Marble Dimensions
    public float IsoLevel = 0;

    float[,,] Density;

    MeshFilter MeshFilter;
    Mesh Mesh;

    public GameObject TempBall;

    void Start()
    {

        Density = new float[MDims.x, MDims.y, MDims.z];
        MeshFilter = GetComponent<MeshFilter>();

        /*
        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                    Density[x, y, z] = 1;
        */
        //GenerateMesh();
    }

    void Update()
    {
        



    }



    void GenerateSphere()
    {

        Vector3 Centre = Vector3.one * MDims.x *0.5f;
        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {
                    Vector3 Pos = new Vector3(x, y, z);
                    float Distance = Vector3.Distance(Pos, Centre);

                    Density[x, y, z] = Distance - (MDims.x * 0.3f);
                }
                    //Density[x, y, z] = 1;

    }


    void GenerateMesh()
    {

        List<Vector3> Vertices = new List<Vector3>();
        List<int> Triangles = new List<int>();

        int[,,] VertexIndices = new int[MDims.x, MDims.y, MDims.z];


        //Generate Vertices
        for (int x = 0; x < MDims.x -1; x++)
            for (int y = 0; y < MDims.y-1; y++)
                for (int z = 0; z < MDims.z - 1; z++)
                {
                    Vector3 Vertex;
                    if (ProcessCell(x, y ,z, out Vertex))
                    {
                        VertexIndices[x,y,z] = Vertices.Count;
                        Vertices.Add(Vertex);
                    }
                    else
                        VertexIndices[x, y, z] = -1;

                }


        //Build Faces
        for (int x = 0; x < MDims.x - 2; x++)
            for (int y = 0; y < MDims.y - 2; y++)
                for (int z = 0; z < MDims.z - 2; z++)
                {
                    BuildQuads(x, y, z, VertexIndices, Triangles);

                }

        Mesh = new Mesh();
        Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        Mesh.vertices = Vertices.ToArray();
        Mesh.triangles = Triangles.ToArray();
        Mesh.RecalculateNormals();

        MeshFilter.mesh = Mesh;

    }


    bool ProcessCell(int x, int y, int z, out Vector3 Vertex)
    {

        List<Vector3> Points = new List<Vector3>();
        List<Vector3> Normals = new List<Vector3>();

        float[] Cube = new float[8];

        for (int i = 0; i < 8; i++)
        {
            Vector3Int Corner = new Vector3Int(x,y,z) + Corners[i];
            Cube[i] = Density[Corner.x,Corner.y,Corner.z];
        }

        //Check if the surface of the object crosses the cell
        bool HasInside = false;
        bool HasOutside = false;

        for (int i = 0; i < 8; i++)
        {
            if (Cube[i] < IsoLevel) HasInside = true;
            else HasOutside = true;
        }

        if (!(HasInside && HasOutside))
        {
            Vertex = Vector3.zero; 
            return false;
        }

        //Find Edge Intersections
        for (int i = 0; i < 12; i++)
        {
            int a = Edges[i,0];
            int b = Edges[i,1];

            float da = Cube[a];
            float db = Cube[b];

            if ((da <  IsoLevel && db > IsoLevel) || (da > IsoLevel && db < IsoLevel))
            {
                Vector3 Point1 = new Vector3(x,y,z) + Corners[a];
                Vector3 Point2 = new Vector3(x,y,z) + Corners[b];

                float t = (IsoLevel - da) / (db/da);
                Vector3 Point = Vector3.Lerp(Point1, Point2, t);

                Points.Add(Point);

                //Approximate normal
                //Vector3 Normal = 
            }
        }

        Vertex = new Vector3(x, y, z);
        return true;
    }


    void BuildQuads(int x, int y,int z, int[,,] Indices, List<int> Tris)
    {

    }



    static readonly Vector3Int[] Corners = new Vector3Int[]
{
    new Vector3Int(0,0,0),
    new Vector3Int(1,0,0),
    new Vector3Int(1,1,0),
    new Vector3Int(0,1,0),
    new Vector3Int(0,0,1),
    new Vector3Int(1,0,1),
    new Vector3Int(1,1,1),
    new Vector3Int(0,1,1)
};

    static readonly int[,] Edges = new int[,]
    {
    {0,1},{1,2},{2,3},{3,0},
    {4,5},{5,6},{6,7},{7,4},
    {0,4},{1,5},{2,6},{3,7}
    };




}
