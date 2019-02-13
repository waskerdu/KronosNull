using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartBombScript : MonoBehaviour {

    const int numClips = 2;
    public AudioClip[] clips = new AudioClip[numClips];
    public float activationDelay = 2;
    float birthTime = 0;
    public int numBeeps = 6;
    int beepCount = 0;
    public state myState = state.asleep;
    AudioSource source;
    public float explosionForce = 10;
    public float damage = 65;
    public float dampen = 10;
    Collider col;
    public float radius = 10;
    GameObject target = null;
    float targetDis = 0;
    Vector3 angleTarget = Vector3.zero;
    Rigidbody rb;
    float minVelocityMatch = 1;
    float turnSpeed = 1;
    Vector3 lookAtDirection = Vector3.zero;
    public float thrust = 0.1f;
    public GameObject volume;
    public GameObject hull;
    public bool isEmp = false;
    bool lockOn = false;
    
    public enum state
    {
        asleep,
        active,
        countdown,
        detonated
    }

    void SetType(bool setEmp)
    {
        isEmp = setEmp;
    }

    void Start()
    {
        birthTime = Time.time;
        source = GetComponent<AudioSource>();
        col = GetComponent<Collider>();
        GetComponent<SphereCollider>().radius = radius;
        volume.transform.localScale = Vector3.one * radius;
        targetDis = radius * 2;
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    /*void OnDrawGizmos()
    {
        if (myState == state.active)
        {
            Gizmos.color = Color.blue;
        }
        else if (myState == state.countdown || myState == state.detonated)
        {
            Gizmos.color = Color.red;
        }
        if (myState!=state.asleep)
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }*/

    void Damage(float dam)
    {
        Detonate();
    }

    void OnCollisionExit(Collision collision)
    {
        transform.GetChild(0).GetComponent<Collider>().enabled = true;
    }


    void OnTriggerEnter(Collider collider)
    {
        if (myState == state.active)
        {
            if (collider.gameObject.tag == "Actor")
            {
                myState = state.countdown;
                volume.GetComponent<MeshRenderer>().material.SetColor("_WireColor", Color.red);
                //GetComponent<Rigidbody>().drag = dampen;
                col.enabled = false;
                if (Vector3.Distance(transform.position, collider.gameObject.transform.position) < targetDis)//targets closest actor
                {
                    target = collider.gameObject;
                }
            }
        }
        else if (myState == state.detonated)
        {
            if (collider.gameObject.tag == "Actor")
            {
                //collider.gameObject.transform.parent.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, radius);
                
                if (!isEmp) { collider.gameObject.transform.parent.SendMessage("Damage", damage); }
                
            }
            if (collider.gameObject.tag == "Shield")
            {
                if (isEmp) { collider.gameObject.SendMessage("Deplete", damage); }
            }
        }
    }

    void Detonate()
    {
        myState = state.detonated;
        col.enabled = true;
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            angleTarget = target.transform.parent.GetComponent<Rigidbody>().velocity - rb.velocity;
            //angleTarget = rb.velocity + angleTarget;
            //Debug.Log(angleTarget.magnitude);
            //Debug.Log(Vector3.Magnitude(rb.velocity + angleTarget));
            if (Vector3.Magnitude(angleTarget) < minVelocityMatch)
            {
                //transform.LookAt(target.transform);
                lockOn = true;
            }
            if (lockOn)
            {
                transform.LookAt(target.transform);
            }
            else
            {
                transform.LookAt(transform.position + angleTarget);
            }

            rb.AddRelativeForce(Vector3.forward * thrust);
        }
    }
    
    void Update()
    {
        if (myState == state.asleep)
        {
            if (Time.time > birthTime + activationDelay)
            {
                myState = state.active;
                col.enabled = true;
                volume.SetActive(true);
            }
        }
        else if (myState == state.countdown)
        {
            if (!source.isPlaying)
            {
                if (beepCount < numBeeps)
                {
                    source.clip = clips[0];
                    source.pitch *= 1.1f;
                    source.Play();
                }
                else
                {
                    source.clip = clips[1];
                    source.Play();
                    Detonate();
                }
                beepCount++;
            }
        }
        else if (myState == state.detonated)
        {
            if (!source.isPlaying)
            {
                Destroy(gameObject);
            }
        }
    }
}
