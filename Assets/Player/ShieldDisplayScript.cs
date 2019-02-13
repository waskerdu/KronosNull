using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShieldDisplayScript : MonoBehaviour {

    int segmentSelected = 0;
    public int numElements = 24;

    void SetSelected(int selected)
    {
        segmentSelected = numElements - 1 - selected;
    }

    void SetSegmentColor(Color col)
    {
        if (segmentSelected < transform.childCount)
        {
            transform.GetChild(segmentSelected).GetComponent<Image>().color = col;
        }
        
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
