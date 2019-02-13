using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 project Open World
 a major refactor allowing maps of arbitrary size
 maps will be depriciated
 object heirarchy:
 level: high level object referencing all sectors
 sector: themed collection of clusters
 cluster: responsble for local pathfinding and item spawning
 chunk: holds a single mesh and a position

 */

namespace GameMap
{
    class ClusterMap
    {
        /*cube grid key:
         * -1: impassible
         * 0: filled
         * 1: empty
         * 
        */
        Grid3i cubeGrid;
        Grid3i cubeGridCopy;
        Grid3f marchingGrid;
        Grid3f marchingGridCopy;
        public List<Chunk> chunkList;
        public List<Vector3> freePositions = new List<Vector3>();
        public List<int> vertHash = new List<int>();
        int chunkWidth = 10;

        int width, height, depth;

        public ClusterMap(int _width, int _height = 0, int _depth = 0)
        {
            if (_height == 0) { _height = _width; }
            if (_depth == 0) { _depth = _width; }
            width = _width;
            height = _height;
            depth = _depth;
            cubeGrid = new Grid3i(width, height, depth);
            cubeGridCopy = new Grid3i(width, height, depth);
            marchingGrid = new Grid3f(width + 1, height + 1, depth + 1);
            marchingGridCopy = new Grid3f(width + 1, height + 1, depth + 1);
            chunkList = new List<Chunk>();
        }

        //public Grid3i GetCubeGrid() { return cubeGrid; }

        //public Grid3f GetMarchingGrid() { return marchingGrid; }

        void PopulateFreePositions()
        {
            freePositions.Clear();
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < height; f++)
                {
                    for (int g = 0; g < depth; g++)
                    {
                        if (cubeGrid.Get(i, f, g) == 1)
                        {
                            //if (cubeGrid.Adg(i, f, g, 0) == 0) { freePositions.Add(new Vector3(i, f, g)); }
                            //Debug.Log(cubeGrid.SearchRegion(i - 1, f - 1, g - 1, 3, 3, 3, 0));
                            if (cubeGrid.SearchRegion(i - 1, f - 1, g - 1, 3, 3, 3, 0) == 0)
                            {
                                freePositions.Add(new Vector3(i + 0.5f, f + 0.5f, g + 0.5f));
                                //Debug.Log("placed");
                            }
                        }
                    }
                }
            }
        }

        public List<Vector3> GetFreePositions(int positions)
        {
            if (freePositions.Count == 0) { PopulateFreePositions(); }
            List<Vector3> output = new List<Vector3>(positions);
            float step = (float)freePositions.Count / positions;
            for (float i = 0; i < positions; i += step)
            {
                output.Add(freePositions[(int)i]);
            }
            return output;
        }

        public Vector3 FreePosition()
        {
            if (freePositions.Count == 0) { PopulateFreePositions(); }
            int randInt = Mathf.RoundToInt(Random.Range(0, freePositions.Count - 1));
            Vector3 output = freePositions[randInt];
            freePositions.RemoveAt(randInt);
            return output;
        }

        public void SmoothMesh(float smoothness)
        {
            float val;
            for (int i = 0; i < marchingGrid.width; i++)
            {
                for (int f = 0; f < marchingGrid.height; f++)
                {
                    for (int g = 0; g < marchingGrid.depth; g++)
                    {
                        //marchingGrid.Set(i, f, g, marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3));
                        val = marchingGridCopy.Get(i, f, g);
                        if (val != 0 && val != 1)
                        {
                            val = Mathf.Lerp(val, marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3), smoothness);
                            //marchingGrid.Set(i, f, g, (marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3)+marchingGridCopy.Get(i,f,g))/2) ;
                            marchingGrid.Set(i, f, g, val);
                        }
                    }
                }
            }
        }

        public void MakeSmoothCaves(float smoothness)
        {
            MakeCavesque();
            float val;
            for (int i = 0; i < marchingGrid.width; i++)
            {
                for (int f = 0; f < marchingGrid.height; f++)
                {
                    for (int g = 0; g < marchingGrid.depth; g++)
                    {
                        //marchingGrid.Set(i, f, g, marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3));
                        val = marchingGridCopy.Get(i, f, g);
                        if (val != 0 && val != 1)
                        {
                            val = Mathf.Lerp(val, marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3), smoothness);
                            //marchingGrid.Set(i, f, g, (marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3)+marchingGridCopy.Get(i,f,g))/2) ;
                            marchingGrid.Set(i, f, g, val);
                        }
                    }
                }
            }
        }

        public void MakeCavesque()
        {
            marchingGrid.CopyTo(ref marchingGridCopy);
            float val;
            bool randomize;
            for (int i = 0; i < marchingGrid.width; i++)
            {
                for (int f = 0; f < marchingGrid.height; f++)
                {
                    for (int g = 0; g < marchingGrid.depth; g++)
                    {
                        randomize = false;
                        val = marchingGrid.Get(i, f, g);
                        //if(val==1&& marchingGridCopy.AdjEdges(i, f, g) == 0) { randomize = true; }
                        if (val == 1 && marchingGrid.AdjEdges(i, f, g) == 0 && marchingGrid.Adg(i, f, g, 0) > 0) { randomize = true; }
                        //if (val == 0 && marchingGridCopy.Adg(i, f, g, 1) > 0) { randomize = true; }
                        if (randomize)
                        {
                            val = Random.Range(0.0f, 1.0f);
                            //Debug.Log(val);
                            marchingGridCopy.Set(i, f, g, val);
                        }
                    }
                }
            }
            marchingGridCopy.CopyTo(ref marchingGrid);
        }

        public void CopyToMarching()
        {
            cubeGrid.CopyToMarching(ref marchingGrid);
        }

        public void MakeBox()
        {
            cubeGrid.Clear(0);
            cubeGrid.SetRegion(1, 1, 1, width - 2, height - 2, depth - 2, 1);
        }

        public void MakeCross(int thickness)
        {
            MakeBox();
            cubeGrid.SetRegion(width / 2 - thickness / 2, height / 2 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 2 - thickness / 2, 1, depth / 2 - thickness / 2, thickness, height - 2, thickness, 0);
            cubeGrid.SetRegion(1, height / 2 - thickness / 2, depth / 2 - thickness / 2, width - 2, thickness, thickness, 0);
        }

        public void MakePillars(int thickness)
        {
            MakeBox();
            cubeGrid.SetRegion(width / 2 - thickness / 2, height / 4 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 4 - thickness / 2, height / 2 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 2 - thickness / 2, height / 4 * 3 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 4 * 3 - thickness / 2, height / 2 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
        }

        public void MakeTangle(int thickness)
        {
            MakeBox();
            cubeGrid.SetRegion(width / 2 - thickness / 2, height / 4 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 4 - thickness / 2, height / 2 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 2 - thickness / 2, height / 4 * 3 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 4 * 3 - thickness / 2, height / 2 - thickness / 2, 1, thickness, thickness, depth - 2, 0);

            cubeGrid.SetRegion(width / 2 - thickness / 2, 1, depth / 4 - thickness / 2, thickness, height - 2, thickness, 0);
            cubeGrid.SetRegion(width / 4 - thickness / 2, 1, depth / 2 - thickness / 2, thickness, height - 2, thickness, 0);
            cubeGrid.SetRegion(width / 2 - thickness / 2, 1, depth / 4 * 3 - thickness / 2, thickness, height - 2, thickness, 0);
            cubeGrid.SetRegion(width / 4 * 3 - thickness / 2, 1, depth / 2 - thickness / 2, thickness, height - 2, thickness, 0);

            cubeGrid.SetRegion(1, height / 2 - thickness / 2, depth / 4 - thickness / 2, width - 2, thickness, thickness, 0);
            cubeGrid.SetRegion(1, height / 4 - thickness / 2, depth / 2 - thickness / 2, width - 2, thickness, thickness, 0);
            cubeGrid.SetRegion(1, height / 2 - thickness / 2, depth / 4 * 3 - thickness / 2, width - 2, thickness, thickness, 0);
            cubeGrid.SetRegion(1, height / 4 * 3 - thickness / 2, depth / 2 - thickness / 2, width - 2, thickness, thickness, 0);
        }

        public void MakeSphere()
        {
            int x, y, z;
            int thickness = width / 2 - 4;
            float dis;
            float radS = thickness * thickness;
            cubeGrid.Clear(0);
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < height; f++)
                {
                    for (int g = 0; g < depth; g++)
                    {
                        x = i - width / 2;
                        y = f - height / 2;
                        z = g - depth / 2;
                        dis = x * x + y * y + z * z;
                        if (dis < radS) { cubeGrid.Set(i, f, g, 1); }
                    }
                }
            }
        }

        public void CullHidden()
        {
            cubeGrid.CopyTo(ref cubeGridCopy);
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < height; f++)
                {
                    for (int g = 0; g < depth; g++)
                    {
                        if (cubeGridCopy.Adg(i, f, g, 0) == 6) { cubeGrid.Set(i, f, g, -1); }
                    }
                }
            }
        }

        public void MakeChunks()
        {
            for (int i = 0; i < width; i+=chunkWidth)
            {
                for (int f = 0; f < height; f+=chunkWidth)
                {
                    for (int g = 0; g < depth; g+=chunkWidth)
                    {
                        chunkList.Add(new Chunk(i, f, g, chunkWidth+1, ref marchingGrid));
                    }
                }
            }
        }
    }

    class Chunk
    {
        public Grid3f marchingGrid;
        public Vector3 position;// = Vector3.zero;
        Mesh mesh;
        float val;
        public Chunk
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            (int x, int y, int z, int width, ref Grid3f copyGrid)
        {
            position = new Vector3(x, y, z);
            marchingGrid = new Grid3f(width);
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < width; f++)
                {
                    for (int g = 0; g < width; g++)
                    {
                        if (copyGrid.OnGrid(x + i, y + f, z + g)) { val = copyGrid.Get(x + i, y + f, z + g); }
                        else { val = 1;}
                        marchingGrid.Set(i, f, g, val);
                    }
                }
            }
        }
    }

    class Map
    {
        /*cube grid key:
         * -1: impassible
         * 0: filled
         * 1: empty
         * 
        */
        Grid3i cubeGrid;
        Grid3i cubeGridCopy;
        Grid3f marchingGrid;
        Grid3f marchingGridCopy;
        public List<Vector3> freePositions = new List<Vector3>();
        public List<int> vertHash = new List<int>();

        int width, height, depth;

        public Map(int _width, int _height = 0, int _depth = 0)
        {
            if (_height == 0) { _height = _width; }
            if (_depth == 0) { _depth = _width; }
            width = _width;
            height = _height;
            depth = _depth;
            cubeGrid = new Grid3i(width, height, depth);
            cubeGridCopy = new Grid3i(width, height, depth);
            marchingGrid = new Grid3f(width + 1, height + 1, depth + 1);
            marchingGridCopy = new Grid3f(width + 1, height + 1, depth + 1);
        }

        public Grid3i GetCubeGrid() { return cubeGrid; }

        public Grid3f GetMarchingGrid() { return marchingGrid; }

        void PopulateFreePositions()
        {
            freePositions.Clear();
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < height; f++)
                {
                    for (int g = 0; g < depth; g++)
                    {
                        if (cubeGrid.Get(i, f, g) == 1)
                        {
                            //if (cubeGrid.Adg(i, f, g, 0) == 0) { freePositions.Add(new Vector3(i, f, g)); }
                            //Debug.Log(cubeGrid.SearchRegion(i - 1, f - 1, g - 1, 3, 3, 3, 0));
                            if (cubeGrid.SearchRegion(i - 1, f - 1, g - 1, 3, 3, 3, 0) == 0)
                            {
                                freePositions.Add(new Vector3(i + 0.5f, f + 0.5f, g + 0.5f));
                                //Debug.Log("placed");
                            }
                        }
                    }
                }
            }
        }

        public List<Vector3> GetFreePositions(int positions)
        {
            if (freePositions.Count == 0) { PopulateFreePositions(); }
            List<Vector3> output = new List<Vector3>(positions);
            float step = (float)freePositions.Count / positions;
            for (float i = 0; i < positions; i += step)
            {
                output.Add(freePositions[(int)i]);
            }
            return output;
        }

        public Vector3 FreePosition()
        {
            if (freePositions.Count == 0) { PopulateFreePositions(); }
            int randInt = Mathf.RoundToInt(Random.Range(0, freePositions.Count - 1));
            Vector3 output = freePositions[randInt];
            freePositions.RemoveAt(randInt);
            return output;
        }

        public void SmoothMesh(float smoothness)
        {
            float val;
            for (int i = 0; i < marchingGrid.width; i++)
            {
                for (int f = 0; f < marchingGrid.height; f++)
                {
                    for (int g = 0; g < marchingGrid.depth; g++)
                    {
                        //marchingGrid.Set(i, f, g, marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3));
                        val = marchingGridCopy.Get(i, f, g);
                        if (val != 0 && val != 1)
                        {
                            val = Mathf.Lerp(val, marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3), smoothness);
                            //marchingGrid.Set(i, f, g, (marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3)+marchingGridCopy.Get(i,f,g))/2) ;
                            marchingGrid.Set(i, f, g, val);
                        }
                    }
                }
            }
        }

        public void MakeSmoothCaves(float smoothness)
        {
            MakeCavesque();
            float val;
            for (int i = 0; i < marchingGrid.width; i++)
            {
                for (int f = 0; f < marchingGrid.height; f++)
                {
                    for (int g = 0; g < marchingGrid.depth; g++)
                    {
                        //marchingGrid.Set(i, f, g, marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3));
                        val = marchingGridCopy.Get(i, f, g);
                        if (val !=0 && val != 1)
                        {
                            val = Mathf.Lerp(val, marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3), smoothness);
                            //marchingGrid.Set(i, f, g, (marchingGridCopy.GetRegionAverage(i - 1, f - 1, g - 1, 3, 3, 3)+marchingGridCopy.Get(i,f,g))/2) ;
                            marchingGrid.Set(i, f, g, val);
                        }
                    }
                }
            }
        }

        public void MakeCavesque()
        {
            marchingGrid.CopyTo(ref marchingGridCopy);
            float val;
            bool randomize;
            for (int i = 0; i < marchingGrid.width; i++)
            {
                for (int f = 0; f < marchingGrid.height; f++)
                {
                    for (int g = 0; g < marchingGrid.depth; g++)
                    {
                        randomize = false;
                        val = marchingGrid.Get(i, f, g);
                        //if(val==1&& marchingGridCopy.AdjEdges(i, f, g) == 0) { randomize = true; }
                        if(val==1 && marchingGrid.AdjEdges(i,f,g)==0 && marchingGrid.Adg(i, f, g, 0) > 0) { randomize = true; }
                        //if (val == 0 && marchingGridCopy.Adg(i, f, g, 1) > 0) { randomize = true; }
                        if (randomize)
                        {
                            val = Random.Range(0.0f, 1.0f);
                            //Debug.Log(val);
                            marchingGridCopy.Set(i, f, g, val);
                        }
                    }
                }
            }
            marchingGridCopy.CopyTo(ref marchingGrid);
        }

        public void CopyToMarching()
        {
            cubeGrid.CopyToMarching(ref marchingGrid);
        }

        public void MakeBox()
        {
            cubeGrid.Clear(0);
            cubeGrid.SetRegion(1, 1, 1, width - 2, height - 2, depth - 2, 1);
        }

        public void MakeCross(int thickness)
        {
            MakeBox();
            cubeGrid.SetRegion(width / 2 - thickness / 2, height / 2 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 2 - thickness / 2, 1, depth / 2 - thickness / 2, thickness, height - 2, thickness, 0);
            cubeGrid.SetRegion(1, height / 2 - thickness / 2, depth / 2 - thickness / 2, width - 2, thickness, thickness, 0);
        }

        public void MakePillars(int thickness)
        {
            MakeBox();
            cubeGrid.SetRegion(width / 2 - thickness / 2, height / 4 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 4 - thickness / 2, height / 2 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 2 - thickness / 2, height / 4 * 3 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 4 * 3 - thickness / 2, height / 2 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
        }

        public void MakeTangle(int thickness)
        {
            MakeBox();
            cubeGrid.SetRegion(width / 2 - thickness / 2, height / 4 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 4 - thickness / 2, height / 2 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 2 - thickness / 2, height / 4 * 3 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            cubeGrid.SetRegion(width / 4 * 3 - thickness / 2, height / 2 - thickness / 2, 1, thickness, thickness, depth - 2, 0);
            
            cubeGrid.SetRegion(width / 2 - thickness / 2, 1, depth / 4 - thickness / 2, thickness, height - 2, thickness, 0);
            cubeGrid.SetRegion(width / 4 - thickness / 2, 1, depth / 2 - thickness / 2, thickness, height - 2, thickness, 0);
            cubeGrid.SetRegion(width / 2 - thickness / 2, 1, depth / 4*3 - thickness / 2, thickness, height - 2, thickness, 0);
            cubeGrid.SetRegion(width / 4*3 - thickness / 2, 1, depth / 2 - thickness / 2, thickness, height - 2, thickness, 0);
            
            cubeGrid.SetRegion(1, height / 2 - thickness / 2, depth / 4 - thickness / 2, width - 2, thickness, thickness, 0);
            cubeGrid.SetRegion(1, height / 4 - thickness / 2, depth / 2 - thickness / 2, width - 2, thickness, thickness, 0);
            cubeGrid.SetRegion(1, height / 2 - thickness / 2, depth / 4*3 - thickness / 2, width - 2, thickness, thickness, 0);
            cubeGrid.SetRegion(1, height / 4*3 - thickness / 2, depth / 2 - thickness / 2, width - 2, thickness, thickness, 0);
        }

        public void MakeSphere(float radius)
        {
            int x, y, z;
            float dis;
            float radS = radius * radius;
            cubeGrid.Clear(0);
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < height; f++)
                {
                    for (int g = 0; g < depth; g++)
                    {
                        x = i - width / 2;
                        y = f - height / 2;
                        z = g - depth / 2;
                        dis = x * x + y * y + z * z;
                        if (dis < radS) { cubeGrid.Set(i, f, g, 1); }
                    }
                }
            }
        }

        public void CullHidden()
        {
            cubeGrid.CopyTo(ref cubeGridCopy);
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < height; f++)
                {
                    for (int g = 0; g < depth; g++)
                    {
                        if (cubeGridCopy.Adg(i, f, g, 0) == 6) { cubeGrid.Set(i, f, g, -1); }
                    }
                }
            }
        }

        
        //handle nav meshes here?
        //GetMarchingMesh
        
    }

    class Grid3f
    {
        public float[] data;
        public int width, height, depth;

        public Grid3f(int _width, int _height = 0, int _depth = 0, float fill = 0)
        {
            width = _width;
            if (_height == 0) { _height = _width; }
            if (_depth == 0) { _depth = _width; }
            width = _width;
            height = _height;
            depth = _depth;
            data = new float[width * height * depth];
            Clear(fill);
        }

        public void CopyTo(ref Grid3f copyTo)
        {
            data.CopyTo(copyTo.data, 0);
        }

        public void Clear(float fill)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = fill;
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

        public int Adg(int x, int y, int z, float val, bool edges = false)
        {
            int count = 0;
            if (Check(x + 1, y, z, val)) { count++; }
            if (Check(x - 1, y, z, val)) { count++; }
            if (Check(x, y + 1, z, val)) { count++; }
            if (Check(x, y - 1, z, val)) { count++; }
            if (Check(x, y, z + 1, val)) { count++; }
            if (Check(x, y, z - 1, val)) { count++; }
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

        public float GetRegionAverage(int x, int y, int z, int width, int height, int depth)
        {
            float output = 0;
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < height; f++)
                {
                    for (int g = 0; g < depth; g++)
                    {
                        output += Get(x + i, y + f, z + g);
                    }
                }
            }
            output /= width * height * depth;
            return output;
        }

        public void SetRegion(int x, int y, int z, int width, int height, int depth, int val)
        {
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < height; f++)
                {
                    for (int g = 0; g < depth; g++)
                    {
                        if (OnGrid(x + i, y + f, z + g))
                        {
                            Set(GetIndex(x + i, y + f, z + g), val);
                        }
                    }
                }
            }
        }

        public bool Check(int index, float val)
        {
            bool output = false;
            if (OnGrid(index))
            {
                if (Get(index) == val) { output = true; }
            }
            return output;
        }

        public bool Check(int x, int y, int z, float val) { return Check(GetIndex(x, y, z), val); }

        public void Set(int index, float val) { data[index] = val; }
        public void Set(int x, int y, int z, float val) { data[GetIndex(x, y, z)] = val; }
        public float Get(int index) { return data[index]; }
        public float Get(int x, int y, int z) { return data[GetIndex(x, y, z)]; }
        public bool OnGrid(int index) { return index > -1 && index < data.Length; }
        public bool OnGrid(int x, int y, int z) { return OnGrid(GetIndex(x, y, z)); }

    }

    class Grid3i
    {
        public int[] data;
        public int width, height, depth;

        public Grid3i(int _width, int _height = 0, int _depth = 0, int fill = 0)
        {
            width = _width;
            if (_height == 0) { _height = _width; }
            if (_depth == 0) { _depth = _width; }
            width = _width;
            height = _height;
            depth = _depth;
            data = new int[width * height * depth];
            Clear(fill);
        }

        public void CopyTo(ref Grid3i copyTo)
        {
            data.CopyTo(copyTo.data, 0);
        }

        public void CopyToMarching(ref Grid3f grid)
        {
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < height; f++)
                {
                    for (int g = 0; g < depth; g++)
                    {
                        if (Get(i, f, g) == 0)
                        {
                            grid.Set(i, f, g, 1);
                            grid.Set(i, f + 1, g, 1);
                            grid.Set(i, f, g + 1, 1);
                            grid.Set(i, f + 1, g + 1, 1);
                            grid.Set(i + 1, f, g, 1);
                            grid.Set(i + 1, f + 1, g, 1);
                            grid.Set(i + 1, f, g + 1, 1);
                            grid.Set(i + 1, f + 1, g + 1, 1);
                        }
                    }
                }
            }
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

        public int SearchRegion(int x, int y, int z, int width, int height, int depth, int val)
        {
            int output = 0;
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < height; f++)
                {
                    for (int g = 0; g < depth; g++)
                    {
                        if (Check(x + i, y + f, z + g, val)) { output++; }
                    }
                }
            }
            return output;
        }

        public int Adg(int x, int y, int z, int val, bool edges = false)
        {
            int count = 0;
            if (Check(x+1, y, z, val)) { count++; }
            if (Check(x-1, y, z, val)) { count++; }
            if (Check(x, y+1, z, val)) { count++; }
            if (Check(x, y-1, z, val)) { count++; }
            if (Check(x, y, z+1, val)) { count++; }
            if (Check(x, y, z-1, val)) { count++; }
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

        public void SetRegion(int x, int y, int z, int width, int height, int depth, int val)
        {
            for (int i = 0; i < width; i++)
            {
                for (int f = 0; f < height; f++)
                {
                    for (int g = 0; g < depth; g++)
                    {
                        if(OnGrid(x + i, y + f, z + g))
                        {
                            Set(GetIndex(x + i, y + f, z + g), val);
                        }
                    }
                }
            }
        }

        public bool Check(int index, int val)
        {
            bool output = false;
            if (OnGrid(index))
            {
                if (Get(index) == val) { output = true; }
            }
            return output;
        }

        public bool Check(int x, int y, int z, int val) { return Check(GetIndex(x, y, z), val); }

        public void Set(int index, int val) { data[index] = val; }
        public void Set(int x, int y, int z, int val) { data[GetIndex(x, y, z)] = val; }
        public int Get(int index) { return data[index]; }
        public int Get(int x, int y, int z) { return data[GetIndex(x, y, z)]; }
        public bool OnGrid(int index) { return index > -1 && index < data.Length; }
        public bool OnGrid(int x, int y, int z) { return OnGrid(GetIndex(x, y, z)); }
    }
}
