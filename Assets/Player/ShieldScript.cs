using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldScript : MonoBehaviour {
    const int numSegments = 24;
    public GameObject[] meshes = new GameObject[numSegments];
    GameObject[] children = new GameObject[numSegments];
    GameObject[] indicators = new GameObject[numSegments];
    public GameObject shieldSegment;
    public GameObject shieldIndicator;
    GameObject tempObj;
    public Color[] colors =new Color[5];
    public float shieldLevel = 2;
    //public float sheildHealth
    public float segmentMaxHealth = 70;
    public float segmentThreshold = 35;
    float[] segmentHealth = new float[numSegments];
    float[] segmentLastDamage = new float[numSegments];
    public GameObject[] startProbes = new GameObject[4];
    GameObject[] probes = new GameObject[numSegments];
    float damageBuffer = 0;
    public GameObject hull;
    public GameObject shieldUi;
    public bool deplete = false;
    List<int> depleteList = new List<int>(numSegments);
    public int empResistance = 4;
    int empCount = 0;
    public bool isCenturion = false;

    int segmentSelected = 0;

    void SetShieldLevel(int _shieldLevel)
    {
        shieldLevel = _shieldLevel;
    }

	void Start () {
        
        int probeCount = -1;
        Vector3 tempPos = Vector3.zero;
        for (int i = 0; i < numSegments; i++)
        {
            segmentHealth[i] = 0;
            
            segmentLastDamage[i] = 0;
            children[i] = Instantiate(shieldSegment, transform.position, transform.rotation, transform);
            children[i].SendMessage("SetMesh", meshes[i]);
            children[i].transform.localScale *= 0.025f;
            indicators[i] = Instantiate(shieldIndicator, transform.position, transform.rotation, transform);
            indicators[i].SendMessage("SetMesh", meshes[i]);
            indicators[i].transform.localScale *= 0.025f;
            indicators[i].SetActive(false);
            if (i % 6 == 0)
            {
                probeCount++;
                probes[i] = startProbes[probeCount];
            }
            else
            {
                tempPos = startProbes[probeCount].transform.position;
                probes[i] = Instantiate(startProbes[probeCount], tempPos, Quaternion.identity, transform);
                probes[i].transform.RotateAround(transform.position, transform.up, 60 * (i % 6));
            }
            probes[i].SendMessage("SetIndex", i);
        }
        Restore();
	}

    void MakeVisible()
    {
        indicators[segmentSelected].SetActive(true);
    }

    void FindSegment(Vector3 position)
    {
        int x, y;
        position = transform.InverseTransformPoint(position);
        if (position.y > 0)
        {
            if (position.y > 2) { x = 3; }
            else { x = 2; }
        }
        else
        {
            if (position.y < -2) { x = 0; }
            else { x = 1; }
        }
        y = Mathf.FloorToInt(Vector3.SignedAngle(Vector3.back, new Vector3(position.x, 0, position.z), Vector3.down) / 60) + 3;
        segmentSelected = x * 6;
        segmentSelected += y;
    }

    void SegmentUpdate(int index)
    {
        float health = segmentHealth[index];
        Color col = Color.white;
        bool setActive = true;
        if (health == shieldLevel) { setActive = false; }
        if (health > -1) { col = colors[(int)health]; }
        else { col = Color.white; }
        /*if (health > -1) { col = colors[(int)health]; }
        else { setActive = false; }/**/
        children[index].SetActive(setActive);
        if (setActive) { children[index].SendMessage("SetSegmentColor", col); }
        shieldUi.SendMessage("SetSelected", index);
        shieldUi.SendMessage("SetSegmentColor", col);
    }

    void SetDamageBuffer(float set)
    {
        if (set > damageBuffer) { damageBuffer = set; }
    }

    void SetSelected(int selected) { segmentSelected = selected; }

    void ProbeDamage(Vector4 damagePack)
    {
        if (segmentHealth[segmentSelected] > -1)
        {
            segmentHealth[segmentSelected] -= 1;
            segmentLastDamage[segmentSelected] = Time.time;
            SegmentUpdate(segmentSelected);
        }
        else
        {
            //project a ray from probe and see if it hits the hull
        }
        
    }

    bool OnShield(ref int x, ref int y)
    {
        bool output = true;
        if (x < 0) { output = false; }
        else if (x > 3) { output = false; }
        if (y < 0) { y += 6; }
        else if (y > 5) { y -= 6; }
        //else if (y < 0) { output = false; }
        //else if (y > 5) { output = false; }
        return output;
    }

    int ToIndex(int x, int y)
    {
        int index = x * 6;
        index += y;
        return index;
    }

    void MakeAdjVisible()
    {
        int y = segmentSelected % 6;
        int x = segmentSelected / 6;
        Debug.Log(x);
        int tempY, tempX;
        if(x==0 || x == 3)
        {
            tempX = x;
            tempY = y + 1;
            if (OnShield(ref tempX,ref tempY))
            {
                children[ToIndex(tempX, tempY)].gameObject.SetActive(true);
            }
            tempY = y - 1;
            if (OnShield(ref tempX, ref tempY))
            {
                children[ToIndex(tempX, tempY)].gameObject.SetActive(true);
            }
            return;
        }
        for (int i = 0; i < 3; i++)
        {
            for (int f = 0; f < 3; f++)
            {
                tempY = y - 1 + f;
                tempX = x - 1 + i;
                if (OnShield(ref tempX, ref tempY))
                {
                    children[ToIndex(tempX, tempY)].gameObject.SetActive(true);
                }
            }
        }
    }

    void Damage(float damage)
    {
        segmentHealth[segmentSelected] --;
        segmentLastDamage[segmentSelected] = Time.time;
        SegmentUpdate(segmentSelected);
        MakeAdjVisible();
        if (segmentHealth[segmentSelected] > -1)
        {
            SendMessageUpwards("Flee");
        }
        
    }

    void DoTryAgain(GameObject gun)
    {
        if (segmentHealth[segmentSelected] < 0)
        {
            gun.SendMessage("FireIgnoreShield");
        }
    }

    void Restore()
    {
        deplete = false;
        for (int i = 0; i < numSegments; i++)
        {
            if (isCenturion && i < numSegments / 2)
            {
                segmentHealth[i] = -1;
            }
            else { segmentHealth[i] = shieldLevel; }
            segmentLastDamage[i] = 0;
            SegmentUpdate(i);
        }
    }

    void Deplete()
    {
        deplete = true;
        depleteList.Clear();
        for (int i = 0; i < numSegments; i++)
        {
            depleteList.Add(i);
        }
        SendMessageUpwards("NeedShield");
    }

    void FixedUpdate ()
    {
        if (deplete && empCount == 0)
        {
            int selected;
            int index;
            bool removed = false;
            empCount = empResistance;
            //Debug.Log(depleteList.Count);
            while (!removed)
            {
                selected = Mathf.FloorToInt(Random.Range(0, depleteList.Count));
                index = depleteList[selected];
                //Debug.Log(selected);
                if (segmentHealth[index] == -1)
                {
                    depleteList.RemoveAt(selected);
                    if (depleteList.Count == 0) { deplete = false; break; }
                }
                else
                {
                    segmentHealth[index]--;
                    SegmentUpdate(index);
                    removed = true;
                }
                
            }
        }
        else if (deplete) { empCount--; }
        for (int i = 0; i < numSegments; i++)
        {
            indicators[i].SetActive(false);
        }
	}
}
