using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextSelector : MonoBehaviour {
    GameText.TextContainer textContainer;
    //text selector gets a reference to a player object
    void SetText(int index)
    {
        transform.GetChild(0).GetComponent<Text>().text = textContainer.titles[index];
        transform.GetChild(1).GetComponent<Text>().text = textContainer.info[index];
    }
	void Start ()
    {
        textContainer = new GameText.TextContainer();
        SetText(1);
	}
	
	void Update ()
    {
		
	}
}
