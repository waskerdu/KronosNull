using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartGrid
{
    class SmartGrid
    {
        List<Vector4> cells;
        List<int> hashStore;
        List<List<int>> hashRef;

        public SmartGrid()
        {
            cells = new List<Vector4>();
            hashStore = new List<int>();
            hashRef = new List<List<int>>();
        }

        bool Equals(Vector3 pos1, Vector3 pos2, float tolerance = 0)
        {
            if (Vector3.Magnitude(pos1 - pos2) > tolerance){ return false; }
            else { return true; }
        }

        bool onGrid(Vector3 pos)
        {
            int tempHash = pos.GetHashCode();
            if (hashStore.Contains(tempHash)) { return true; }
            else { return false; }
        }

        public void Add(Vector4 newCell)
        {
            cells.Add(newCell);
            Vector3 tempPos = newCell;
            int tempHash = tempPos.GetHashCode();
            if (hashStore.Contains(tempHash))
            {
                //do stuff
                int index = hashStore.IndexOf(tempHash);
                hashRef[index].Add(cells.Count - 1);
            }
            else
            {
                hashStore.Add(tempHash);
                hashRef.Add(new List<int>());
                hashRef[hashRef.Count - 1].Add(cells.Count - 1);
            }
        }
        public float Get(Vector3 pos)
        {
            //returns value of cell at position pos. if cell is not listed it returns a value of 1
            int hash = pos.GetHashCode();
            if (hashStore.Contains(hash))
            {
                //do stuff
                int index = hashStore.IndexOf(hash);
                foreach (int item in hashRef[index])
                {
                    Vector3 tempPos = cells[item];
                    if (Equals(tempPos, pos))
                    {
                        return cells[item].w;
                    }
                }
            }
            else { return 1; }
            Debug.Log("get method malfunctioned");










































































































































        




























































        














        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        

















            return 0;
        }
        public float Average(Vector3 pos)
        {
            //averages the value in the cell at position pos with adj values and returns it
            return 0;
        }
    }
}
