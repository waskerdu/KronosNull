using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour {
    //menu manager
    int numItems;
    int pointerIndex = 1;
    GameInput.ControllerManager controllerManager;
    public GameObject pointer;
	void Start ()
    {
        numItems = transform.childCount;
        controllerManager = GameObject.FindGameObjectsWithTag("PlayerManager")[0].GetComponent<PlayerManagerScript>().controllerManager;
        pointer.transform.SetParent(transform.GetChild(pointerIndex));
    }

    void BumpPointer(int bump)
    {
        pointerIndex += bump;
        if (pointerIndex == 0) { pointerIndex = numItems - 1; }
        if (pointerIndex == numItems) { pointerIndex = 1; }
        pointer.transform.SetParent(transform.GetChild(pointerIndex));
        pointer.transform.position = pointer.transform.parent.position;
    }
	
	void Update ()
    {
        if (controllerManager == null) { controllerManager = GameObject.FindGameObjectsWithTag("PlayerManager")[0].GetComponent<PlayerManagerScript>().controllerManager; }
        if (controllerManager.uiController.isDownDown) { BumpPointer(1); }
        if (controllerManager.uiController.isUpDown) { BumpPointer(-1); }
        if (controllerManager.uiController.isConfirmDown) { pointer.transform.parent.SendMessage("Confirm", SendMessageOptions.DontRequireReceiver); }
        if (controllerManager.uiController.isConfirmDown) { pointer.transform.parent.SendMessage("Bump", 1, SendMessageOptions.DontRequireReceiver); }
        if (controllerManager.uiController.isConfirmDown) { pointer.transform.parent.SendMessage("Bump", -1, SendMessageOptions.DontRequireReceiver); }
    }
}
