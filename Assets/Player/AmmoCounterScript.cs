using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCounterScript : MonoBehaviour {
    public int maxCapacity = 3;
    public int capacity = 3;
    
	void Start () {
        ResetAmmo();
	}
	
	void ResetAmmo()
    {
        capacity = maxCapacity;
        for (int i = 0; i < maxCapacity; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    void DepleteAmmo()
    {
        capacity--;
        transform.GetChild(capacity).gameObject.SetActive(false);
    }
}
