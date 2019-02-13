using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour {
    public int width = 10;
    GameMap.Map map;
    Mesh mesh;
    List<Vector3> vertices;// = new List<Vector3>();
    //List<Vector3> normals = new List<Vector3>();
    List<int> triangles;// = new List<int>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        
        if (vertices == null) { return; }
        /*for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
        for (int i = 0; i < triangles.Count; i+=3)
        {
            Gizmos.DrawLine(vertices[triangles[i]], vertices[triangles[i + 1]]);
            Gizmos.DrawLine(vertices[triangles[i + 1]], vertices[triangles[i + 2]]);
            Gizmos.DrawLine(vertices[triangles[i + 2]], vertices[triangles[i]]);
        }*/
    }

    void Start ()
    {
        //mesh = GetComponent<MeshFilter>().mesh;
        map = new GameMap.Map(width);
        map.MakeBox();
        
        map.CullHidden();
        //MeshGenerator.CubeMesh cubeMesh = new MeshGenerator.CubeMesh(ref map);
        MeshGenerator.MarchingMesh marchingMesh = new MeshGenerator.MarchingMesh(ref map);
        //vertices = cubeMesh.vertexList;
        //triangles = cubeMesh.triangleList;
        //vertices = marchingMesh.vertexList;
        vertices = marchingMesh.debugList;
        triangles = marchingMesh.triangleList;
        //vertices = map.GetCubeVertices();
        //mesh = map.GetCubeMesh(ref vertices, ref normals, ref triangles);
        //GetComponent<MeshFilter>().mesh = cubeMesh.GetCubeMesh();
        GetComponent<MeshFilter>().mesh = marchingMesh.GetMarchingMesh();
        //triangles = map.GetTriangleList();
        //GetComponent<MeshFilter>().mesh.normals
        //GetComponent<MeshFilter>().mesh.vertices = vertices.ToArray();
        //GetComponent<MeshFilter>().mesh.triangles = triangles.ToArray();
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }
	
	void Update ()
    {
		
	}
}
