using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class arenaGen : MonoBehaviour {
    [Header("Public Objects")]
    public GameObject cube;
    public GameObject player;
    public GameObject turret;
    public GameObject shieldRestore;
    public int numShieldRestore = 2;
    public GameObject ammoRestore;
    public int numAmmoRestore = 2;
    public GameObject navNode;
    public int numNavNode = 6;
    public GameObject playerSelector;
    List<GameObject> playerSelectors = new List<GameObject>();
    public bool useKeyboard = true;
    public GameObject chunk;
    public List<GameObject> chunks = new List<GameObject>();
    [Header("Game Launch Stuff")]
    public bool gameLaunched = false;
    bool selectingPlayers = false;
    public float gameLaunchTime = 3;
    [Header("Level Gen variables")]
    public int arenaWidth = 20;
    public int arenaHeight = 20;
    public int arenaDepth = 20;
    public MapType mapType = MapType.cross;
    public int thickness = 2;
    public float smoothness = 0.5f;
    public bool cavesque = false;
    public int minRooms = 1;
    public int minWidth = 4;
    public int maxWidth = 4;
    [Header("Ship Selection")]
    public BotLogic.botDifficulty botDiff = BotLogic.botDifficulty.medium;
    public int maxPlayers = 4;
    public int numOverride = -1;
    public int numPlayers = 1;
    public int playerShield = 3;
    public int numBots = 0;
    public int maxBots = 4;
    int numCameras = 0;
    public List<int> botsList = new List<int>();
    public bool useCamera = false;
    public GameObject[] classes = new GameObject[2];
    public GameObject spectator;
    public string[] classDescriptions = new string[2];
    Vector3 focalPoint;
    public GameObject pointer;
    List<GameInput.Player> players = new List<GameInput.Player>();
    public float[] sensitivities =
    {
        0.25f,
        0.5f,
        0.75f,
        1.0f,
        1.25f,
        1.5f,
        1.75f
    };
    List<List<Rect>> layouts = new List<List<Rect>>();
    //List<PointOfInterest> pointsOfInterest = new List<PointOfInterest>();
    public LevelGeneration.NavSystem navSystem;
    //these should probably be uneque by area. fine for right now
    public List<GameObject> actors = new List<GameObject>();
    List<GameObject> shieldRestoreStations = new List<GameObject>();//not public because I may not use
    List<GameObject> ammoRestoreStations = new List<GameObject>();
    public List<GameObject> mines = new List<GameObject>();//the only one that gets updated regularly, maybe use another
    public int playerIndex = -1;
    //GameMap.Map myMap;
    GameMap.ClusterMap myMap;
    //public int 
    [Header("Menu Stuff")]
    public int pointerIndex = 1;
    public int numElements = 9;
    public string botNumHeader = "Number of Enemy Bots: ";
    public string botDifficultyHeader = "Bot Difficulty: ";
    public int botDiffIndex = 0;
    public string[] botDifficultyArray = { "Easy", "Medium", "Hard", "Maximum" };
    public int mapTypeIndex = 0;
    public string mapTypeHeader = "Map Type: ";
    string[] mapTypeArray = { "Cross", "Tangle", "Warren", "Pillars", "Cube", "Sphere" };
    public int mapSizeIndex = 0;
    public string mapSizeHeader = "Map Size: ";
    public string[] mapSizeArray = { "Small", "Medium", "Large", "Huge" };
    bool showPauseMenu = false;
    public GameObject pauseMenu;
    int pauseIndex = 1;
    public GameObject pausePointer;
    public Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow, Color.black };
    GameInput.ControllerManager controllerManager;
    public bool hasControllerManager = false;
    public int shieldLevel = 2;
    int damageLevel = 6;
    float volume = 1;
    public float fallOff = 0.1f;

    bool isPaused = false;

    //LevelGeneration.VoxelMap myMap;
    //public int seed = 0

    public enum MapType{
        cross,
        tangle,
        warren,
        pillars,
        cube,
        sphere,
        race,
    }

    void Start()
    {
        //begin controller manager
        //controllerManager = GameObject.FindGameObjectsWithTag("PlayerManager")[0].GetComponent<PlayerManagerScript>().controllerManager;
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("PlayerManager").Length; i++)
        {
            if (i == 0)
            {
                controllerManager = GameObject.FindGameObjectsWithTag("PlayerManager")[i].GetComponent<PlayerManagerScript>().controllerManager;
                hasControllerManager = true;
            }
            else
            {
                //GameObject.FindGameObjectsWithTag("PlayerManager")[i].
                Destroy(GameObject.FindGameObjectsWithTag("PlayerManager")[i]);
            }
        }
        navSystem = new LevelGeneration.NavSystem();
        BuildLayouts();
        //setup ui controller
        for (int i = 0; i < 5; i++)
        {
            playerSelectors.Add(Instantiate(playerSelector, transform));
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SetPauseMenu(bool show)
    {
        if (show != showPauseMenu && gameLaunched)
        {
            showPauseMenu = !showPauseMenu;
            if (showPauseMenu) { pauseMenu.SetActive(true); }
            else { pauseMenu.SetActive(false); }
        }
    }

    void MakeDirty(int playerIndex)
    {
        controllerManager.dirty = true;
        players[playerIndex].sensitivity = sensitivities[playerSelectors[playerIndex].GetComponent<ClassSelectorScript>().sensitivityIndex];
        players[playerIndex].alligance = playerSelectors[playerIndex].GetComponent<ClassSelectorScript>().colorIndex;
        players[playerIndex].color = playerSelectors[playerIndex].GetComponent<ClassSelectorScript>().colors[players[playerIndex].alligance];
        for (int i = 0; i < playerSelectors[playerIndex].GetComponent<ClassSelectorScript>().numAlliedBots; i++)
        {
            botsList.Add(players[playerIndex].alligance);
        }
    }

    void MenuUpdate()
    {
        if (controllerManager.uiController.isDownDown) { MovePointer(1); }
        if (controllerManager.uiController.isUpDown) { MovePointer(-1); }
        switch (pointerIndex)
        {
            case 2:
                if (controllerManager.uiController.isLeftDown) { ChangeNumBots(-1); }
                if (controllerManager.uiController.isRightDown) { ChangeNumBots(1); }
                break;
            case 3:
                if (controllerManager.uiController.isLeftDown) { ChangeBotDiff(-1); }
                if (controllerManager.uiController.isRightDown) { ChangeBotDiff(1); }
                break;
            case 4:
                if (controllerManager.uiController.isLeftDown) { ChangeMapType(-1); }
                if (controllerManager.uiController.isRightDown) { ChangeMapType(1); }
                break;
            case 5:
                if (controllerManager.uiController.isLeftDown) { ChangeMapSize(-1); }
                if (controllerManager.uiController.isRightDown) { ChangeMapSize(1); }
                break;
            case 6:
                if (controllerManager.uiController.isLeftDown) { ChangeNumShields(-1); }
                if (controllerManager.uiController.isRightDown) { ChangeNumShields(1); }
                break;
            case 7:
                if (controllerManager.uiController.isLeftDown) { ChangeNumAmmo(-1); }
                if (controllerManager.uiController.isRightDown) { ChangeNumAmmo(1); }
                break;
            case 8:
                if (controllerManager.uiController.isLeftDown) { ChangeShieldLevel(-1); }
                if (controllerManager.uiController.isRightDown) { ChangeShieldLevel(1); }
                break;
            case 9:
                if (controllerManager.uiController.isLeftDown) { ChangeDamageLevel(-1); }
                if (controllerManager.uiController.isRightDown) { ChangeDamageLevel(1); }
                break;
            case 10:
                if (controllerManager.uiController.isConfirmDown) { Application.Quit(); }
                break;
            default:
                break;
        }
    }

    void ChangeDamageLevel(int change)
    {
        damageLevel = Mathf.Clamp(damageLevel + change, 1, 11);
        pointer.transform.parent.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Weapon Damage Level:\n" + damageLevel * 10 + "% Hull Damage";
    }

    void ChangeShieldLevel(int change)
    {

        shieldLevel = Mathf.Clamp(shieldLevel + change, -1, 3);
        if (shieldLevel == -1)
        {
            pointer.transform.parent.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Ship Shield Level: None";
        }
        else { pointer.transform.parent.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Ship Shield Level: " + shieldLevel; }
    }

    void ChangeNumAmmo(int change)
    {
        numAmmoRestore = Mathf.Clamp(numAmmoRestore+change, 0, 10);
        pointer.transform.parent.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Number of Ammo Drops: " + numAmmoRestore;
    }

    void ChangeNumShields(int change)
    {
        numShieldRestore = Mathf.Clamp(numShieldRestore+change, 0, 10);
        pointer.transform.parent.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Number of Shield Recharges: " + numShieldRestore;
    }

    int Wrap(int val, int min, int max)
    {
        if (val < min) { val = max; }
        else if (val > max) { val = min; }
        return val;
    }

    void ChangeMapSize(int change)
    {
        mapSizeIndex = Wrap(mapSizeIndex + change, 0, mapSizeArray.Length - 1);
        pointer.transform.parent.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = mapSizeHeader + mapSizeArray[mapSizeIndex];
    }

    void ChangeMapType(int change)
    {
        mapTypeIndex += change;
        mapTypeIndex = Wrap(mapTypeIndex, 0, mapTypeArray.Length - 1);
        pointer.transform.parent.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = mapTypeHeader + mapTypeArray[mapTypeIndex];
    }

    void ChangeBotDiff(int change)
    {
        botDiffIndex += change;
        if (botDiffIndex < 0) { botDiffIndex = botDifficultyArray.Length - 1; }
        if (botDiffIndex > botDifficultyArray.Length - 1) { botDiffIndex = 0; }
        pointer.transform.parent.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = botDifficultyHeader + botDifficultyArray[botDiffIndex];
    }

    void ChangeNumBots(int move)
    {
        numBots += move;
        numBots = Mathf.Clamp(numBots, 0, maxBots);
        pointer.transform.parent.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = botNumHeader + numBots;
    }

    void MovePointer(int move)
    {
        pointerIndex += move;
        pointerIndex = Wrap(pointerIndex, 1, numElements);
        pointer.transform.SetParent(transform.GetChild(0).GetChild(0).GetChild(pointerIndex));
        pointer.transform.position = pointer.transform.parent.position;
    }

    void Update()
    {
        //menu music
        if (!transform.GetChild(0).gameObject.GetComponent<AudioSource>().isPlaying && !gameLaunched && volume > 0)
        {
            transform.GetChild(0).gameObject.GetComponent<AudioSource>().Play();
        }
        else if(!transform.GetChild(0).gameObject.GetComponent<AudioSource>().isPlaying && volume > 0)
        {
            transform.GetChild(0).gameObject.GetComponent<AudioSource>().Play();
        }
        else if (gameLaunched && volume > 0)
        {
            volume -= Time.deltaTime * fallOff;
            if (volume < 0) { volume = 0; }
            transform.GetChild(0).gameObject.GetComponent<AudioSource>().volume = volume;
        }

        //controllerManager.Update();
        if (controllerManager == null)
        {
            controllerManager = GameObject.FindGameObjectsWithTag("PlayerManager")[0].GetComponent<PlayerManagerScript>().controllerManager;
        }
        if (selectingPlayers)
        {
            int playersReady = 0;
            for (int i = 0; i < playerSelectors.Count; i++)
            {
                if (playerSelectors[i].GetComponent<ClassSelectorScript>().launched) { playersReady++; }
            }
            gameLaunchTime -= Time.deltaTime;
            if (gameLaunchTime < 0) { LaunchMatch(); }
            if (controllerManager.dirty)
            {
                numPlayers = controllerManager.GetPlayers(ref players);
                //Debug.Log(controllerManager.playersReady);
                controllerManager.dirty = false;
                if (controllerManager.playersEnabled == playersReady && numPlayers>0) { LaunchMatch(); }
                else if (numPlayers == 0)
                {
                    transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                    foreach (GameObject selector in playerSelectors)
                    {
                        selector.SendMessage("ResetPointer");
                        selector.GetComponent<ClassSelectorScript>().canWrite = false;
                    }
                    selectingPlayers = false;
                }
                UpdateSelectors();
            }
        }
        else if (!gameLaunched)
        {
            MenuUpdate();
            if (controllerManager.dirty)
            {
                numPlayers = controllerManager.GetPlayers(ref players);
                controllerManager.dirty = false;
            }
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].controller.isConfirmDown && pointerIndex == 1)
                {
                    BeginPlayerSelect(i);
                    transform.GetChild(0).GetChild(0).gameObject.SetActive(false);//disables main menu
                    numPlayers = 1;
                    break;
                }
            }
            botDiff = (BotLogic.botDifficulty)botDiffIndex;
            mapType = (MapType)mapTypeIndex;
            arenaHeight = 0;
            arenaDepth = 0;
            switch (mapType)
            {
                case MapType.cross:
                    switch (mapSizeIndex)
                    {
                        case 0:
                            arenaWidth = 20;
                            thickness = 4;
                            break;
                        case 1:
                            arenaWidth = 40;
                            thickness = 6;
                            break;
                        case 2:
                            arenaWidth = 60;
                            thickness = 8;
                            break;
                        case 3:
                            arenaWidth = 100;
                            thickness = 10;
                            break;
                        default:
                            break;
                    }
                    break;
                case MapType.tangle:
                    switch (mapSizeIndex)
                    {
                        case 0:
                            arenaWidth = 20;
                            thickness = 4;
                            break;
                        case 1:
                            arenaWidth = 40;
                            thickness = 6;
                            break;
                        case 2:
                            arenaWidth = 60;
                            thickness = 8;
                            break;
                        case 3:
                            arenaWidth = 100;
                            thickness = 10;
                            break;
                        default:
                            break;
                    }
                    break;
                case MapType.warren:
                    switch (mapSizeIndex)
                    {
                        case 0:
                            arenaWidth = 20;
                            thickness = 4;
                            break;
                        case 1:
                            arenaWidth = 40;
                            thickness = 6;
                            break;
                        case 2:
                            arenaWidth = 60;
                            thickness = 8;
                            break;
                        case 3:
                            arenaWidth = 100;
                            thickness = 10;
                            break;
                        default:
                            break;
                    }
                    break;
                case MapType.pillars:
                    switch (mapSizeIndex)
                    {
                        case 0:
                            arenaWidth = 20;
                            thickness = 4;
                            break;
                        case 1:
                            arenaWidth = 40;
                            thickness = 6;
                            break;
                        case 2:
                            arenaWidth = 60;
                            thickness = 8;
                            break;
                        case 3:
                            arenaWidth = 100;
                            thickness = 10;
                            break;
                        default:
                            break;
                    }
                    break;
                case MapType.cube:
                    switch (mapSizeIndex)
                    {
                        case 0:
                            arenaWidth = 20;
                            break;
                        case 1:
                            arenaWidth = 40;
                            break;
                        case 2:
                            arenaWidth = 60;
                            break;
                        case 3:
                            arenaWidth = 100;
                            break;
                        default:
                            break;
                    }
                    break;
                case MapType.sphere:
                    switch (mapSizeIndex)
                    {
                        case 0:
                            arenaWidth = 20;
                            break;
                        case 1:
                            arenaWidth = 40;
                            break;
                        case 2:
                            arenaWidth = 60;
                            break;
                        case 3:
                            arenaWidth = 100;
                            break;
                        default:
                            break;
                    }
                    break;
                case MapType.race:
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (showPauseMenu)
            {
                if (controllerManager.uiController.isDownDown) { MovePausePointer(1); }
                if (controllerManager.uiController.isUpDown) { MovePausePointer(-1); }
                if (pauseIndex == 1 && controllerManager.uiController.isConfirmDown) { SetPauseMenu(false); controllerManager.isPaused = false; }
                if (pauseIndex == 2 && controllerManager.uiController.isConfirmDown)
                {
                    GameObject.FindGameObjectsWithTag("PlayerManager")[0].GetComponent<PlayerManagerScript>().selectingPlayers = true;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                if (pauseIndex == 3 && controllerManager.uiController.isConfirmDown) { Application.Quit(); }
            }
        }
        //if (Input.GetKeyDown(KeyCode.Tab)) { Application.Quit(); }
        //if (Input.GetKeyDown(KeyCode.T)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }

    void MovePausePointer(int move)
    {
        pauseIndex = Wrap(pauseIndex + move, 1, 3);
        pausePointer.transform.SetParent(pauseMenu.transform.GetChild(pauseIndex));
        pausePointer.transform.position = pausePointer.transform.parent.position;
        //controllerManager.isPaused = false;
    }

    void BeginPlayerSelect(int first)
    {
        selectingPlayers = true;
        //players[first].controller.isEnabled = true;
        for (int i = 0; i < 5; i++)
        {
            playerSelectors[i].SetActive(true);
            playerSelectors[i].SendMessage("SetIndex", i);
            playerSelectors[i].GetComponent<ClassSelectorScript>().canWrite = true;
        }
        playerSelectors[first].SendMessage("SetRectImmediate", layouts[0][0]);
        UpdateSelectors();
        controllerManager.dirty = true;
    }

    void UpdateSelectors()
    {
        int iter = 0;
        ClassSelectorScript script;
        for (int i = 0; i < players.Count; i++)
        {
            playerSelectors[i].SetActive(true);
            if (players[i].controller.isEnabled)
            {
                script = playerSelectors[i].GetComponent<ClassSelectorScript>();
                playerSelectors[i].SetActive(true);
                if (iter % 2 == 0) { playerSelectors[i].SendMessage("SetOldRect", new Rect(0, 0, 0, 0)); }
                else { playerSelectors[i].SendMessage("SetOldRect", new Rect(1, 0, 0, 0)); }
                playerSelectors[i].SendMessage("SetRect", layouts[numPlayers - 1][iter]);
                script.SetController(players[i].controller);
                iter++;
            }
            else
            {
                if (iter % 2 == 0) { playerSelectors[i].SendMessage("SetRect", new Rect(0, 0, 0, 0)); }
                else { playerSelectors[i].SendMessage("SetRect", new Rect(1, 0, 0, 0)); }
            }
        }
    }

    void BuildLayouts()
    {
        layouts.Add(new List<Rect>());
        layouts.Add(new List<Rect>());
        layouts.Add(new List<Rect>());
        layouts.Add(new List<Rect>());
        //singleplayer
        layouts[0].Add(new Rect(0, 0, 1, 1));
        //2 player
        layouts[1].Add(new Rect(0.0f, 0.0f, 0.5f, 1.0f));
        layouts[1].Add(new Rect(0.5f, 0.0f, 0.5f, 1.0f));
        //3 player
        layouts[2].Add(new Rect(0.0f, 0.5f, 0.5f, 0.5f));
        layouts[2].Add(new Rect(0.5f, 0.5f, 0.5f, 0.5f));
        layouts[2].Add(new Rect(0.25f, 0.0f, 0.5f, 0.5f));
        //4 player
        layouts[3].Add(new Rect(0.0f, 0.5f, 0.5f, 0.5f));
        layouts[3].Add(new Rect(0.5f, 0.5f, 0.5f, 0.5f));
        layouts[3].Add(new Rect(0.0f, 0.0f, 0.5f, 0.5f));
        layouts[3].Add(new Rect(0.5f, 0.0f, 0.5f, 0.5f));
    }

    void LaunchMatch()
    {
        gameLaunched = true;
        selectingPlayers = false;
        controllerManager.canWrite = false;
        //disable main camera
        transform.GetChild(0).GetComponent<Camera>().enabled = false;
        BuildLevel();
        Cursor.lockState = CursorLockMode.Locked;
        PlacePlayers();
        PlaceBots(numBots);

        Vector3 tempPos;
        
        for (int i = 0; i < numNavNode; i++)
        {
            navSystem.AddPoint(Instantiate(navNode, myMap.FreePosition() * transform.localScale.x, Quaternion.identity));
        }
        EnsureConnectivity();
        for (int i = 0; i < numShieldRestore; i++)
        {
            tempPos = myMap.FreePosition() * transform.localScale.x;
            shieldRestoreStations.Add(Instantiate(shieldRestore, tempPos, Quaternion.identity));
            navSystem.AddPoint(Instantiate(navNode, tempPos, Quaternion.identity), LevelGeneration.poiType.shield, 1000);
        }
        //add ammo restore stations
        for (int i = 0; i < numShieldRestore; i++)
        {
            tempPos = myMap.FreePosition() * transform.localScale.x;
            ammoRestoreStations.Add(Instantiate(ammoRestore, tempPos, Quaternion.identity));
            navSystem.AddPoint(Instantiate(navNode, tempPos, Quaternion.identity), LevelGeneration.poiType.ammo, 1000);
        }
        EnsureConnectivity();
        navSystem.CalculatePathfinding();
        foreach (GameObject actor in actors)
        {
            actor.GetComponent<movement>().shieldRecharge = shieldRestoreStations;
            actor.GetComponent<movement>().ammoRestore = ammoRestoreStations;
            actor.GetComponent<movement>().SetActors(actors);
        }

        for (int i = 0; i < playerSelectors.Count; i++)
        {
            playerSelectors[i].transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    void BuildLevel()
    {
        switch (mapType)
        {
            case MapType.cube:
                myMap = new GameMap.ClusterMap(arenaWidth, arenaHeight, arenaDepth);
                myMap.MakeBox();
                break;
            case MapType.cross:
                myMap = new GameMap.ClusterMap(arenaWidth, arenaHeight, arenaDepth);
                myMap.MakeCross(thickness);
                break;
            case MapType.warren:
                myMap = new GameMap.ClusterMap(arenaWidth);
                myMap.MakeCross(arenaWidth - thickness * 2);
                break;
            case MapType.sphere:
                myMap = new GameMap.ClusterMap(arenaWidth, arenaHeight, arenaDepth);
                myMap.MakeSphere();
                break;
            case MapType.pillars:
                myMap = new GameMap.ClusterMap(arenaWidth, arenaHeight, arenaDepth);
                myMap.MakePillars(thickness);
                break;
            case MapType.tangle:
                myMap = new GameMap.ClusterMap(arenaWidth, arenaHeight, arenaDepth);
                myMap.MakeTangle(thickness);
                break;
            case MapType.race:
                myMap = new GameMap.ClusterMap(6, 6, 160);
                myMap.MakeBox();
                break;
            default:
                break;
        }
        myMap.CopyToMarching();
        if (cavesque) { myMap.MakeCavesque(); }
        myMap.SmoothMesh(smoothness);
        myMap.MakeChunks();
        GameObject tempChunk;
        MeshGenerator.MarchingMesh marchingMesh;
        for (int i = 0; i < myMap.chunkList.Count; i++)
        {
            tempChunk = Instantiate(chunk, transform);
            tempChunk.SetActive(true);
            tempChunk.transform.position = myMap.chunkList[i].position * transform.localScale.x;
            marchingMesh = new MeshGenerator.MarchingMesh(ref myMap.chunkList[i].marchingGrid);
            tempChunk.GetComponent<MeshFilter>().mesh = marchingMesh.GetMarchingMesh();
            tempChunk.GetComponent<MeshCollider>().sharedMesh = tempChunk.GetComponent<MeshFilter>().sharedMesh;
        }

        focalPoint = new Vector3(arenaWidth * cube.transform.localScale.x * 0.5f, arenaWidth * cube.transform.localScale.y * 0.5f, arenaWidth * cube.transform.localScale.z * 0.5f);
    }

    void PlacePlayers()
    {
        GameObject nextPlayer;
        numCameras = numPlayers;
        if (useCamera) { numCameras += numBots; }
        if (numCameras > maxPlayers) { numCameras = maxPlayers; }
        int num = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].controller.isEnabled)
            {
                //nextPlayer = Instantiate(classes[playerSelectors[i].GetComponent<ClassSelectorScript>().GetClassIndex()]);
                if (players[i].alligance == 4)
                {
                    players[i].alligance = -2;
                    nextPlayer = Instantiate(spectator);
                    nextPlayer.SetActive(true);
                    nextPlayer.transform.position = myMap.FreePosition() * transform.localScale.x;
                    nextPlayer.transform.LookAt(focalPoint);
                    nextPlayer.SendMessage("SetCameraRect", layouts[numCameras - 1][num]);
                    nextPlayer.GetComponent<SpectatorScript>().SetController(players[i].GetController());
                    /*nextPlayer.GetComponent<movement>().SetController(players[i].GetController());
                    nextPlayer.GetComponent<movement>().sensitivity *= players[i].sensitivity;
                    nextPlayer.GetComponent<movement>().aimAssist = players[i].autoAim;*/
                }
                else
                {
                    nextPlayer = Instantiate(classes[0]);
                    actors.Add(nextPlayer);
                    nextPlayer.SetActive(true);
                    nextPlayer.transform.position = myMap.FreePosition() * transform.localScale.x;
                    nextPlayer.transform.LookAt(focalPoint);
                    nextPlayer.SendMessage("SetCameraRect", layouts[numCameras - 1][num]);
                    nextPlayer.GetComponent<movement>().allegiance = playerIndex;
                    nextPlayer.GetComponent<movement>().SetController(players[i].GetController());
                    nextPlayer.GetComponent<movement>().sensitivity *= players[i].sensitivity;
                    nextPlayer.GetComponent<movement>().allegiance = players[i].alligance;
                    nextPlayer.GetComponent<movement>().aimAssist = players[i].autoAim;
                    nextPlayer.GetComponent<movement>().color = players[i].color;
                    nextPlayer.BroadcastMessage("SetShieldLevel", shieldLevel);
                    nextPlayer.BroadcastMessage("SetDamage", damageLevel * 10);
                }
                
                num++;
            }
        }
    }

    void PlacePlayers(int num)
    {
        GameObject nextPlayer;
        num = numPlayers;
        if (numOverride != -1){ num = numOverride; }
        numCameras = num;
        if (useCamera) { numCameras += numBots; }
        if (numCameras > maxPlayers) { numCameras = maxPlayers; }
        for (int i = 0; i < num; i++)
        {
            nextPlayer = Instantiate(classes[0]);
            actors.Add(nextPlayer);
            nextPlayer.SetActive(true);
            nextPlayer.transform.position = myMap.FreePosition() * transform.localScale.x;
            nextPlayer.transform.LookAt(focalPoint);
            nextPlayer.SendMessage("SetCameraRect", layouts[numCameras-1][i]);
            nextPlayer.transform.GetChild(5).GetComponent<ShieldScript>().shieldLevel = playerShield;
            nextPlayer.GetComponent<movement>().allegiance = playerIndex;
            if (numOverride == -1) { nextPlayer.GetComponent<movement>().SetController(players[i].GetController()); }
            else
            {
                GameInput.ControllerType type = GameInput.ControllerType.xbox;
                if(Input.GetJoystickNames()[i]=="Wireless Controller") { type = GameInput.ControllerType.dualShock; }
                nextPlayer.GetComponent<movement>().SetController(new GameInput.Controller(type, i + 1));
                nextPlayer.GetComponent<movement>().selfUpdate = true;
            }
            
        }
    }

    void PlaceBots(int num)
    {
        GameObject nextPlayer;
        for (int i = 0; i < num; i++)
        {
            botsList.Add(4);
        }
        num = botsList.Count;
        for (int i = 0; i < num; i++)
        {
            nextPlayer = Instantiate(classes[0]);
            actors.Add(nextPlayer);
            nextPlayer.SetActive(true);
            nextPlayer.GetComponent<movement>().isBot = true;
            nextPlayer.transform.position = myMap.FreePosition() * transform.localScale.x;
            nextPlayer.transform.LookAt(focalPoint);
            /*if (useCamera && numPlayers + i < maxPlayers)
            {
                nextPlayer.SendMessage("SetCameraRect", layouts[numCameras - 1][i+numPlayers]);
            }
            else { nextPlayer.transform.GetChild(1).GetComponent<Camera>().enabled = false; }*/
            nextPlayer.transform.GetChild(2).GetComponent<Camera>().enabled = false;
            //nextPlayer.GetComponent<movement>().SetController(new GameInput.Controller(GameInput.ControllerType.xbox, -1));
            nextPlayer.GetComponent<movement>().allegiance = botsList[i];
            nextPlayer.GetComponent<movement>().difficulty = botDiff;
            nextPlayer.GetComponent<movement>().color = colors[botsList[i]];
            nextPlayer.BroadcastMessage("SetShieldLevel", shieldLevel);
            nextPlayer.BroadcastMessage("SetDamage", damageLevel * 10);
        }
    }

    void DisablePlayerSelect(int index)
    {
        playerSelectors[index].SetActive(false);
        playerSelectors.Add(playerSelectors[index]);
        playerSelectors.RemoveAt(index);
    }

    void EnsureConnectivity()
    {
        for (int i = 0; i < 100; i++)//if it is not broken we have a map connectivity problem.
        {
            if (navSystem.IsConnected()) { break; }
            else { navSystem.AddPoint(Instantiate(navNode, myMap.FreePosition() * transform.localScale.x, Quaternion.identity)); }
            navSystem.GenerateNavMesh();
        }
    }  
}
