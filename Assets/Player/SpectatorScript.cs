using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorScript : MonoBehaviour {

    // Use this for initialization
    public float speed = 10;
    public float boostSpeed = 30;
    public float turnSpeed = 10;
    public GameInput.Controller controller;
    Rigidbody rb;
    public Vector3 velocity;
    public Vector3 spinVector;
    BotLogic.BotHandler botHandler;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetController(GameInput.Controller _controller)
    {
        controller = _controller;
    }

    void SetCameraRect(Rect rect)
    {
        GetComponent<Camera>().rect = rect;
    }

    void Update()
    {
        if (controller != null)
        {
            controller.Update();
            velocity = controller.movement;
            spinVector = controller.angle;
            spinVector = transform.localToWorldMatrix.MultiplyVector(spinVector * turnSpeed);
            velocity = transform.localToWorldMatrix.MultiplyVector(velocity * speed);
            rb.velocity = velocity;
            rb.angularVelocity = spinVector;
        }
        else
        {
            Debug.Log("null controller on spectator");
        }
    }
}
