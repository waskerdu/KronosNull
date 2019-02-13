using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseAudioSource : MonoBehaviour {

    bool isPaused = false;
	
	// Update is called once per frame
	void Update ()
    {
		if (Time.timeScale==0 && !isPaused)
        {
            isPaused = true;
            GetComponent<AudioSource>().Pause();
        }
        else if(isPaused && Time.timeScale != 0)
        {
            GetComponent<AudioSource>().UnPause();
            isPaused = false;
        }
	}
}
