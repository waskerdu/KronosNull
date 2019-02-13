using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelGeneration
{
    enum levelType
    {
        box,
        crossedBox,
        classic
    }

    public enum poiType
    {
        regular,
        shield,
        ammo,
        spawn
    }

    public struct PointOfInterest
    {
        public GameObject me;
        public int index;
        public List<int> visiblePoints;
        public Vector3 position;
        public float weight;
        public poiType type;

        public PointOfInterest(GameObject _me, int _index, poiType _type, float _weight = 0)
        {
            me = _me;
            index = _index;
            visiblePoints = new List<int>();
            position = me.transform.position;
            weight = _weight;
            type = _type;
        }

        public void AddPoint(PointOfInterest newPoint)
        {
            visiblePoints.Add(newPoint.index);
        }

        public void AddPoint(int newPoint)
        {
            visiblePoints.Add(newPoint);
        }
    }

    class PathfindData
    {
        public List<int> parentList;
        public List<float> distanceList;
    }

    public class NavSystem
    {
        public List<PointOfInterest> pointsOfInterest = new List<PointOfInterest>();
        Ray ray = new Ray();
        RaycastHit hit;
        List<int> visList = new List<int>();//what is this for again? used line 310 and others
        List<List<PathfindData>> data;// = new List<List<PathfindData>>();
        List<int> normalPoi = new List<int>();

        public void ReadyData()
        {
            if (data != null) { data.Clear();}
            else { data = new List<List<PathfindData>>(); }
            for (int i = 0; i < pointsOfInterest.Count; i++)
            {
                data.Add(new List<PathfindData>());
                for (int f = 0; f < pointsOfInterest.Count; f++)
                {
                    data[i].Add(new PathfindData());
                }
            }
        }

        public int FindNode(Vector3 position)
        {
            float minDistance = float.MaxValue;
            int startNode = -1;
            foreach (PointOfInterest item in pointsOfInterest)
            {
                if (Vector3.Magnitude(position - item.position) < minDistance)
                {
                    startNode = item.index;
                    minDistance = Vector3.Magnitude(position - item.position);
                }
            }
            return startNode;
        }

        public int FindEndNode(int startNode, poiType type)
        {
            float minDistance = float.MaxValue;
            int endNode = 0;
            for (int i = 0; i < pointsOfInterest.Count; i++)
            {
                if (pointsOfInterest[i].type == type)
                {
                    if (data[startNode][i].distanceList[i] < minDistance)
                    {
                        minDistance = data[startNode][i].distanceList[i];
                        endNode = i;
                    }
                }
            }
            return endNode;
        }

        public int FindRandomNode()
        {
            //return Mathf.RoundToInt(Random.Range(0, pointsOfInterest.Count - 1));
            int hold = Random.Range(0, normalPoi.Count - 1);
            Debug.Log(hold);
            return normalPoi[hold];
        }

        public List<GameObject> GetRandomPath(Vector3 position)
        {
            int startNode = FindNode(position);
            int endNode = FindRandomNode();
            return GetPath(startNode, endNode);
        }

        public List<GameObject> GetPath(Vector3 position, poiType type)
        {
            int startNode = FindNode(position);
            int endNode = FindEndNode(startNode, type);
            return GetPath(startNode, endNode);
        }

        public List<GameObject> GetPath(Vector3 startPosition, Vector3 endPosition)
        {
            return GetPath(FindNode(startPosition), FindNode(endPosition));
        }

        public List<GameObject> GetPath(Vector3 position, int endNode = -1)
        {
            int startNode = FindNode(position);
            if (endNode == -1) { endNode = FindRandomNode(); }
            return GetPath(startNode, endNode);
        }

        public List<GameObject> GetPath(int startNode, poiType type)
        {
            return GetPath(startNode,FindEndNode(startNode, type));
        }

        public List<GameObject> GetPath(int startNode, int endNode = -1)
        {
            List<GameObject> navList = new List<GameObject>();
            if (endNode == -1) { endNode = Mathf.RoundToInt(Random.Range(0, pointsOfInterest.Count - 1)); }
            int currentNode = endNode;
            while(true)
            {
                //Debug.Log(currentNode);
                navList.Insert(0, pointsOfInterest[currentNode].me);
                currentNode = data[startNode][endNode].parentList[currentNode];
                if (currentNode == startNode) { break; }
            }
            //Debug.Log(currentNode);
            return navList;
        }

        public void CalculatePathfinding()
        {
            List<int> openList = new List<int>();
            List<float> distanceList = new List<float>();
            List<int> parentList = new List<int>();
            List<bool> checkedList = new List<bool>();
            ReadyData();

            //for each node in pointsOfInterest
            //create an openList, a closedList, a distance list, and a parent list. 
            //set distance of first node to zero and float.max to the rest
            //find item with lowest distance that is in openList
            //for each connected node
            //if it is not in openList, add it, set its parent and its distance
            //otherwise compare its distance to the distance it would have, and if the latter is lower replace it and change its parent
            for (int startNode = 0; startNode < pointsOfInterest.Count; startNode++)
            //for (int startNode = 2; startNode < 3; startNode++)
            {
                //populate regular list
                if (pointsOfInterest[startNode].type == poiType.regular) { normalPoi.Add(startNode); }
                //init data structures
                openList.Clear();
                distanceList.Clear();
                parentList.Clear();
                checkedList.Clear();
                for (int i = 0; i < pointsOfInterest.Count; i++)
                {
                    distanceList.Add(float.MaxValue);
                    parentList.Add(-1);
                    checkedList.Add(false);
                }
                openList.Add(startNode);
                distanceList[startNode] = 0;
                parentList[startNode] = startNode;
                //use d's algorithm
                int currentNode;
                float currentDistance;
                while (parentList.Contains(-1))
                {
                    //find node with smallest distance
                    currentNode = -1;
                    currentDistance = float.MaxValue;
                    foreach (int item in openList)
                    {
                        if (distanceList[item] < currentDistance && !checkedList[item])
                        {
                            currentNode = item;
                            currentDistance = distanceList[item];
                        }
                    }
                    checkedList[currentNode] = true;
                    
                    foreach (int item in pointsOfInterest[currentNode].visiblePoints)
                    {
                        currentDistance = Vector3.Magnitude(pointsOfInterest[item].position - pointsOfInterest[currentNode].position);
                        currentDistance += distanceList[currentNode];
                        if (openList.Contains(item))
                        {
                            //compare item's distance to the distance it would have, and if the latter is lower replace it and change its parent
                            if (currentDistance < distanceList[item])
                            {
                                parentList[item] = currentNode;
                                distanceList[item] = currentDistance + pointsOfInterest[item].weight;
                            }
                        }
                        else
                        {
                            //add it to openList, set its parent and its distance
                            openList.Add(item);
                            parentList[item] = currentNode;
                            distanceList[item] = currentDistance + pointsOfInterest[item].weight;
                        }
                    }
                    

                }
                //copy data and calculate paths to shortestPathTree
                currentNode = 0;
                //shortestPathTree.Add(new List<List<int>>());
                //data.Add(new List<PathfindData>());
                for (int endNode = 0; endNode < pointsOfInterest.Count; endNode++)
                {
                    //shortestPathTree[startNode].Add(new List<int>());
                    //data[startNode].Add(new PathfindData());
                    data[startNode][endNode].distanceList = new List<float>(distanceList);
                    data[startNode][endNode].parentList = new List<int>(parentList);
                    currentNode = endNode;
                    do
                    {
                        //shortestPathTree[startNode][endNode].Insert(0, currentNode);
                        currentNode = parentList[currentNode];
                    } while (currentNode != startNode);
                }
            }
        }

        public void AddPoint(GameObject obj, poiType type=poiType.regular, float weight = 0)
        {
            pointsOfInterest.Add(new PointOfInterest(obj, pointsOfInterest.Count, type, weight));
            CalculateVisiblity(pointsOfInterest.Count - 1);
        }

        public void GenerateNavMesh()
        {
            for (int i = 0; i < pointsOfInterest.Count; i++)
            {
                CalculateVisiblity(i);
            }
        }

        public bool IsConnected()
        {

            //closed list starts empty
            //open list is the first node
            //while open list count > 0
            //pick first element from openList
            //for each node connected to it
            //if it is not in openList or closedList
            //add it to openList
            //once done if closedList.count<total number of nodes return false
            //otherwise return true

            List<int> openList = new List<int>();
            List<int> closedList = new List<int>();

            openList.Add(0);

            while (openList.Count > 0)
            {
                foreach (int item in pointsOfInterest[openList[0]].visiblePoints)
                {
                    if(!openList.Contains(item) && !closedList.Contains(item))
                    {
                        openList.Add(item);
                    }
                }
                closedList.Add(openList[0]);
                openList.RemoveAt(0);
            }
            return closedList.Count == pointsOfInterest.Count;
        }

        void CalculateVisiblity(int index)
        {
            ray.origin = pointsOfInterest[index].position;
            visList.Clear();
            for (int i = 0; i < pointsOfInterest.Count; i++)
            {
                if (i != index)
                {
                    //shoot a raycast between objects. if it hits they are visible add to pointsOfInterest[index]
                    ray.direction = pointsOfInterest[i].position - ray.origin;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.transform.gameObject == pointsOfInterest[i].me)
                        {
                            visList.Add(i);
                            Debug.DrawLine(pointsOfInterest[index].position, pointsOfInterest[i].position, Color.red, 100);
                        }
                    }
                }
            }
            foreach (int item in visList)
            {
                if (!pointsOfInterest[index].visiblePoints.Contains(item)) { pointsOfInterest[index].AddPoint(item); }
                if (!pointsOfInterest[item].visiblePoints.Contains(index))
                {
                    pointsOfInterest[item].AddPoint(index);
                }
            }
        }
    }


    public struct Player
    {
        public int playerNum;
        public GameInput.Controller controller;
        public int classIndex;
        public Player(GameInput.Controller _controller, int _playerNum = 1, int _classIndex = 0)
        {
            playerNum = _playerNum;
            controller = _controller;
            classIndex = _classIndex;
        }
        public GameInput.Controller GetController() { return controller; }
    }

    class Path
    {
        //path is a series of grids arrainged in a list
        //each grid may link to another path
        List<Grid3> gridList;

    }

    struct room
    {
        public int x, y, z, xWidth, yWidth, zWidth, val;
        public room(int myX, int myY, int myZ, int myXWidth, int myYWidth, int myZWidth, int myVal)
        {
            x = myX;
            y = myY; z = myZ;
            xWidth = myXWidth;
            yWidth = myYWidth;
            zWidth = myZWidth;
            val = myVal;
        }
    }

    struct levelData
    {
        public int width, minRooms, minWidth, maxWidth;
        public levelData(int myWidth, int myMinRooms, int myMinWidth, int myMaxWidth)
        {
            width = myWidth;
            minRooms = myMinRooms;
            minWidth = myMinWidth;
            maxWidth = myMaxWidth;
        }
    }

    struct cell
    {
        public int x, y, z, val;
        public cell(int myX = 0, int myY = 0, int myZ = 0, int myVal = 0)
        {
            x = myX;
            y = myY;
            z = myZ;
            val = myVal;
        }
    }

    struct voxelPlus
    {
        public cell myCell;
        public GameObject vox;
        public voxelPlus(GameObject myVox, cell pos)
        {
            myCell = pos;
            vox = myVox;
        }
    }

    class VoxelMap
    {
        Grid3 myGrid;
        Grid3 gridCopy;
        int mapWidth;
        GameObject voxel;
        List<room> connectorList;
        List<voxelPlus> voxelList;
        void conway()
        {
            int i, f, g, index;
            index = 0;
            myGrid.data.CopyTo(gridCopy.data, 0);//updates the copy of the grid for double buffering
            for (i = 0; i < mapWidth; i++)
            {
                for (f = 0; f < mapWidth; f++)
                {
                    for (g = 0; g < mapWidth; g++)
                    {
                        if (gridCopy.data[index] == 0)
                        {
                            if (gridCopy.adj(i, f, g, 1) > 1 && gridCopy.adjEdges(i, f, g) == 0) { myGrid.data[index] = 1; }
                        }
                        index++;
                    }
                }
            }
        }
        public void DeleteCell(int x, int y, int z)
        {
            //only removes cell, does not yet add neighbors
            myGrid.setVal(x, y, z, 1);
            for (int i = 0; i < voxelList.Count; i++)
            {
                voxelPlus vox = voxelList[i];
                if (x == vox.myCell.x && y == vox.myCell.y && z == vox.myCell.z)
                {
                    Object.Destroy(vox.vox);
                    voxelList.RemoveAt(i);
                    break;
                }
            }
        }
        public Vector3 startPosition()
        {
            cell myCell = new cell();
            myGrid.cellValueExistsReplace(ref myCell, 1, 2);
            Vector3 myPos = new Vector3(myCell.x * voxel.transform.localScale.x, myCell.y * voxel.transform.localScale.y, myCell.z * voxel.transform.localScale.z);
            return myPos;
        }
        public Vector3 freePosition()
        {
            List<int> openSpace = new List<int>();
            int i;
            int selected = 0;
            for (i = 0; i < myGrid.dataSize; i++)
            {
                if (myGrid.data[i] == 1)
                {
                    if (myGrid.adj(i, 1) == 6) { openSpace.Add(i); }
                }
            }
            selected = irandom(0, openSpace.Count);
            cell myCell;
            myCell = myGrid.getCell(openSpace[selected]);
            Vector3 myPos = new Vector3(myCell.x * voxel.transform.localScale.x, myCell.y * voxel.transform.localScale.y, myCell.z * voxel.transform.localScale.z);
            return myPos;
        }
        public cell startCell()
        {
            cell myCell = new cell();
            myGrid.cellValueExistsReplace(ref myCell, 2, 4);
            return myCell;
        }
        public void buildLevel()
        {
            //switch (my)
            int thickness = 6;
            int passageThickness = 4;
            myGrid.setRegion(1, 1, 1, mapWidth - 2, mapWidth - 2, mapWidth - 2, 1);
            myGrid.setRegion(mapWidth / 2 - thickness / 2, 1, mapWidth / 2 - thickness / 2, thickness, mapWidth - 2, thickness, 0);
            myGrid.setRegion(1, mapWidth / 2 - thickness / 2, mapWidth / 2 - thickness / 2, mapWidth - 2, thickness, thickness, 0);
            myGrid.setRegion(mapWidth / 2 - thickness / 2, mapWidth / 2 - thickness / 2, 1, thickness, thickness, mapWidth - 2, 0);
            //myGrid.setRegion(mapWidth / 4 - passageThickness / 2, 1, mapWidth / 2 - passageThickness / 2, passageThickness, mapWidth - 2, passageThickness, 1);
            //myGrid.setRegion(1, 1, 1, mapWidth - 2, 4, mapWidth - 2, 1);
        }
        public void roomCull(float reduction = 1.0f)
        {
            int testI;
            room tempRoom;
            int endSize = (int)(connectorList.Count * reduction);
            while (connectorList.Count > endSize)
            {
                testI = irandom(0, connectorList.Count);
                tempRoom = connectorList[testI];
                tempRoom.val = 0;
                connectorList.RemoveAt(testI);
                myGrid.setRegion(tempRoom);
                tempRoom.val = 1;
                if (isConnected() == false) { myGrid.setRegion(tempRoom); }
            }
        }
        public bool isConnected()
        {
            bool output = true;
            int val = 1;
            int fillVal = 2;
            List<cell> openList = new List<cell>();
            cell tempCell = new cell();
            myGrid.data.CopyTo(gridCopy.data, 0);//updates the copy of the grid for double buffering
            if (gridCopy.cellValueExists(ref tempCell, val))
            {
                openList.Add(tempCell);
                gridCopy.setVal(tempCell.x, tempCell.y, tempCell.z, fillVal);
            }
            while (openList.Count > 0)
            {
                tempCell = openList[0];
                addCell(ref openList, tempCell.x + 1, tempCell.y, tempCell.z, val, fillVal);
                addCell(ref openList, tempCell.x - 1, tempCell.y, tempCell.z, val, fillVal);

                addCell(ref openList, tempCell.x, tempCell.y - 1, tempCell.z, val, fillVal);
                addCell(ref openList, tempCell.x, tempCell.y + 1, tempCell.z, val, fillVal);

                addCell(ref openList, tempCell.x, tempCell.y, tempCell.z - 1, val, fillVal);
                addCell(ref openList, tempCell.x, tempCell.y, tempCell.z + 1, val, fillVal);
                openList.RemoveAt(0);
            }
            output = !gridCopy.inGrid(val);
            return output;
        }
        public void addCell(ref List<cell> myList, int x, int y, int z, int val, int fillVal)
        {
            cell nextCell = new cell();
            if (gridCopy.queryCell(x, y, z, val, ref nextCell))
            {
                myList.Add(nextCell);
                gridCopy.data[gridCopy.getIndex(x, y, z)] = fillVal;
            }
        }
        public int irandom(int min, int max)
        {
            float fMin = (float)min;
            float fMax = (float)max;
            float fOut = Random.Range(fMin, fMax + 1);
            return Mathf.RoundToInt(fOut);

        }
        public room placeRoom(levelData myData)
        {
            int maxLeng = myData.maxWidth;
            int minLeng = myData.minWidth;
            int xLeng = irandom(minLeng, maxLeng);
            int yLeng = irandom(minLeng, maxLeng);
            int zLeng = irandom(minLeng, maxLeng);
            //assumes mapWidth is greater than maxLeng
            int x = irandom(1, mapWidth - xLeng - 1);
            int y = irandom(1, mapWidth - yLeng - 1);
            int z = irandom(1, mapWidth - zLeng - 1);
            myGrid.setRegion(x, y, z, xLeng, yLeng, zLeng, 1);
            room tempRoom = new room(x, y, z, xLeng, yLeng, zLeng, 1);
            return tempRoom;
        }
        public void cullHidden()
        {
            myGrid.data.CopyTo(gridCopy.data, 0);
            int index = 0;
            for (int i = 0; i < mapWidth; i++)
            {
                for (int f = 0; f < mapWidth; f++)
                {
                    for (int g = 0; g < mapWidth; g++)
                    {
                        if (myGrid.data[index] == 0)
                        {
                            if (gridCopy.adj(i, f, g, 0, true) == 6) { myGrid.data[index] = 3; }
                        }
                        index++;
                    }
                }
            }
        }
        public void instantiate()
        {
            int index = 0;
            float xScale = voxel.transform.localScale.x;
            float yScale = voxel.transform.localScale.y;
            float zScale = voxel.transform.localScale.z;
            for (int i = 0; i < mapWidth; i++)
            {
                for (int f = 0; f < mapWidth; f++)
                {
                    for (int g = 0; g < mapWidth; g++)
                    {
                        if (myGrid.data[index] == 0)
                        {
                            //GameObject tempObj = Object.Instantiate(voxel);
                            //tempObj.transform.position = new Vector3(i * xScale, f * yScale, g * zScale);
                        }
                        index++;
                    }
                }
            }
        }
        public void setVal(int x, int y, int z, int val) { myGrid.setVal(x, y, z, val); }//manual set val for debuggling only
        public VoxelMap(int width, GameObject myVoxel)
        {
            int seed;
            seed = (int)System.DateTime.Now.Ticks;
            seed = 30;
            Random.InitState(seed);
            //Debug.Log(seed);
            myGrid = new Grid3(width);
            gridCopy = new Grid3(width);
            mapWidth = width;
            voxel = myVoxel;
            connectorList = new List<room>();
        }
    }

    class Grid3
    {
        public int[] data;
        public int dataSize;
        int gridWidth;
        public bool cellValueExists(ref cell myCell, int val)
        {
            bool output = true;
            for (int i = 0; i < dataSize; i++)
            {
                if (data[i] == val)
                {
                    myCell = getCell(i);
                    break;
                }
            }
            return output;
        }
        public bool cellValueExistsReplace(ref cell myCell, int val, int rep)
        {
            bool output = true;
            for (int i = 0; i < dataSize; i++)
            {
                if (data[i] == val)
                {
                    myCell = getCell(i);
                    data[i] = rep;
                    break;
                }
            }
            return output;
        }
        public cell getCell(int index)
        {
            int x = index / (gridWidth * gridWidth);
            index -= x * gridWidth * gridWidth;
            int y = index / gridWidth;
            index -= y * gridWidth;
            int z = index;
            cell outCell = new cell(x, y, z, data[index]);
            return outCell;
        }
        public bool inGrid(int val)
        {
            for (int i = 0; i < dataSize; i++)
            {
                if (data[i] == val) { return true; }
            }
            return false;
        }
        public void setRegion(room myRoom) { setRegion(myRoom.x, myRoom.y, myRoom.z, myRoom.xWidth, myRoom.yWidth, myRoom.zWidth, myRoom.val); }
        public void setRegion(int x, int y, int z, int xLeng, int yLeng, int zLeng, int val)
        {
            for (int i = 0; i < xLeng; i++)
            {
                for (int f = 0; f < yLeng; f++)
                {
                    for (int g = 0; g < zLeng; g++)
                    {
                        if (onGrid(i + x, f + y, g + z)) { setVal(i + x, f + y, g + z, val); }
                    }
                }
            }
        }
        public int adj(int index, int val, bool includeEdge = false)
        {
            cell tempCell = getCell(index);
            return adj(tempCell.x, tempCell.y, tempCell.z, val, includeEdge);
        }
        public int adj(int x, int y, int z, int val, bool includeEdge = false)
        {
            int count = 0;
            if (query(x - 1, y, z, val)) { count++; }
            if (query(x + 1, y, z, val)) { count++; }

            if (query(x, y - 1, z, val)) { count++; }
            if (query(x, y + 1, z, val)) { count++; }

            if (query(x, y, z - 1, val)) { count++; }
            if (query(x, y, z + 1, val)) { count++; }
            if (includeEdge) { count += adjEdges(x, y, z); }
            return count;
        }
        public bool queryCell(int x, int y, int z, int val, ref cell myCell)
        {
            bool output = onGrid(x, y, z);
            int testVal, index;
            if (output)
            {
                index = getIndex(x, y, z);
                testVal = data[index];
                output = testVal == val;
                if (output) { myCell = getCell(index); }
            }
            return output;
        }
        public bool query(int x, int y, int z, int val)
        {
            bool output = onGrid(x, y, z);
            if (output)
            {
                output = (getVal(x, y, z) == val);
            }
            return output;
        }
        public int adjEdges(int x, int y, int z)
        {
            int edges = 0;
            if (x == 0 || x == gridWidth - 1) { edges++; }
            if (y == 0 || y == gridWidth - 1) { edges++; }
            if (z == 0 || z == gridWidth - 1) { edges++; }
            return edges;
        }
        public bool onGrid(int x, int y, int z)
        {
            if (x < 0 || x > gridWidth - 1) { return false; }
            else if (y < 0 || y > gridWidth - 1) { return false; }
            else if (z < 0 || z > gridWidth - 1) { return false; }
            else { return true; }
        }
        public int getWidth() { return gridWidth; }
        public void setVal(int x, int y, int z, int val)
        {
            data[getIndex(x, y, z)] = val;
        }
        public int getVal(int x, int y, int z)
        {
            return data[getIndex(x, y, z)];
        }
        public int getIndex(int x, int y, int z)
        {
            int index = x * gridWidth * gridWidth;
            index += y * gridWidth;
            index += z;
            return index;
        }
        public void clear(int val)
        {
            for (int i = 0; i < dataSize; i++)
            {
                data[i] = val;
            }
        }
        public Grid3(int width, int clearVal = 0)
        {
            dataSize = width * width * width;
            data = new int[dataSize];
            clear(clearVal);
            gridWidth = width;
        }
    }

    class Grid
    {
        int[] data;
        int width, height, depth;

        public Grid(int _width, int _height = 0, int _depth = 0, int fill = 0)
        {
            width = _width;
            if (_height == 0) { height = width; }
            else { height = _height; }
            if (_depth == 0) { depth = width; }
            else { depth = _depth; }
            data = new int[width * height * depth];
            Clear(fill);
        }

        public void Clear(int val)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = val;
            }
        }

        public int GetIndex(int x, int y, int z)
        {
            int output = 0;
            output += x;
            output += y * width;
            output += z * width * height;
            return output;
        }

        public int Adg(int x, int y, int z, int val, bool edges = false)
        {
            int count = 0;
            if (Get(x + 1, y, z) == val) { count++; }
            if (Get(x - 1, y, z) == val) { count++; }
            if (Get(x, y + 1, z) == val) { count++; }
            if (Get(x, y - 1, z) == val) { count++; }
            if (Get(x, y, z + 1) == val) { count++; }
            if (Get(x, y, z - 1) == val) { count++; }
            if (edges) { count += AdjEdges(x, y, z); }
            return count;
        }

        public int AdjEdges(int x, int y, int z)
        {
            int count = 0;
            if (!OnGrid(x + 1, y, z)) { count++; }
            if (!OnGrid(x - 1, y, z)) { count++; }
            if (!OnGrid(x, y + 1, z)) { count++; }
            if (!OnGrid(x, y - 1, z)) { count++; }
            if (!OnGrid(x, y, z + 1)) { count++; }
            if (!OnGrid(x, y, z - 1)) { count++; }
            return count;
        }

        public void Set(int index, int val) { data[index] = val; }
        public void Set(int x, int y, int z, int val) { data[GetIndex(x, y, z)] = val; }
        public int Get(int index) { return data[index]; }
        public int Get(int x, int y, int z) { return data[GetIndex(x, y, z)]; }
        public bool OnGrid(int index) { return index > -1 && index < data.Length; }
        public bool OnGrid(int x, int y, int z) { return OnGrid(GetIndex(x, y, z)); }
    }
}