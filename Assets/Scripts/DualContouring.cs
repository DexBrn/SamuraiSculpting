using System.Collections.Generic;
using UnityEngine;


public class DualContouring : MonoBehaviour
{
    public Vector3Int MDims = new Vector3Int(32, 48, 32); //Marble Dimensions
    public float IsoLevel = 0;

    float[,,] Density;

    MeshFilter MeshFilter;
    Mesh Mesh;


    void Start()
    {

        Density = new float[MDims.x, MDims.y, MDims.z];
        MeshFilter = GetComponent<MeshFilter>();
        //GenerateSphere();
        GenerateCuboid();
        GenerateMesh();
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.J))
        {
            
        }


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

    void GenerateCuboid()
    {
        
        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {
                    bool OnBoundary = (x == 0 || x == MDims.x - 1 ||
                        y == 0 || y == MDims.y - 1 ||
                        z == 0 || z == MDims.z - 1);

                    Density[x, y, z] = OnBoundary ? -1f : 1f; 
                }
    }


    void GenerateMesh()
    {

        List<Vector3> Vertices = new List<Vector3>();
        List<int> Triangles = new List<int>();

        int[,,] VertexIndices = new int[MDims.x, MDims.y, MDims.z];


        // Generate one vertex per cell that contains a surface crossing
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
        for (int x = 1; x < MDims.x - 2; x++)
            for (int y = 1; y < MDims.y - 2; y++)
                for (int z = 1; z < MDims.z - 2; z++)
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

                float t = (IsoLevel - da) / (db-da);
                Vector3 Point = Vector3.Lerp(Point1, Point2, t);

                Points.Add(Point);

                //Approximate normal
                Vector3 Normal = CalculateNormal(Point);
                Normals.Add(Normal);
            }
        }
        if (Points.Count == 0)
        {
            Vertex = Vector3.zero;
            return false;
        }

        Vertex = SolveQEF(Points, Normals);
        return true;
    }


    void BuildQuads(int x, int y,int z, int[,,] Indices, List<int> Tris)
    {
        // X face
        TryQuad(Indices[x, y, z], Indices[x, y, z + 1], Indices[x, y + 1, z + 1], Indices[x, y + 1, z], Tris);

        // Y face
        TryQuad(Indices[x, y, z], Indices[x + 1, y, z], Indices[x + 1, y, z + 1], Indices[x, y, z + 1], Tris);

        // Z face
        TryQuad(Indices[x, y, z], Indices[x + 1, y, z], Indices[x + 1, y + 1, z], Indices[x, y + 1, z], Tris);

    }

    bool HasSignChange(int x0, int y0, int z0, int x1, int y1, int z1) 
    {
        float d0 = Density[x0, y0, z0];
        float d1 = Density[x1, y1, z1];
        return (d0 < IsoLevel) != (d1 < IsoLevel);

    }


    void TryQuad(int a, int b, int c, int d, List<int> Tris)
    {
        if (a < 0 || b < 0 || c < 0 || d < 0) return;

        Tris.Add(a);
        Tris.Add(b);
        Tris.Add(c);

        Tris.Add(a);
        Tris.Add(c);
        Tris.Add(d);
    }



    Vector3 CalculateNormal(Vector3 Pos)
    {
        float d = 0.01f;

        float dx = Sample(Pos + Vector3.right * d) - Sample(Pos - Vector3.right * d);
        float dy = Sample(Pos + Vector3.up * d) - Sample(Pos - Vector3.up * d);
        float dz = Sample(Pos + Vector3.forward * d) - Sample(Pos - Vector3.forward * d);

        return new Vector3(dx, dy, dz).normalized;
    }

    float Sample(Vector3 Pos)
    {
        int x = Mathf.Clamp((int)Pos.x, 0, MDims.x - 1);
        int y = Mathf.Clamp((int)Pos.y, 0, MDims.y - 1);
        int z = Mathf.Clamp((int)Pos.z, 0, MDims.z - 1);

        return Density[x, y, z];
    }

    Vector3 SolveQEF(List<Vector3> Points, List<Vector3> Normals)
    {
        Vector3 Avg = Vector3.zero;

        for (int i = 0; i < Points.Count; i++)
            Avg += Points[i];


        Avg /=Points.Count;

        // Simple approximation: move towards planes
        for (int iter = 0; iter < 5; iter++)
        {
            for (int i = 0; i < Points.Count; ++i)
            {
                Vector3 Point = Points[i];
                Vector3 Normal = Normals[i];

                float Distance = Vector3.Dot(Normal, Avg - Point);
                Avg -= Normal * Distance;
            }
        }


        return Avg;


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
