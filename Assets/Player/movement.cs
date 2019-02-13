using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour {
    float torque = 0.1f;
    public float force = 1.0f;
    public float maxHealth = 100;
    public float health = 1;
    public Vector3 moveVector = Vector3.zero;
    public Vector3 spinVector = Vector3.zero;
    public float breakForce = 100.0f;
    float rotBreak = 0.0f;
    float linBreak = 0.0f;
    bool isBraking = false;
    bool isAngBreaking = false;
    public float boostPower = 2;
    bool isBoosting = true;
    public float lateralDampening = 0.1f;
    public int controller = 1;
    string xThrust;
    string yThrust;
    string zThrust;
    string yawTorque;
    string pitchTorque;
    string rollTorque;
    string breakButton;
    string angBreakButton;
    string boostButton;
    string fire1Button;
    string fire2Button;
    string invertY;
    Rigidbody myRb;
    bool directAngleControl = true;
    public float sensitivity = 10;
    float regularFov = 90;
    float boostFov = 90;
    float fovSlerp = 0;
    float fovSlerpSpeed = 0.03f;
    public GameObject myCamera;
    bool disableInput = false;
    bool shouldInvertY = false;
    public float boostSensitivity = 0.1f;
    public bool inStealth = false;
    GameInput.Controller myController;
    //Vector3 headingDesired = Vector3.zero;
    LevelGeneration.NavSystem navSystem;
    [Header("Aim Assist")]
    public bool aimAssist = true;
    public float aimAssistDampen = 0.5f;
    public float aimAssistAltDampen = 0.5f;
    public float aimAssistAngle = 20;
    public float aimAssistInnerAngle = 5;
    public bool targetInCrosshair = false;
    public float aimAssistDistanceScale = 200;
    
    [Header("bot logic")]
    //initialization
    public bool isBot = false;
    BotLogic.BotHandler botHandler;
    List<GameObject> navList;
    public GameObject arenaMaster;
    GameObject navTarget;
    Vector3 navTargetPosition;
    GameObject attackTarget;
    int navListIter = 0;
    LineRenderer navPath;
    List<GameObject> enemies = new List<GameObject>();
    List<GameObject> allies = new List<GameObject>();
    public List<GameObject> shieldRecharge = new List<GameObject>();
    public List<GameObject> ammoRestore = new List<GameObject>();
    List<GameObject> mines = new List<GameObject>();
    Vector3 navPoint = Vector3.zero;
    bool needRandomVec = false;
    Vector3 randomVec = Vector3.zero;
    //ai state
    public BotLogic.aiState myAiState = BotLogic.aiState.getShield;
    public float botTurnSpeed = 180;
    public BotLogic.aiState oldAiState = BotLogic.aiState.none;
    public BotLogic.aiState bufferAiState = BotLogic.aiState.none;
    public BotLogic.attackState myAttackType = BotLogic.attackState.rush;
    public BotLogic.attackState oldAttackState = BotLogic.attackState.none;
    public BotLogic.attackState bufferAttackState = BotLogic.attackState.none;
    //ai stats
    public int allegiance = -1;
    public float crosshairFov = 1;
    bool isNewAiState = false;
    bool isNewAttackState = false;
    public bool isObservant = false;//can bot spot player lights and lasers?
    public bool needsLaser = true;//does bot need an active laser to shoot?
    public bool goodListener = false;//
    public float minCircle = 100;//for circiling attack state
    public float maxCircle = 300;
    bool targetVisible = false;//is target within fov
    //bool targetInCrosshair = false;
    [Header("Difficulty")]
    public BotLogic.botDifficulty difficulty = BotLogic.botDifficulty.max;
    public float botFov = 45;
    float bombTimer = -1;
    public float minBombTime = 0;
    public float maxBombTime = 0;
    float gunTimer = -1;
    public float minGunTime = 0;
    public float maxGunTime = 1;
    public float reactionTime = 0.25f;
    public bool selfUpdate = false;
    public float fleeChance = 1;
    Vector3 oldFacing = Vector3.zero;
    public GameObject targetUi;
    public Color color = Color.white;
    public GameObject spectator;
    [Header("Bot Voices Stuff")]
    public AudioClip[] attack;
    public AudioClip[] getAmmo;
    public AudioClip[] getShield;
    public AudioClip[] wander;
    public AudioClip[] investigate;
    public AudioClip[] flee;
    AudioSource audioSource;

    void Start()
    {
        myRb = gameObject.GetComponent<Rigidbody>();
        myRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        //health = maxHealth;
        //navPath = GetComponent<LineRenderer>();
        SetDifficulty(difficulty);
        targetUi.GetComponent<TargetUIScript>().SetAllies(ref allies);
        targetUi.GetComponent<TargetUIScript>().SetEnemies(ref enemies);
        audioSource = GetComponent<AudioSource>();
        //SetController(new GameInput.Controller(GameInput.ControllerType.xbox, -1));
    }

    void Update()
    {
        disableInput = Time.timeScale == 0;
        if (!disableInput) { InputHandling(); }
        

    }

    void FixedUpdate()
    {
        myRb.AddRelativeForce(moveVector * force);

        if (isBraking) { linBreak = breakForce; }
        else { linBreak = 0; }

        myRb.drag = linBreak;
        if (isBoosting)
        {
            //kill lateral velocity over time while maintaining and increasing forward velocity
            Vector3 vDesired = transform.InverseTransformVector(myRb.velocity);//converts velocity to local space
            vDesired.x = 0;
            vDesired.y = 0;
            if (vDesired.z < 0) { vDesired.z = 0; }
            vDesired = transform.TransformVector(vDesired);//converts desired vec back into world space
            myRb.velocity = Vector3.Slerp(myRb.velocity, vDesired, lateralDampening);
            myRb.AddRelativeForce(Vector3.forward * boostPower);
            //engine is boosting sound
            //transform.GetChild(0).gameObject.GetComponent<AudioSource>().volume = engineVolume * 2;
        }
        else
        {
            //transform.GetChild(0).gameObject.GetComponent<AudioSource>().volume = myController.movement.magnitude * engineVolume;
        }
        //if (!transform.GetChild(0).gameObject.GetComponent<AudioSource>().isPlaying) { transform.GetChild(0).gameObject.GetComponent<AudioSource>().Play(); }
    }

    void InputHandling()
    {
        /*if (botHandler == null) { botHandler = new BotLogic.BotHandler(transform, ref myController); }
        Vector3 avoidance = botHandler.CollisionAvoidance();
        Debug.Log(avoidance);
        if (avoidance != Vector3.zero)
        {
            //lookAt = false;
            //shouldBoost = false;
            myController.angle = Vector3.zero;
            //myController.movement = Vector3.zero;
            myController.angle.x = avoidance.y;
            myController.angle.y = -avoidance.x;
            //myController.movement.x = avoidance.y;
            //myController.movement.y = avoidance.x;
            //Vector3.SmoothDamp()
            myController.movement.z = 1;
            //shouldAccelerate = false;
        }/**/
        if (isBot)
        {
            if (myController == null)
            {
                SetController(new GameInput.Controller(GameInput.ControllerType.xbox, -1));
                navSystem = arenaMaster.GetComponent<arenaGen>().navSystem;
                botHandler = new BotLogic.BotHandler(transform, ref myController, reactionTime);
            }
            BotUpdate();
        }
        else if (selfUpdate) { myController.Update(); }
        moveVector = myController.movement;
        spinVector = myController.angle;

        isBoosting = myController.isBoosting;
        isBraking = myController.isBraking;
        if (isBoosting) { spinVector = Vector3.zero; }
        if (myController.isFire1Down) { BroadcastMessage("Fire1Down"); }
        if (myController.isFire1Up) { BroadcastMessage("Fire1Up"); }
        if (myController.isFire2Down) { BroadcastMessage("Fire2Down"); }
        if (myController.isFire2Up) { BroadcastMessage("Fire2Up"); }
        
        spinVector = transform.localToWorldMatrix.MultiplyVector(spinVector * sensitivity);
        /* Aim Assist system
         * Lowers sensitivity and makes movement axis linear while player is aiming within a few degrees of an enemy
         * Should prefer enemy the closest to the center of screen
         * Should "nudge" the controller along target's relative motion (not yet implimented)
         * currently disabled because it sucks
         */
        /*if (aimAssist)
        {
            myController.useLookCurve = true;
            GameObject enemy = null;
            float minDiff = float.MaxValue;
            float angDiff;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (botHandler == null) { botHandler = new BotLogic.BotHandler(transform, ref myController); }
                angDiff = botHandler.isVisibleAngle(transform.forward, transform.position, enemies[i].transform.position);
                if (!float.IsNaN(angDiff))
                {
                    if (angDiff < minDiff) { enemy = enemies[i];minDiff = angDiff; }
                }
            }
            angDiff = minDiff;
            if (enemy != null)
            {
                if (angDiff < aimAssistInnerAngle)
                {
                    spinVector *= aimAssistAltDampen;
                    myController.useLookCurve = false;
                }
                else if (angDiff < aimAssistAngle)
                {
                    spinVector *= aimAssistDampen;
                    myController.useLookCurve = false;
                }
            }
        }/**/
        myRb.angularVelocity = spinVector;

        if (health < 0)
        {
            //transform.GetChild()
            if (!isBot)
            {
                GameObject tempObject = Instantiate(spectator, transform.position, transform.rotation);
                tempObject.SetActive(true);
                tempObject.GetComponent<Camera>().enabled = false;
                tempObject.GetComponent<SpectatorScript>().SetController(myController);
                myCamera.transform.SetParent(tempObject.transform);
                targetUi.transform.SetParent(tempObject.transform);
                targetUi.transform.GetChild(2).gameObject.SetActive(true);
            }
            gameObject.SetActive(false);
        }

        /*if (myController.isStealthDown)
        {
            inStealth = !inStealth;
            if (inStealth) { BroadcastMessage("StealthModeActivate"); }
            else { BroadcastMessage("StealthModeDeactivate"); }
        }*/
    }

    public void SetActors(List<GameObject> actors)
    {
        foreach (GameObject actor in actors)
        {
            int actorAlligance = actor.GetComponent<movement>().allegiance;
            if ((actorAlligance == allegiance && actorAlligance != -1) || actorAlligance == -2)
            {
                allies.Add(actor);
            }
            else if (actor != gameObject)
            {
                enemies.Add(actor);
            }
        }
    }

    void SetCameraRect(Rect camRect)
    {
        myCamera.GetComponent<Camera>().rect = camRect;
    }

    /*void setController(int con=1)
    {
        controller = con;
    }*/

    void Damage(float dam)
    {
        health -= dam;
        myAiState = BotLogic.aiState.getShield;
        BroadcastMessage("SetHealthDisplay", health / maxHealth);
    }

    void setControlls()
    {
        string controllerNum = controller.ToString();
        xThrust = "x" + controllerNum;
        yThrust = "y" + controllerNum;
        zThrust = "z" + controllerNum;

        yawTorque = "yaw" + controllerNum;
        pitchTorque = "pitch" + controllerNum;
        rollTorque = "roll" + controllerNum;

        breakButton = "brake" + controllerNum;
        boostButton = "boost" + controllerNum;
        //angBreakButton = "angBreak" + controllerNum;
        fire1Button = "fire1_" + controllerNum;
        fire2Button = "fire2_" + controllerNum;
        invertY = "invertY" + controllerNum;
        //myController = new GameInput.Controller(GameInput.ControllerType.xbox, controller);
    }

    /*public void SetController(ref GameInput.Controller _myController)
    {
        myController = _myController;
        botHandler = new BotLogic.BotHandler(transform, ref myController);
        navSystem = arenaMaster.GetComponent<arenaGen>().navSystem;
    }*/

    public void SetController(GameInput.Controller _myController)
    {
        myController = _myController;
        Debug.Log(myController.index);
        if (myController.index == -1)
        {
            isBot = true;
            navSystem = arenaMaster.GetComponent<arenaGen>().navSystem;
            botHandler = new BotLogic.BotHandler(transform, ref myController, reactionTime);
        }
    }

    //bot logic
    void NeedAmmo() { myAiState = BotLogic.aiState.getAmmo; }
    void NeedShield() { myAiState = BotLogic.aiState.getShield; }

    public void Flee()
    {
        if (Random.Range(0.0f, 1.0f) < fleeChance && myAiState!=BotLogic.aiState.getShield)
        {
            myAiState = BotLogic.aiState.flee;
        }
    }

    public void SetDifficulty(BotLogic.botDifficulty _difficulty)
    {
        difficulty = _difficulty;
        switch (difficulty)
        {
            case BotLogic.botDifficulty.easy:
                botFov = 30;
                minBombTime = 5;
                maxBombTime = 20;
                minGunTime = 0.5f;
                maxGunTime = 4;
                reactionTime = 2f;
                fleeChance = 1.0f/6;
                break;
            case BotLogic.botDifficulty.medium:
                botFov = 45;
                minBombTime = 3;
                maxBombTime = 10;
                minGunTime = 0;
                maxGunTime = 2;
                reactionTime = 0.3f;
                fleeChance = 1.0f / 10;
                break;
            case BotLogic.botDifficulty.hard:
                botFov = 90;
                minBombTime = 3;
                maxBombTime = 5;
                minGunTime = 0;
                maxGunTime = 1;
                reactionTime = 0.25f;
                fleeChance = 1.0f / 20;
                break;
            case BotLogic.botDifficulty.max:
                botFov = 90;
                minBombTime = 1;
                maxBombTime = 5;
                minGunTime = 0;
                maxGunTime = 0;
                reactionTime = 0;
                fleeChance = 0;
                break;
            default:
                break;
        }
    }

    void BotUpdate()
    {
        if (myAiState != oldAiState) { isNewAiState = true; oldAiState = myAiState; }
        else { isNewAiState = false; }
        botHandler.isVisible(transform.position, attackTarget, botFov);
        if (targetVisible && !botHandler.isVisible(transform.position, attackTarget, botFov))
        {
            targetVisible = false;
            //Debug.Log("cannot see");
        }
        else if(!targetVisible && botHandler.isVisible(transform.position, attackTarget, botFov))
        {
            targetVisible = true;
            //Debug.Log("can see");
        }

        myController.isBraking = false;
       
        switch (myAiState)
        {
            case BotLogic.aiState.wander:
                if (isNewAiState)
                {
                    Debug.Log("is wandering");
                    bufferAiState = BotLogic.aiState.wander;
                    navList = navSystem.GetRandomPath(transform.position);
                    botHandler.PlotCourse(ref navList);
                    audioSource.clip = wander[0];
                    audioSource.Play();
                }
                //if(botHandler.AtPosition())
                else if (botHandler.AtNavTarget(botFov, true, 10, 10))
                {
                    navList = navSystem.GetRandomPath(transform.position);
                    botHandler.PlotCourse(ref navList);
                }
                if (botHandler.BeginAttack(ref attackTarget, ref enemies, botFov)) { myAiState = BotLogic.aiState.attack; }
                break;
            case BotLogic.aiState.getAmmo:
                if (isNewAiState)
                {
                    Debug.Log("getting ammo");
                    if (!botHandler.GetNavItem(ref navTarget, ref ammoRestore))
                    {
                        myAiState = bufferAiState;
                        break;
                    }
                    navList = navSystem.GetPath(transform.position, navTarget.transform.position);
                    botHandler.PlotCourse(ref navList);
                    audioSource.clip = getAmmo[0];
                    audioSource.Play();
                }
                else if (botHandler.AtNavTarget()) { myAiState = bufferAiState; }
                break;
            case BotLogic.aiState.getShield:
                if (isNewAiState)
                {
                    Debug.Log("getting shield");
                    if (!botHandler.GetNavItem(ref navTarget, ref shieldRecharge)) { myAiState = bufferAiState; break; }
                    navList = navSystem.GetPath(transform.position, navTarget.transform.position);
                    botHandler.PlotCourse(ref navList);
                    audioSource.clip = getShield[0];
                    audioSource.Play();
                }
                else if (botHandler.AtNavTarget()) { myAiState = bufferAiState; }
                break;
            case BotLogic.aiState.attack:
                if (isNewAiState)
                {
                    Debug.Log("attacking!");
                    bufferAiState = BotLogic.aiState.attack;
                    audioSource.clip = attack[0];
                    audioSource.Play();
                }

                //this is where attack behavior comes in
                //options are:
                //1: try to ram player
                //2: keep at some operative distance
                //3: hit and run tactics
                //4: try to use special weapon
                //where possible bots should rotate to show players undamaged shields. maybe don't do explicily
                //some bots should be smart enough to investigate the lights and lasers of players
                //bots should also be able to hear player's engines and especially boosting
                switch (myAttackType)
                {
                    case BotLogic.attackState.rush:
                        botHandler.AtPosition(attackTarget.transform.position, true, true, true, 10);
                        if (!botHandler.isVisible(transform.position, attackTarget, botFov)) { myAiState = BotLogic.aiState.investigate; }
                        break;
                    case BotLogic.attackState.holdPosition:
                        break;
                    case BotLogic.attackState.hitAndRun:
                        break;
                    case BotLogic.attackState.nimble:
                        break;
                    case BotLogic.attackState.circler:
                        if (myController.isFire2Down) { myController.isFire2Up = true; myController.isFire2Down = false; }
                        else if(targetInCrosshair)
                        //else if (botHandler.isVisible(transform.position, attackTarget, 1))
                        {
                            if (bombTimer == -1) { bombTimer = Random.Range(minBombTime, maxBombTime); }
                            else if (bombTimer < 0)
                            {
                                bombTimer = -1;
                                myController.isFire2Down = true;
                            }
                            else { bombTimer -= Time.deltaTime; }
                            myController.isFire2Up = false;
                        }
                        if (myController.isFire1Down) { myController.isFire1Up = true; myController.isFire1Down = false; }
                        //else if (botHandler.isVisible(transform.position, attackTarget, 1))
                        else if (targetInCrosshair)
                        {
                            if (gunTimer == -1) { gunTimer = Random.Range(minGunTime, maxGunTime); }
                            else if (gunTimer < 0)
                            {
                                gunTimer = -1;
                                myController.isFire1Down = true;
                            }
                            else { gunTimer -= Time.deltaTime; }
                            myController.isFire1Up = false;
                        }
                        if (targetInCrosshair && Vector3.Magnitude(attackTarget.transform.position-transform.position)<30)
                        {
                            myController.isBraking = true;
                        }
                        else if (botHandler.AtPosition(attackTarget.transform.position, true, true, true, 30, false))
                        {
                            botHandler.AtPosition(attackTarget.transform.position, false, false, true, 0, false);
                            myController.isBraking = true;
                        }
                        else { needRandomVec = true; }
                        if (!botHandler.isVisible(transform.position, attackTarget, botFov)) { myAiState = BotLogic.aiState.investigate; }
                        break;
                    default:
                        break;
                }
                break;
            case BotLogic.aiState.investigate:
                if (isNewAiState)
                {
                    Debug.Log("investigating point");
                    navPoint = attackTarget.transform.position;
                    audioSource.clip = investigate[0];
                    audioSource.Play();
                }
                if (targetVisible) { myAiState = BotLogic.aiState.attack; }
                else if (botHandler.AtPosition(navPoint, true, true, true, 0.5f)) { myAiState = BotLogic.aiState.wander; }
                break;
            case BotLogic.aiState.guard:
                break;
            case BotLogic.aiState.flee:
                if (isNewAiState)
                {
                    Debug.Log("fleeing");
                    bufferAiState = BotLogic.aiState.wander;
                    navList = navSystem.GetRandomPath(transform.position);
                    botHandler.PlotCourse(ref navList);
                    audioSource.clip = flee[0];
                    audioSource.Play();
                }
                else if (botHandler.AtNavTarget(botFov, true, 10, 10))
                {
                    myAiState = BotLogic.aiState.wander;
                }
                break;
            case BotLogic.aiState.none:
                //botHandler.ResetReactionTime();
                //botHandler.isLookingAt(enemies[0].transform.position, 0, 1, true);
                //botHandler.AtPosition(enemies[0].transform.position, true, true, true, 10);
                break;
            default:
                break;
        }
    }
}
