using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartBombLauncherScript : MonoBehaviour {

    //smart bomb
    public float damage = 10;
    public float speed = 10;
    GameObject bullet;
    RaycastHit hit;
    Ray ray;
    Vector3 tempVec;
    public bool fire1 = false;
    public bool fire2 = false;
    public float reloadTime = 1;
    float lastFired = 0;
    public AudioClip clip;
    AudioSource audioSource;
    public GameObject smartBomb;
    GameObject tempBullet;
    public int maxAmmo = 3;
    public int ammo = 0;
    public GameObject ammoCounter;
    public bool isEmp = true;

    void Fire1Down() { fire1 = true; }
    void Fire1Up() { fire1 = false; }
    void Fire2Down() { fire2 = true; }
    void Fire2Up() { fire2 = false; }

    enum state
    {
        ready,
        fire,
        reload,
        primed,
        empty
    }

    state myState = state.ready;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        ResetAmmo();
    }

    void ResetAmmo() { ammo = maxAmmo; myState = state.ready; }


    void Update()
    {
        //weapon is a simple state machine. right now it is audio driven
        if (myState == state.ready && fire2)
        {
            //fire
            myState = state.fire;
            lastFired = Time.time;
        }
        else if (myState == state.fire && ammo > 0) 
        {
            myState = state.reload;
            audioSource.clip = clip;
            audioSource.Play();
            tempBullet = Instantiate(smartBomb, transform.position, Quaternion.identity);
            tempBullet.SetActive(true);
            tempBullet.SendMessage("SetType", isEmp);
            tempBullet.GetComponent<Rigidbody>().velocity = transform.parent.GetComponent<Rigidbody>().velocity;
            tempBullet.GetComponent<Rigidbody>().velocity += transform.forward * speed;
            ammo--;
            if (ammo == 0)
            {
                myState = state.empty;
                SendMessageUpwards("NeedAmmo");
            }
            ammoCounter.SendMessage("DepleteAmmo");
        }
        //else if (myState == state.fire && ammo == 0) { SendMessageUpwards("NeedAmmo"); }
        else if (myState == state.reload)
        {
            if (!audioSource.isPlaying)
            {
                myState = state.primed;//comment out to make it not audio driven
            }
            if (Time.time > lastFired + reloadTime)
            {
                myState = state.primed;
            }

        }
        else if (myState == state.primed && !fire2) { myState = state.ready; }
    }
}
