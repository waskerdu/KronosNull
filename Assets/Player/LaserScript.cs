using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserScript : MonoBehaviour {
    RaycastHit hit;
    Ray ray;
    public float dis;
    LineRenderer line;
    Vector3 tempVec;
    public float ang = 1.0f;
    float stealthMultiplier = 1;

    void StealthModeActivate()
    {
        stealthMultiplier = 0;
    }

    void StealthModeDeactivate()
    {
        stealthMultiplier = 1;
    }

    void Start ()
    {
        line = GetComponent<LineRenderer>();
    }
	
	void Update () {
        ray.origin = transform.position;
        ray.direction = transform.forward;
        if(Physics.Raycast(ray, out hit, 1000000, -1, QueryTriggerInteraction.Ignore))
        {
            dis = hit.distance;
            //sin theta = opposite / hypotnuse
            //line.endWidth = (float)(Mathf.Sin(ang * Mathf.Deg2Rad) * dis);
            Vector3 endPoint = line.GetPosition(1);
            endPoint.z = dis * stealthMultiplier;
            line.SetPosition(1, endPoint);
            /*if (hit.collider.gameObject.tag == "Actor")
            {
                setColor(Color.red);
            }
            else { setColor(Color.blue); }/**/
        } 
	}

    void SetLaserColor(Color nextColor)
    {
        line.startColor = nextColor;
        line.endColor = nextColor;
        //Debug.Log("here");
    }
}
