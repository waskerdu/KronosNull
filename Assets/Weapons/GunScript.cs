using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunScript : MonoBehaviour {
    //rifle
    public float damage = 10;
    GameObject bullet;
    RaycastHit hit;
    Ray ray;
    Vector3 tempVec;
    public float dis = 0;
    public bool fire1 = false;
    public bool fire2 = false;
    public float reloadTime = 1;
    float reloadClock = 0;
    int soundIter = 0;
    float lastFired = 0;
    public AudioClip[] clips = new AudioClip[4];
    AudioSource audioSource;
    public float bulletForce = 10;
    public float regenRate = 1;
    public float regenDelay = 2;
    public int burst = 1;
    int burstRemaining = 0;
    public float cycleDelay = 0.3f;
    float cycleClock = 0;
    public bool automaticWeapon = false;
    public bool aimAssist = true;
    public bool useAudioTiming = true;

    void Fire1Down() { fire1 = true; }
    void Fire1Up() { fire1 = false; }
    void Fire2Down() { fire2 = true; }
    void Fire2Up() { fire2 = false; }
    movement script;

    enum state
    {
        ready,
        lockon,
        fire,
        cycle,
        reload,
        primed
    }

    state myState = state.ready;

    void Start ()
    {
        audioSource = GetComponent<AudioSource>();
        script = transform.parent.gameObject.GetComponent<movement>();
        burstRemaining = burst;
        if (useAudioTiming)
        {
            cycleDelay = clips[0].length;
            reloadTime = 0;
            /*for (int i = 0; i < clips.Length; i++)
            {
                reloadTime += clips[i].length;
            }/**/
            reloadTime = clips[0].length/2;
        }
        //convert delays from seconds to fixed update intervals
        reloadTime /= Time.fixedDeltaTime;
        cycleDelay /= Time.fixedDeltaTime;
	}

    void SetDamage(int _damage) { damage = _damage; }

    void FireIgnoreShield()
    {
        ray.origin = transform.position;
        ray.direction = transform.forward;
        if (aimAssist)
        {
            if (Physics.Raycast(ray,out hit))
            {
                if (hit.collider.gameObject.tag == "Shield")
                {
                    //BroadcastMessage("SetLaserColor", Color.red);
                    hit.collider.gameObject.SendMessageUpwards("Damage", damage);
                    //hit.collider.gameObject.transform.parent.GetComponent<Rigidbody>().AddForceAtPosition(ray.direction, hit.point);
                }
            }
        }
        else
        {
            if (Physics.Raycast(ray, out hit, 1000000, -1, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject.tag == "Actor")
                {
                    hit.collider.gameObject.transform.parent.SendMessage("Damage", damage);
                    hit.collider.gameObject.transform.parent.GetComponent<Rigidbody>().AddForceAtPosition(ray.direction, hit.point);
                }
            }
        }
        
    }

    void FixedUpdate()
    {
        if (reloadClock < 0) { reloadClock = 0; }
        else { reloadClock--; }

        if (cycleClock < 0) { cycleClock = 0; }
        else { cycleClock--; }
    }


	void Update ()
    {
        switch (myState)
        {
            case state.ready:
                if (fire1)
                {
                    myState = state.fire;
                }
                break;

            case state.lockon:
                break;

            case state.fire:
                lastFired = Time.time;
                if (burstRemaining > 1)
                {
                    myState = state.cycle;
                    cycleClock = cycleDelay;
                    burstRemaining--;
                }
                else
                {
                    myState = state.reload;
                    reloadClock = reloadTime;
                    soundIter = 1;
                }
                audioSource.clip = clips[0];
                audioSource.Play();
                break;

            case state.cycle:
                //if (Time.time > lastFired + cycleDelay)
                if (cycleClock == 0) { myState = state.fire; }
                /*if (!audioSource.isPlaying)
                {
                    myState = state.fire;
                }*/
                break;

            case state.reload:
                //if (soundIter < clips.Length)
                if (reloadClock == 0)
                {
                    myState = state.primed;
                }
                /*else
                {
                    if (!audioSource.isPlaying && soundIter < clips.Length)
                    {
                        audioSource.clip = clips[soundIter];
                        audioSource.Play();
                        soundIter++;
                    }
                }*/
                break;

            case state.primed:
                if (!fire1 || automaticWeapon) { myState = state.ready; burstRemaining = burst; }
                break;

            default:
                break;
        }
        ray.origin = transform.position;
        ray.direction = transform.forward;
        //if (Physics.Raycast(ray, out hit, 1000000, -1, QueryTriggerInteraction.Ignore))
        if (Physics.Raycast(ray, out hit))
        {
            
            Color color = Color.blue;
            if (myState == state.reload || myState == state.cycle) { color = Color.yellow; }
            else if (hit.collider.gameObject.tag == "Actor" || hit.collider.gameObject.tag=="Shield") { color=Color.red; }
            BroadcastMessage("SetLaserColor", color);
            if (color != Color.blue) { script.targetInCrosshair = true; }
            else { script.targetInCrosshair = false; }
        }
        if (Physics.Raycast(ray, out hit))
        {
            dis = hit.distance;
            hit.collider.gameObject.SendMessage("FindSegment", hit.point, SendMessageOptions.DontRequireReceiver);
            hit.collider.gameObject.SendMessage("MakeVisible", SendMessageOptions.DontRequireReceiver);
            if (myState != state.reload)
            {
                if (hit.collider.gameObject.tag == "Shield")
                {
                    if (myState == state.fire)
                    {
                        //hit.collider.gameObject.SendMessage("FindSegment", hit.point, SendMessageOptions.DontRequireReceiver);
                        hit.collider.gameObject.SendMessage("DoTryAgain", gameObject, SendMessageOptions.DontRequireReceiver);
                        hit.collider.gameObject.SendMessage("Damage", damage, SendMessageOptions.DontRequireReceiver);
                    }
                    /*else
                    {
                        hit.collider.gameObject.SendMessage("FindSegment", hit.point, SendMessageOptions.DontRequireReceiver);
                        hit.collider.gameObject.SendMessage("MakeVisible", SendMessageOptions.DontRequireReceiver);
                    }*/
                }

                else if (hit.collider.gameObject.tag == "Actor")
                {
                    if (myState == state.fire)
                    {
                        hit.collider.gameObject.transform.parent.SendMessage("Damage", damage);
                        hit.collider.gameObject.transform.parent.GetComponent<Rigidbody>().AddForceAtPosition(ray.direction, hit.point);
                    }
                }
            }
        }
    }
}
