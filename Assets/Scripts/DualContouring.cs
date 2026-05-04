using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Unity.VisualScripting;

public class DualContouring : MonoBehaviour
{
    public Vector3Int MDims = new Vector3Int(64, 96, 64);
    public float IsoLevel = 0;

    public float[,,] Density;
    bool[,,] Visited;

    MeshFilter MeshFilter;
    Mesh Mesh;

    public GameObject Sword;
    public GameObject Target;
    public GameObject VisibleTarget;

    List<Vector3> Vertices = new List<Vector3>();
    List<int> Triangles = new List<int>();

    private Vector3[] PointsBuffer = new Vector3[12];
    private Vector3[] NormalsBuffer = new Vector3[12];

    [ThreadStatic] private static float[] t_Cube;
    [ThreadStatic] private static Vector3[] t_Points;
    [ThreadStatic] private static Vector3[] t_Normals;

    private Mesh OutlineMesh;
    public Material OutlineMaterial;

    void Start()
    {
        Density = new float[MDims.x, MDims.y, MDims.z];
        Visited = new bool[MDims.x, MDims.y, MDims.z];
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
                if (VisibleTarget.activeSelf)
                { Target.SetActive(false); VisibleTarget.SetActive(false); }
                else if (!Target.activeSelf)
                    Target.SetActive(true);
                else if (!VisibleTarget.activeSelf)
                    VisibleTarget.SetActive(true);

            




        }
    }

    private void FixedUpdate()
    {
        if (transform.parent)
        {
            if (Input.GetKey(KeyCode.E))
            {

                transform.parent.Rotate(0, 1, 0);
                if (Target.transform.childCount > 0 && Target.tag != "ParentRotate")
                { Target.transform.GetChild(0).Rotate(0, 1, 0); return; }
                Target.transform.Rotate(0, 1, 0);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                transform.parent.Rotate(0, -1, 0);
                if (Target.transform.childCount > 0 && Target.tag != "ParentRotate")
                { Target.transform.GetChild(0).Rotate(0, -1, 0); return; }
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


    public void GenerateMesh()
    {
        Vertices.Clear();
        Triangles.Clear();

        int[,,] VertexIndices = new int[MDims.x, MDims.y, MDims.z];
        int totalCells = (MDims.x - 1) * (MDims.y - 1) * (MDims.z - 1);

        // Compute vertices in parallel on background threads
        Vector3[] vertexResults = new Vector3[totalCells];
        bool[] vertexValid = new bool[totalCells];

        Parallel.For(0, MDims.x - 1, x =>
        {
            for (int y = 0; y < MDims.y - 1; y++)
                for (int z = 0; z < MDims.z - 1; z++)
                {
                    int idx = x * (MDims.y - 1) * (MDims.z - 1) + y * (MDims.z - 1) + z;
                    Vector3 vertex;
                    if (ProcessCellThreadSafe(x, y, z, out vertex))
                    {
                        vertexResults[idx] = vertex;
                        vertexValid[idx] = true;
                    }
                }
        });

        // Back on main thread: collect valid vertices and build index map
        for (int x = 0; x < MDims.x - 1; x++)
            for (int y = 0; y < MDims.y - 1; y++)
                for (int z = 0; z < MDims.z - 1; z++)
                {
                    int idx = x * (MDims.y - 1) * (MDims.z - 1) + y * (MDims.z - 1) + z;
                    if (vertexValid[idx])
                    {
                        VertexIndices[x, y, z] = Vertices.Count;
                        Vertices.Add(vertexResults[idx]);
                    }
                    else
                        VertexIndices[x, y, z] = -1;
                }

        for (int x = 1; x < MDims.x - 2; x++)
            for (int y = 1; y < MDims.y - 2; y++)
                for (int z = 1; z < MDims.z - 2; z++)
                    BuildQuads(x, y, z, VertexIndices, Triangles);

        if (Mesh == null) { Mesh = new Mesh(); Mesh.MarkDynamic(); }
        Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Mesh.Clear();
        Mesh.SetVertices(Vertices);
        Mesh.SetTriangles(Triangles, 0);
        //Mesh.RecalculateNormals();

        if (name != "Marble")
        {
            DestroyImmediate(GetComponent<MeshFilter>());
            MeshFilter = gameObject.AddComponent<MeshFilter>();
        }

        MeshFilter.mesh = Mesh;

        ApplyOutline();
    }

    void ApplyOutline()
    {
        Transform outlineTransform = transform.Find("Outline");
        GameObject outlineObj;
        MeshFilter outlineMF;

        if (outlineTransform == null)
        {
            outlineObj = new GameObject("Outline");
            outlineObj.transform.SetParent(transform, false);
            outlineObj.transform.localPosition = Vector3.zero;
            outlineObj.transform.localRotation = Quaternion.identity;
            outlineObj.transform.localScale = Vector3.one * 1.02f;
            Vector3 meshCentre = Mesh.bounds.center;
            outlineObj.transform.localPosition = meshCentre - meshCentre * 1.02f;
            outlineMF = outlineObj.AddComponent<MeshFilter>();

            var mr = outlineObj.AddComponent<MeshRenderer>();
            mr.material = OutlineMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }
        else
        {
            outlineObj = outlineTransform.gameObject;
            outlineMF = outlineObj.GetComponent<MeshFilter>();
        }

        // Always re-initialise OutlineMesh if lost (new instance, domain reload, etc.)
        if (OutlineMesh == null)
        {
            OutlineMesh = new Mesh();
            OutlineMesh.MarkDynamic();
            OutlineMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            outlineMF.mesh = OutlineMesh;
        }

        int[] invertedTris = new int[Triangles.Count];
        for (int i = 0; i < Triangles.Count; i += 3)
        {
            invertedTris[i] = Triangles[i];
            invertedTris[i + 1] = Triangles[i + 2];
            invertedTris[i + 2] = Triangles[i + 1];
        }

        OutlineMesh.Clear();
        OutlineMesh.SetVertices(Vertices);
        OutlineMesh.SetTriangles(invertedTris, 0);
        OutlineMesh.RecalculateNormals();
    }

    // Thread-safe version of ProcessCell — no shared buffers, all locals
    bool ProcessCellThreadSafe(int x, int y, int z, out Vector3 Vertex)
    {
        //float[] Cube = new float[8];
        // First call on this thread: allocates. Every subsequent call: reuses.
        if (t_Cube == null) t_Cube = new float[8];
        if (t_Points == null) t_Points = new Vector3[12];
        if (t_Normals == null) t_Normals = new Vector3[12];

        // Now safe to use — no other thread touches these
        float[] Cube = t_Cube;
        Vector3[] Points = t_Points;
        Vector3[] Normals = t_Normals;
        int Count = 0;
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

        // Stack-allocated fixed size — avoids List<> heap allocation per cell
        //Vector3[] Points = new Vector3[12];
        //Vector3[] Normals = new Vector3[12];

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
                Points[Count] = Vector3.Lerp(Point1, Point2, t);
                Normals[Count] = CalculateNormal(Points[Count]);
                Count++;
            }
        }

        if (Count == 0)
        {
            Vertex = Vector3.zero;
            return false;
        }

        Vertex = SolveQEFThreadSafe(Points, Normals, Count, x, y, z);
        return true;
    }

    // Thread-safe QEF — takes array + count instead of List<>
    Vector3 SolveQEFThreadSafe(Vector3[] Points, Vector3[] Normals, int Count, int CellX, int CellY, int CellZ)
    {
        Vector3 MassPoint = Vector3.zero;
        for (int i = 0; i < Count; i++)
            MassPoint += Points[i];
        MassPoint /= Count;

        float ATA00 = 0, ATA01 = 0, ATA02 = 0;
        float ATA11 = 0, ATA12 = 0, ATA22 = 0;
        float ATb0 = 0, ATb1 = 0, ATb2 = 0;

        for (int i = 0; i < Count; i++)
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

        Result.x = Mathf.Clamp(Result.x, CellX, CellX + 1f);
        Result.y = Mathf.Clamp(Result.y, CellY, CellY + 1f);
        Result.z = Mathf.Clamp(Result.z, CellZ, CellZ + 1f);

        return Result;
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


    public List<List<Vector3Int>> FindIslands(int minX, int MaxX, int minY, int MaxY, int minZ, int MaxZ)
    {
        System.Array.Clear(Visited, 0, Visited.Length);

        List<List<Vector3Int>> islands = new List<List<Vector3Int>>();

        for (int x = minX; x <= MaxX; x++)
            for (int y = minY; y <= MaxY; y++)
                for (int z = minZ; z <= MaxZ; z++)
                {
                    if (Visited[x, y, z]) continue;
                    if (Density[x, y, z] <= IsoLevel) continue;

                    List<Vector3Int> island = new List<Vector3Int>();
                    Queue<Vector3Int> queue = new Queue<Vector3Int>();

                    queue.Enqueue(new Vector3Int(x, y, z));
                    Visited[x, y, z] = true;

                    while (queue.Count > 0)
                    {
                        var current = queue.Dequeue();
                        island.Add(current);

                        foreach (var dir in Directions)
                        {
                            var next = current + dir;

                            if (next.x < 0 || next.y < 0 || next.z < 0 ||
                                next.x >= MDims.x || next.y >= MDims.y || next.z >= MDims.z)
                                continue;

                            if (Visited[next.x, next.y, next.z]) continue;
                            if (Density[next.x, next.y, next.z] <= IsoLevel) continue;

                            Visited[next.x, next.y, next.z] = true;
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
        GameObject debris = new GameObject("Debris");

        debris.transform.position = transform.position;
        debris.transform.rotation = transform.rotation;
        debris.transform.localScale = transform.localScale;
        debris.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
        debris.AddComponent<DebrisCleanup>();

        var dc = debris.AddComponent<DualContouring>();
        dc.OutlineMaterial = OutlineMaterial;
        dc.MDims = MDims;
        dc.IsoLevel = IsoLevel;

        dc.Density = new float[MDims.x, MDims.y, MDims.z];

        // clear
        for (int x = 0; x < MDims.x; x++)
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                    dc.Density[x, y, z] = -100f;

        // copy island
        foreach (var voxel in island)
        {
            dc.Density[voxel.x, voxel.y, voxel.z] = Density[voxel.x, voxel.y, voxel.z];
            Density[voxel.x, voxel.y, voxel.z] = -100f;
        }

        dc.GenerateMesh();

        var rb = debris.AddComponent<Rigidbody>();
        debris.AddComponent<BoxCollider>();

        rb.mass = 1f;
        rb.AddForce(UnityEngine.Random.onUnitSphere * 10f, ForceMode.Impulse);
    }








    /// ////////////ACTIONS//////////////////////////


    public void Slice(Vector3 StartPos, Vector3 EndPos)
    {
        StartCoroutine(SliceCoroutine(StartPos, EndPos));
    }

    IEnumerator SliceCoroutine(Vector3 StartPos, Vector3 EndPos)
    {
        StartPos = transform.InverseTransformPoint(StartPos);
        EndPos = transform.InverseTransformPoint(EndPos);

        Vector3 camForward = transform.InverseTransformDirection(Camera.main.transform.forward);
        Vector3 screenDir = Vector3.ProjectOnPlane(EndPos - StartPos, camForward).normalized;
        Vector3 PlaneNormal = Vector3.Cross(screenDir, camForward).normalized;

        UnityEngine.Plane Plane = new UnityEngine.Plane(PlaneNormal, StartPos);


        // Carve pass — spread across frames by yielding every few rows
        for (int x = 0; x < MDims.x; x++)
        {
            for (int y = 0; y < MDims.y; y++)
                for (int z = 0; z < MDims.z; z++)
                {
                    float PlaneDistance = Plane.GetDistanceToPoint(new Vector3(x, y, z));
                    if (Mathf.Abs(PlaneDistance) < 1f)
                        Density[x, y, z] = -100f;
                }

            // Yield every column to spread load across frames
            if (x % 1 == 0) yield return null;
        }

        var islands = FindIslands(0, MDims.x - 1, 0, MDims.y - 1, 0, MDims.z - 1);
        yield return null;

        if (islands.Count > 0)
        {
            Vector3 targetLocal = transform.InverseTransformPoint(Target.transform.position);
            Vector3 targetVoxel = new Vector3(
                targetLocal.x + MDims.x * 0.5f,
                targetLocal.y + MDims.y * 0.5f,
                targetLocal.z + MDims.z * 0.5f
            );

            List<Vector3Int> mainIsland = islands[0];
            float bestDist = float.MaxValue;

            foreach (var island in islands)
            {
                // Average position of all voxels in this island
                Vector3 centroid = Vector3.zero;
                foreach (var voxel in island)
                    centroid += new Vector3(voxel.x, voxel.y, voxel.z);
                centroid /= island.Count;
                print(centroid);
                print(targetVoxel/2);
                float dist = Vector3.Distance(centroid, targetVoxel/2);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    mainIsland = island;
                }
            }

            foreach (var island in islands)
            {
                if (island == mainIsland) continue;
                CreateDebris(island);
                yield return null;
            }
        }

        // Run heavy mesh computation on background thread,
        // then apply to mesh on main thread
        var vertexResults = new Vector3[(MDims.x - 1) * (MDims.y - 1) * (MDims.z - 1)];
        var vertexValid = new bool[(MDims.x - 1) * (MDims.y - 1) * (MDims.z - 1)];

        var task = Task.Run(() =>
        {
            Parallel.For(0, MDims.x - 1, x =>
            {
                for (int y = 0; y < MDims.y - 1; y++)
                    for (int z = 0; z < MDims.z - 1; z++)
                    {
                        int idx = x * (MDims.y - 1) * (MDims.z - 1) + y * (MDims.z - 1) + z;
                        Vector3 vertex;
                        if (ProcessCellThreadSafe(x, y, z, out vertex))
                        {
                            vertexResults[idx] = vertex;
                            vertexValid[idx] = true;
                        }
                    }
            });
        });

        // Wait for background task without blocking main thread
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
        {
            UnityEngine.Debug.LogException(task.Exception);
            yield break;
        }

        // Collect results and build mesh back on main thread
        Vertices.Clear();
        Triangles.Clear();
        int[,,] VertexIndices = new int[MDims.x, MDims.y, MDims.z];

        for (int x = 0; x < MDims.x - 1; x++)
            for (int y = 0; y < MDims.y - 1; y++)
                for (int z = 0; z < MDims.z - 1; z++)
                {
                    int idx = x * (MDims.y - 1) * (MDims.z - 1) + y * (MDims.z - 1) + z;
                    if (vertexValid[idx])
                    {
                        VertexIndices[x, y, z] = Vertices.Count;
                        Vertices.Add(vertexResults[idx]);
                    }
                    else
                        VertexIndices[x, y, z] = -1;
                }

        for (int x = 1; x < MDims.x - 2; x++)
            for (int y = 1; y < MDims.y - 2; y++)
                for (int z = 1; z < MDims.z - 2; z++)
                    BuildQuads(x, y, z, VertexIndices, Triangles);

        if (Mesh == null) Mesh = new Mesh();
        Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Mesh.Clear();
        Mesh.SetVertices(Vertices);
        yield return null;
        Mesh.SetTriangles(Triangles, 0);
        yield return null;
        //Mesh.RecalculateNormals();
        MeshFilter.mesh = Mesh;
        ApplyOutline();
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