using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.EventSystems.EventTrigger;

public class DualContouring : MonoBehaviour
{
    public Vector3Int MDims = new Vector3Int(64, 96, 64);
    public float IsoLevel = 0;

    float[,,] Density;

    MeshFilter MeshFilter;
    Mesh Mesh;

    public GameObject Sword;
    public GameObject Target;

    void Start()
    {
        Density = new float[MDims.x, MDims.y, MDims.z];
        MeshFilter = GetComponent<MeshFilter>();
        //GameObject Sword = GameObject.Find("Sword");

        if (name == "Marble")
        {
            GenerateCuboid();
            //GenerateSphere();
            GenerateMesh();
        }
        
    }

    private void Update()
    {
        if (transform.parent)
        {
            if (Input.GetKeyDown(KeyCode.H))
                Target.SetActive(!Target.activeSelf);
        }
    }

    private void FixedUpdate()
    {
        if (transform.parent)
        {
            if (Input.GetKey(KeyCode.E))
            {

                transform.parent.Rotate(0, 1, 0);
                Target.transform.Rotate(0, 1, 0);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                transform.parent.Rotate(0, -1, 0);
                Target.transform.Rotate(0, -1, 0);
            }

            
        }
        
    }

    public void ResetMarble()
    {
        Density = new float[MDims.x, MDims.y, MDims.z];
        transform.parent.rotation = quaternion.identity;
        GenerateCuboid();
        GenerateMesh();
    }


    void GenerateCuboid()
    {
        Vector3 halfSize = new Vector3(MDims.x, MDims.y, MDims.z) * 0.3f;

        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {
                    Vector3 pos = GetVoxelPosition(x, y, z);

                    Vector3 absPos = new Vector3(
                        Mathf.Abs(pos.x),
                        Mathf.Abs(pos.y),
                        Mathf.Abs(pos.z)
                    );

                    Vector3 d = absPos - halfSize;

                    float outside = Vector3.Max(d, Vector3.zero).magnitude;
                    float inside = Mathf.Min(Mathf.Max(d.x, Mathf.Max(d.y, d.z)), 0);

                    Density[x, y, z] = -(inside + outside);
                }
    }

    void GenerateSphere()
    {
        Vector3 Centre = new Vector3(MDims.x, MDims.y, MDims.z) * 0.5f;
        float Radius = MDims.x * 0.4f;
        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {
                    Vector3 Pos = new Vector3(x, y, z);
                    // Positive inside sphere, negative outside
                    Density[x, y, z] = Radius - Vector3.Distance(Pos, Centre);
                }
    }

    void GenerateMesh()
    {
        List<Vector3> Vertices = new List<Vector3>();
        List<int> Triangles = new List<int>();
        int[,,] VertexIndices = new int[MDims.x, MDims.y, MDims.z];

        // Generate one vertex per cell that contains a surface crossing
        for (int x = 0; x < MDims.x - 1; x++)
            for (int y = 0; y < MDims.y - 1; y++)
                for (int z = 0; z < MDims.z - 1; z++)
                {
                    Vector3 Vertex;
                    if (ProcessCell(x, y, z, out Vertex))
                    {
                        VertexIndices[x, y, z] = Vertices.Count;
                        Vertices.Add(Vertex);
                    }
                    else
                        VertexIndices[x, y, z] = -1;
                }

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
        if (name!= "Marble")
        {
            DestroyImmediate(GetComponent<MeshFilter>());
            MeshFilter= gameObject.AddComponent<MeshFilter>();
        }
        
        MeshFilter.mesh = Mesh;
    }



    bool ProcessCell(int x, int y, int z, out Vector3 Vertex)
    {
        List<Vector3> Points = new List<Vector3>();
        List<Vector3> Normals = new List<Vector3>();

        float[] Cube = new float[8];
        for (int i = 0; i < 8; i++)
        {
            Vector3Int Corner = new Vector3Int(x, y, z) + Corners[i];
            Cube[i] = Density[Corner.x, Corner.y, Corner.z];
        }

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

        for (int i = 0; i < 12; i++)
        {
            int a = Edges[i, 0];
            int b = Edges[i, 1];
            float da = Cube[a];
            float db = Cube[b];

            if ((da < IsoLevel) != (db < IsoLevel))
            {
                Vector3 Point1 = new Vector3(x, y, z) + Corners[a];
                Vector3 Point2 = new Vector3(x, y, z) + Corners[b];
                float t = (IsoLevel - da) / (db - da);
                Vector3 Point = Vector3.Lerp(Point1, Point2, t);
                Points.Add(Point);
                Normals.Add(CalculateNormal(Point));
            }
        }

        if (Points.Count == 0)
        {
            Vertex = Vector3.zero;
            return false;
        }

        Vertex = SolveQEF(Points, Normals, x, y, z);
        return true;
    }

    void BuildQuads(int x, int y, int z, int[,,] Indices, List<int> Tris)
    {

        // X-axis edge: between (x,y,z) and (x+1,y,z)
        if (HasSignChange(x, y, z, x + 1, y, z))
        {
            bool flip = Density[x, y, z] < IsoLevel;
            TryQuad(
                Indices[x, y - 1, z - 1],
                Indices[x, y, z - 1],
                Indices[x, y, z],
                Indices[x, y - 1, z],
                Tris, flip);
        }

        // Y-axis edge: between (x,y,z) and (x,y+1,z)
        if (HasSignChange(x, y, z, x, y + 1, z))
        {
            bool flip = Density[x, y, z] < IsoLevel;
            TryQuad(
                Indices[x - 1, y, z - 1],
                Indices[x - 1, y, z],
                Indices[x, y, z],
                Indices[x, y, z - 1],
                Tris, flip);
        }

        // Z-axis edge: between (x,y,z) and (x,y,z+1)
        if (HasSignChange(x, y, z, x, y, z + 1))
        {
            bool flip = Density[x, y, z] < IsoLevel;
            TryQuad(
                Indices[x - 1, y - 1, z],
                Indices[x, y - 1, z],
                Indices[x, y, z],
                Indices[x - 1, y, z],
                Tris, flip);
        }
    }

    bool HasSignChange(int x0, int y0, int z0, int x1, int y1, int z1)
    {
        float d0 = Density[x0, y0, z0];
        float d1 = Density[x1, y1, z1];
        return (d0 < IsoLevel) != (d1 < IsoLevel);
    }

    void TryQuad(int a, int b, int c, int d, List<int> Tris, bool flip)
    {
        if (a < 0 || b < 0 || c < 0 || d < 0) return;

        if (!flip)
        {
            Tris.Add(a); Tris.Add(b); Tris.Add(c);
            Tris.Add(a); Tris.Add(c); Tris.Add(d);
        }
        else
        {
            Tris.Add(a); Tris.Add(c); Tris.Add(b);
            Tris.Add(a); Tris.Add(d); Tris.Add(c);
        }
    }

    Vector3 CalculateNormal(Vector3 Pos)
    {
        float d = 0.5f; // Use a larger step for voxel data
        float dx = Sample(Pos + Vector3.right * d) - Sample(Pos - Vector3.right * d);
        float dy = Sample(Pos + Vector3.up * d) - Sample(Pos - Vector3.up * d);
        float dz = Sample(Pos + Vector3.forward * d) - Sample(Pos - Vector3.forward * d);
        return new Vector3(dx, dy, dz).normalized;
    }

    float Sample(Vector3 Pos)
    {
        int x0 = Mathf.Clamp(Mathf.FloorToInt(Pos.x), 0, MDims.x - 1);
        int y0 = Mathf.Clamp(Mathf.FloorToInt(Pos.y), 0, MDims.y - 1);
        int z0 = Mathf.Clamp(Mathf.FloorToInt(Pos.z), 0, MDims.z - 1);

        int x1 = Mathf.Clamp(x0 + 1, 0, MDims.x - 1);
        int y1 = Mathf.Clamp(y0 + 1, 0, MDims.y - 1);
        int z1 = Mathf.Clamp(z0 + 1, 0, MDims.z - 1);

        float tx = Pos.x - x0;
        float ty = Pos.y - y0;
        float tz = Pos.z - z0;

        float c000 = Density[x0, y0, z0];
        float c100 = Density[x1, y0, z0];
        float c010 = Density[x0, y1, z0];
        float c110 = Density[x1, y1, z0];

        float c001 = Density[x0, y0, z1];
        float c101 = Density[x1, y0, z1];
        float c011 = Density[x0, y1, z1];
        float c111 = Density[x1, y1, z1];

        float c00 = Mathf.Lerp(c000, c100, tx);
        float c10 = Mathf.Lerp(c010, c110, tx);
        float c01 = Mathf.Lerp(c001, c101, tx);
        float c11 = Mathf.Lerp(c011, c111, tx);

        float c0 = Mathf.Lerp(c00, c10, ty);
        float c1 = Mathf.Lerp(c01, c11, ty);

        return Mathf.Lerp(c0, c1, tz);


    }

    Vector3 SolveQEF(List<Vector3> Points, List<Vector3> Normals, int CellX, int CellY, int CellZ)
    {
        Vector3 MassPoint = Vector3.zero;
        for (int i = 0; i < Points.Count; i++)
            MassPoint += Points[i];
        MassPoint /= Points.Count;

        float ATA00 = 0, ATA01 = 0, ATA02 = 0;
        float ATA11 = 0, ATA12 = 0, ATA22 = 0;
        float ATb0 = 0, ATb1 = 0, ATb2 = 0;

        for (int i = 0; i < Points.Count; i++)
        {
            Vector3 N = Normals[i];
            Vector3 P = Points[i] - MassPoint;

            ATA00 += N.x * N.x; ATA01 += N.x * N.y; ATA02 += N.x * N.z;
            ATA11 += N.y * N.y; ATA12 += N.y * N.z;
            ATA22 += N.z * N.z;

            float B = Vector3.Dot(N, P);
            ATb0 += N.x * B;
            ATb1 += N.y * B;
            ATb2 += N.z * B;
        }

        float Det = ATA00 * (ATA11 * ATA22 - ATA12 * ATA12)
                   - ATA01 * (ATA01 * ATA22 - ATA12 * ATA02)
                   + ATA02 * (ATA01 * ATA12 - ATA11 * ATA02);

        Vector3 Result;
        if (Mathf.Abs(Det) < 1e-4f)
        {
            Result = MassPoint;
        }
        else
        {
            float InvDet = 1f / Det;
            Vector3 Offset;
            Offset.x = InvDet * (ATb0 * (ATA11 * ATA22 - ATA12 * ATA12)
                               + ATb1 * (ATA02 * ATA12 - ATA01 * ATA22)
                               + ATb2 * (ATA01 * ATA12 - ATA11 * ATA02));
            Offset.y = InvDet * (ATb0 * (ATA12 * ATA02 - ATA01 * ATA22)
                               + ATb1 * (ATA00 * ATA22 - ATA02 * ATA02)
                               + ATb2 * (ATA01 * ATA02 - ATA00 * ATA12));
            Offset.z = InvDet * (ATb0 * (ATA01 * ATA12 - ATA02 * ATA11)
                               + ATb1 * (ATA02 * ATA01 - ATA00 * ATA12)
                               + ATb2 * (ATA00 * ATA11 - ATA01 * ATA01));

            Result = MassPoint + Offset;
        }

        // Clamp strictly inside the cell to prevent spikes
        Result.x = Mathf.Clamp(Result.x, CellX, CellX + 1f);
        Result.y = Mathf.Clamp(Result.y, CellY, CellY + 1f);
        Result.z = Mathf.Clamp(Result.z, CellZ, CellZ + 1f);

        return Result;
    }

    static readonly Vector3Int[] Corners = new Vector3Int[]
    {
        new Vector3Int(0,0,0), new Vector3Int(1,0,0),
        new Vector3Int(1,1,0), new Vector3Int(0,1,0),
        new Vector3Int(0,0,1), new Vector3Int(1,0,1),
        new Vector3Int(1,1,1), new Vector3Int(0,1,1)
    };

    static readonly int[,] Edges = new int[,]
    {
        {0,1},{1,2},{2,3},{3,0},
        {4,5},{5,6},{6,7},{7,4},
        {0,4},{1,5},{2,6},{3,7}
    };

    Vector3 GetVoxelPosition(int x, int y, int z)
    {
        return new Vector3(
            x - MDims.x * 0.5f,
            y - MDims.y * 0.5f,
            z - MDims.z * 0.5f
        );
    }


    public List<List<Vector3Int>> FindIslands()
    {
        bool[,,] visited = new bool[MDims.x, MDims.y, MDims.z];
        List<List<Vector3Int>> islands = new List<List<Vector3Int>>();

        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {
                    if (visited[x, y, z]) continue;
                    if (Density[x, y, z] <= IsoLevel) continue;

                    List<Vector3Int> island = new List<Vector3Int>();
                    Queue<Vector3Int> queue = new Queue<Vector3Int>();

                    queue.Enqueue(new Vector3Int(x, y, z));
                    visited[x, y, z] = true;

                    while (queue.Count > 0)
                    {
                        Vector3Int current = queue.Dequeue();
                        island.Add(current);

                        foreach (Vector3Int dir in Directions)
                        {
                            Vector3Int next = current + dir;

                            if (next.x < 0 || next.y < 0 || next.z < 0 ||
                                next.x >= MDims.x || next.y >= MDims.y || next.z >= MDims.z)
                                continue;

                            if (visited[next.x, next.y, next.z]) continue;
                            if (Density[next.x, next.y, next.z] <= IsoLevel) continue;

                            visited[next.x, next.y, next.z] = true;
                            queue.Enqueue(next);
                        }
                    }

                    islands.Add(island);
                }

        return islands;
    }


    static readonly Vector3Int[] Directions = new Vector3Int[]
{
    new Vector3Int(1,0,0), new Vector3Int(-1,0,0),
    new Vector3Int(0,1,0), new Vector3Int(0,-1,0),
    new Vector3Int(0,0,1), new Vector3Int(0,0,-1),
};




    public void CreateDebris(List<Vector3Int> island)
    {
        GameObject debris = Instantiate(gameObject);

        debris.transform.position = Sword.transform.position;
        debris.transform.rotation = transform.rotation;

        var dc = debris.AddComponent<DualContouring>();
        debris.AddComponent<DebrisCleanup>();
        //var dc = debris.AddComponent<DualContouring>();
        //
        dc.MDims = MDims;
        dc.IsoLevel = IsoLevel;

        dc.Density = new float[MDims.x, MDims.y, MDims.z];

        
        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {
                    dc.Density[x, y, z] = -100;
                }
        

        // Copy ONLY this island
        foreach (var voxel in island)
        {
            //print(voxel);
            dc.Density[voxel.x, voxel.y, voxel.z] = Density[voxel.x, voxel.y, voxel.z];

            // Remove from original
            Density[voxel.x, voxel.y, voxel.z] = -100f;
        }

        dc.GenerateMesh();
        GenerateMesh();
        // Add physics
        var rb = debris.AddComponent<Rigidbody>();
        //rb.mass = island.Count * 0.01f;
        if (!debris.GetComponent<BoxCollider>())
            debris.AddComponent<BoxCollider>();
        debris.GetComponent<BoxCollider>().size /= 10;
        rb.mass = 1f;
        rb.AddForce((Vector3.up + Vector3.right) * 2 , ForceMode.Impulse);
    }







    /// ////////////ACTIONS//////////////////////////




    public void Slice(Vector3 StartPos, Vector3 EndPos)
    {



        float PositiveSide = 0f;
        float NegativeSide = 0f;


        StartPos = transform.InverseTransformPoint(StartPos);
        EndPos = transform.InverseTransformPoint(EndPos);

        Vector3 camForward = transform.InverseTransformDirection(Camera.main.transform.forward);

        Vector3 screenDir = Vector3.ProjectOnPlane(EndPos - StartPos, camForward).normalized;
        Vector3 PlaneNormal = Vector3.Cross(screenDir, camForward).normalized;

        UnityEngine.Plane Plane = new UnityEngine.Plane(PlaneNormal, StartPos);



        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {
                    float d = Density[x,y,z];

                    if (d > IsoLevel)
                    {
                        Vector3 Pos = new Vector3(x, y, z);

                        float PlaneDistance = Plane.GetDistanceToPoint(Pos);

                        if (PlaneDistance > 0f) PositiveSide++;
                        else NegativeSide++;
                    }
                }

        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {
                    Vector3 Pos = new Vector3(x, y, z);

                    float PlaneDistance = Plane.GetDistanceToPoint(Pos);
                    
                    if (Mathf.Abs(PlaneDistance) < 1f)
                    {
                        Density[x, y, z] = -100;
                    }
                    /*
                    print(PlaneDistance);
                    if (PositiveSide > NegativeSide)
                        Density[x, y, z ] = Mathf.Min(Density[x,y,z], PlaneDistance);
                    else
                        Density[x, y, z] = Mathf.Min(Density[x, y, z], -PlaneDistance);
                    */
                    
                }
        GenerateMesh();
        var islands = FindIslands();
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

            CreateDebris(island);
        }
    }
    
    


    public void ApplyTantoCut(Vector3 Centre, Vector3 HalfSize, Quaternion Rotation)
    {
        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {

                    Vector3 Local = Quaternion.Inverse(Rotation) *
                                    (new Vector3(x + 0.0f, y + 0.0f, z + 0.0f) - Centre);
                    if (Mathf.Abs(Local.x) < HalfSize.x &&
                        Mathf.Abs(Local.y) < HalfSize.y &&
                        Mathf.Abs(Local.z) < HalfSize.z)
                    {
                        Density[x, y, z] = -100f;
                    }
                }

        Destroy(GameObject.Find("TantoCut"));
        GenerateMesh();
    }

    

}