using UnityEngine;
using Unity;
using System.Collections;
using System.Collections.Generic;
using EzySlice;
using static UnityEditor.Searcher.SearcherWindow.Alignment;


public class VoxelMarble : MonoBehaviour
{

    public Vector3 MarbleDimensions = new Vector3(32, 48, 32);

    bool[,,] Voxels;

    MeshFilter MeshFilter;

    void Start()
    {

        //Create Solid Block of Marble with X Y Z dimensions determined above

        Voxels = new bool[Mathf.RoundToInt(MarbleDimensions.x), Mathf.RoundToInt(MarbleDimensions.y), Mathf.RoundToInt(MarbleDimensions.z)];
        MeshFilter = new MeshFilter();

        for(int x=0;  x<MarbleDimensions.x; x++)
            for (int y = 0; y < MarbleDimensions.y; y++)
                for (int z = 0; z < MarbleDimensions.z; z++)
                    Voxels[x, y, z] = true;
        
        GenerateMesh();
    }

    void Update()
    {
        
    }

    void GenerateMesh()
    {

        List<Vector3> Vertices = new List<Vector3>();
        List<int> Triangles = new List<int>();

        for (int x = 0; x < MarbleDimensions.x; x++)
            for (int y = 0; y < MarbleDimensions.y; y++)
                for (int z = 0; z < MarbleDimensions.z; z++)
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

    }

    void AddFace(Vector3 Pos, Vector3 Dir, List<Vector3> Vertices, List<int> Triangles)
    {

        int Index = Vertices.Count;

        Vertices.Add(Pos);
        Vertices.Add(Pos + Vector3.up);
        Vertices.Add(Pos + Vector3.right);
        Vertices.Add(Pos + Vector3.up + Vector3.right);



    }
    

    



}
