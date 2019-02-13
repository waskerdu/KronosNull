using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScript : MonoBehaviour {
    public GameObject chunk;
    public List<GameObject> chunks = new List<GameObject>();
    public int arenaWidth = 20;
    public int arenaHeight = 20;
    public int arenaDepth = 20;
    public  MapType mapType = MapType.cross;
    //public int mapTypeIndex = 0;
    public int mapSizeIndex = 0;
    public int thickness = 2;
    public float smoothness = 0.5f;
    public bool cavesque = false;
    public int minRooms = 1;
    public int minWidth = 4;
    public int maxWidth = 4;
    GameMap.ClusterMap myMap;

    public enum MapType
    {
        cross,
        tangle,
        warren,
        pillars,
        cube,
        sphere,
        race,
        tutorial,
    }

    void MakeMapDimensions()
    {
        //mapType = (MapType)mapTypeIndex;
        arenaHeight = 0;
        arenaDepth = 0;
        switch (mapType)
        {
            case MapType.cross:
                switch (mapSizeIndex)
                {
                    case 0:
                        arenaWidth = 20;
                        thickness = 4;
                        break;
                    case 1:
                        arenaWidth = 40;
                        thickness = 6;
                        break;
                    case 2:
                        arenaWidth = 60;
                        thickness = 8;
                        break;
                    case 3:
                        arenaWidth = 100;
                        thickness = 10;
                        break;
                    default:
                        break;
                }
                break;
            case MapType.tangle:
                switch (mapSizeIndex)
                {
                    case 0:
                        arenaWidth = 20;
                        thickness = 4;
                        break;
                    case 1:
                        arenaWidth = 40;
                        thickness = 6;
                        break;
                    case 2:
                        arenaWidth = 60;
                        thickness = 8;
                        break;
                    case 3:
                        arenaWidth = 100;
                        thickness = 10;
                        break;
                    default:
                        break;
                }
                break;
            case MapType.warren:
                switch (mapSizeIndex)
                {
                    case 0:
                        arenaWidth = 20;
                        thickness = 4;
                        break;
                    case 1:
                        arenaWidth = 40;
                        thickness = 6;
                        break;
                    case 2:
                        arenaWidth = 60;
                        thickness = 8;
                        break;
                    case 3:
                        arenaWidth = 100;
                        thickness = 10;
                        break;
                    default:
                        break;
                }
                break;
            case MapType.pillars:
                switch (mapSizeIndex)
                {
                    case 0:
                        arenaWidth = 20;
                        thickness = 4;
                        break;
                    case 1:
                        arenaWidth = 40;
                        thickness = 6;
                        break;
                    case 2:
                        arenaWidth = 60;
                        thickness = 8;
                        break;
                    case 3:
                        arenaWidth = 100;
                        thickness = 10;
                        break;
                    default:
                        break;
                }
                break;
            case MapType.cube:
                switch (mapSizeIndex)
                {
                    case 0:
                        arenaWidth = 20;
                        break;
                    case 1:
                        arenaWidth = 40;
                        break;
                    case 2:
                        arenaWidth = 60;
                        break;
                    case 3:
                        arenaWidth = 100;
                        break;
                    default:
                        break;
                }
                break;
            case MapType.sphere:
                switch (mapSizeIndex)
                {
                    case 0:
                        arenaWidth = 20;
                        break;
                    case 1:
                        arenaWidth = 40;
                        break;
                    case 2:
                        arenaWidth = 60;
                        break;
                    case 3:
                        arenaWidth = 100;
                        break;
                    default:
                        break;
                }
                break;
            case MapType.race:
                break;
            default:
                break;
        }

        switch (mapType)
        {
            case MapType.cube:
                myMap = new GameMap.ClusterMap(arenaWidth, arenaHeight, arenaDepth);
                myMap.MakeBox();
                break;
            case MapType.cross:
                myMap = new GameMap.ClusterMap(arenaWidth, arenaHeight, arenaDepth);
                myMap.MakeCross(thickness);
                break;
            case MapType.warren:
                myMap = new GameMap.ClusterMap(arenaWidth);
                myMap.MakeCross(arenaWidth - thickness * 2);
                break;
            case MapType.sphere:
                myMap = new GameMap.ClusterMap(arenaWidth, arenaHeight, arenaDepth);
                myMap.MakeSphere();
                break;
            case MapType.pillars:
                myMap = new GameMap.ClusterMap(arenaWidth, arenaHeight, arenaDepth);
                myMap.MakePillars(thickness);
                break;
            case MapType.tangle:
                myMap = new GameMap.ClusterMap(arenaWidth, arenaHeight, arenaDepth);
                myMap.MakeTangle(thickness);
                break;
            case MapType.race:
                myMap = new GameMap.ClusterMap(6, 6, 160);
                myMap.MakeBox();
                break;
            case MapType.tutorial:
                myMap = new GameMap.ClusterMap(6, 6, 160);
                break;
            default:
                break;
        }
    }

    void Build()
    {
        MakeMapDimensions();
        myMap.CopyToMarching();
        if (cavesque) { myMap.MakeCavesque(); }
        myMap.SmoothMesh(smoothness);
        myMap.MakeChunks();
        GameObject tempChunk;
        MeshGenerator.MarchingMesh marchingMesh;
        for (int i = 0; i < myMap.chunkList.Count; i++)
        {
            tempChunk = Instantiate(chunk, transform);
            tempChunk.SetActive(true);
            tempChunk.transform.position = myMap.chunkList[i].position * transform.localScale.x;
            marchingMesh = new MeshGenerator.MarchingMesh(ref myMap.chunkList[i].marchingGrid);
            tempChunk.GetComponent<MeshFilter>().mesh = marchingMesh.GetMarchingMesh();
            tempChunk.GetComponent<MeshCollider>().sharedMesh = tempChunk.GetComponent<MeshFilter>().sharedMesh;
        }
    }

    void Rebuild()
    {

    }

    void Start () {
        Build();
	}
	
	void Update () {
		
	}
}
