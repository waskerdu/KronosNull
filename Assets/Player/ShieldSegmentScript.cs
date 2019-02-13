using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSegmentScript : MonoBehaviour {
    Mesh myMesh;
    Mesh mesh2;
    MeshFilter myMeshFilter;
    void SetMesh(GameObject source)
    {
        myMesh = source.GetComponent<MeshFilter>().sharedMesh;
        //mesh2 = Instantiate(myMesh);
        //source.GetComponent<MeshFilter>().sharedMesh = mesh2;
        myMeshFilter = GetComponent<MeshFilter>();
        myMeshFilter.sharedMesh = myMesh;
        
        //SetColor(Color.red);
    }

    void SetSegmentColor(Color col)
    {
        GetComponent<MeshRenderer>().material.SetColor("_Color",col);
        //GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", col);
        //GetComponent<MeshRenderer>().material.SetColor()
    }


	void Start ()
    {
        
	}
	
	void Update ()
    {
		
	}
}
