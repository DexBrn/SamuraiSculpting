using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSGUsageExample : MonoBehaviour
{
    public GameObject lhs;

    public GameObject rhs;

    public CSG.BooleanOp Operation;

    public Material material;

    GameObject Temp;

    public void Perform()
    {
        Model result = CSG.Perform(Operation,lhs, rhs);
        var composite = new GameObject();
        composite.AddComponent<MeshFilter>().sharedMesh = result.mesh;
        result.materials.Add(material);
        composite.AddComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();
        composite.name = Operation.ToString() + " Object"; 
    }


    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            Perform();
            Destroy(lhs);
            Destroy(rhs);
            Temp.transform.position = new Vector3 (0.0f,0.0f,0.0f);


        }


    }





}
