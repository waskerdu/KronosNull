using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManagerScript : MonoBehaviour
{
    //the player manager keeps track of the active players at all times. It is created in the first scene and never deleted
    //it holds an instance of a controller manager and maintains lists of active players
    List<GameInput.Player> players = new List<GameInput.Player>();
    public GameInput.ControllerManager controllerManager;
    public int numPlayers = 0;
    public bool selectingPlayers = false;
    GameObject arenaMaster;

    void Start ()
    {
        //DontDestroyOnLoad(gameObject);
        controllerManager = new GameInput.ControllerManager(gameObject);
        //arenaMaster = GameObject.FindGameObjectsWithTag("ArenaMaster")[0];
    }

    void SetPauseMenu(bool show)
    {
        /*if (show != showPauseMenu && gameLaunched)
        {
            showPauseMenu = !showPauseMenu;
            if (showPauseMenu) { pauseMenu.SetActive(true); }
            else { pauseMenu.SetActive(false); }
        }*/
        arenaMaster.SendMessage("SetPauseMenu", show);
    }

    void Update()
    {
        if (arenaMaster == null)
        {
            arenaMaster = GameObject.FindGameObjectsWithTag("ArenaMaster")[0];
        }
        controllerManager.Update();
        //Debug.Log(Time.timeScale);
        int enabledIter = 0;
        for (int i = 0; i < controllerManager.players.Count; i++)
        {
            transform.GetChild(enabledIter).gameObject.SetActive(false);
            if (controllerManager.players[i].controller.isEnabled)
            {
                //activate relevant icon
                //transform.GetChild(enabledIter).gameObject.SetActive(true);
                //transform.GetChild(enabledIter).gameObject.GetComponent<Image>().color = controllerManager.factionColors[enabledIter];
                enabledIter++;
            }
        }
        numPlayers = enabledIter;
    }
}
