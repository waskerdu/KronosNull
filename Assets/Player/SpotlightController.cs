using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightController : MonoBehaviour {

    // Use this for initialization
    public float intensity = 1;
    public float minIntensity = 0.5f;
    public float range = 1000;
    public float minRange = 100;
    void StealthModeActivate()
    {
        GetComponent<Light>().intensity = minIntensity;
        GetComponent<Light>().range = minRange;
    }
    void StealthModeDeactivate()
    {
        GetComponent<Light>().intensity = intensity;
        GetComponent<Light>().range = range;
    }
	void Start () {
        StealthModeDeactivate();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
