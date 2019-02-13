using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPointerScript : MonoBehaviour {

    GameInput.Controller controller;
    public Vector3 moveVec = Vector3.zero;
    public float speed = 1;
    Transform myTransform;

	// Use this for initialization
	void Start ()
    {
        myTransform = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        /*moveVec.x = controller.x;
        moveVec.y = controller.y;
        //if (moveVec.magnitude > 1) { moveVec.Normalize(); }
        moveVec *= speed;
        myTransform.localPosition += moveVec;*/
	}

    public void SetPointerController(ref GameInput.Controller nextController)
    {
        controller = nextController;
    }
}
