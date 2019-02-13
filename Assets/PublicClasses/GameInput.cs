using System.Collections.Generic;
using UnityEngine;

namespace GameInput
{
    public enum ControllerType
    {
        keyboard,
        xbox,
        dualShock,
        aggregate
    }

    public class Player
    {
        public int playerIndex;
        public Controller controller;
        public int classIndex;
        public bool dirty;
        public float sensitivity;
        public int alligance;
        public bool autoAim = true;
        public Color color = Color.white;
        public Player(Controller _controller, int _playerIndex = 1, int _classIndex = 0)
        {
            playerIndex = _playerIndex;
            controller = _controller;
            classIndex = _classIndex;
            dirty = false;
        }
        public Controller GetController() { return controller; }
        public void SetDirty(bool _dirty) { dirty = _dirty; }
        public void SetControllerIndex(int _playerIndex)
        {
            playerIndex = _playerIndex;
            controller.SetController(playerIndex);
        }
    }

    public class Button
    {
        string axisString;
        bool state = false;
        bool lastState = false;
        float threshold = 0.5f;
        bool invert = true;
        float val;
        public Button(string _axisString, float _threshold, bool _invert)
        {
            axisString = _axisString;
            threshold = _threshold;
            invert = _invert;
        }
        public void Update()
        {
            lastState = state;
            val = Input.GetAxis(axisString);
            if (invert) { val = - val; }
            if (val > threshold) { state = true; }
            else { state = false; }
        }
        public bool Get()//must be called every frame
        {
            return state;
        }
        public bool GetDown()
        {
            if (state && !lastState) { return true; }
            else { return false; }
        }
        public bool GetUp()
        {
            if (!state && lastState) { return true; }
            else { return false; }
        }
    }

    public class Axis
    {
        string posString;
        string negString;
        bool isButton;
        public Axis(string _posString, string _negString, bool _isButton = true)
        {
            posString = _posString;
            negString = _negString;
            isButton = _isButton;
        }
        public float GetValue()
        {
            float output = 0;
            if (isButton)
            {
                if (Input.GetButton(posString)) { output++; }
                if (Input.GetButton(negString)) { output--; }
            }
            else
            {
                //ugly hack, just used for dual shock 4 triggers
                output = (Input.GetAxis(posString) / 2 + 0.5f);
                output -= (Input.GetAxis(negString) / 2 + 0.5f);
            }
            return output;
        }
    }

    public class Controller
    {
        public ControllerType myType;

        List<Axis> axes = new List<Axis>();
        public bool isConnected = false;
        public bool isEnabled = false;
        public bool isReady = false;
        public int enabledState = 0;//0: not enabled 1: enabled 2: ready
        public int index;
        public float deadZone = 0.25f;
        public float outerDeadZone = 1;
        public float lookSensitivity = 1;
        public float lookCurve = 0.5f;
        public float moveSensitivity = 1;
        public float moveCurve = 0.5f;
        public bool useLookCurve = true;
        public bool useMoveCurve = true;
        public bool useLookFilter = true;

        public Vector3 movement = Vector3.zero;

        string xString;
        int xAxis = -1;
        string yString;
        int yAxis = -1;
        string zString;
        int zAxis = -1;

        public Vector3 angle = Vector3.zero;
        Vector2 tempAngle = Vector2.zero;

        string pitchString;
        int pitchAxis = -1;
        public bool invertY = false;
        string yawString;
        int yawAxis = -1;
        string rollString;
        int rollAxis = -1;

        public Vector2 ui = Vector2.zero;
        string uixString;
        string uiyString;

        string boostString;
        public bool isBoosting;
        public bool isBoostingDown;
        public bool isBoostingUp;

        string brakeString;
        public bool isBraking;
        public bool isBrakingDown;
        public bool isBrakingUp;

        string fire1String;
        public bool isFire1;
        public bool isFire1Down;
        public bool isFire1Up;

        string fire2String;
        public bool isFire2;
        public bool isFire2Down;
        public bool isFire2Up;

        string pauseString;
        public bool isPause;
        public bool isPauseDown;
        public bool isPauseUp;

        string invertString;
        public bool isInvert;
        public bool isInvertDown;
        public bool isInvertUp;

        string stealthString;
        public bool isStealth;
        public bool isStealthDown;
        public bool isStealthUp;

        string confirmString;
        public bool isConfirm;
        public bool isConfirmDown;
        public bool isConfirmUp;

        string cancelString;
        public bool isCancel;
        public bool isCancelDown;
        public bool isCancelUp;

        Button leftButton;
        string leftString;
        public bool isLeft;
        public bool isLeftDown;
        public bool isLeftUp;

        Button rightButton;
        string rightString;
        public bool isRight;
        public bool isRightDown;
        public bool isRightUp;

        Button upButton;
        string upString;
        public bool isUp;
        public bool isUpDown;
        public bool isUpUp;

        Button downButton;
        string downString;
        public bool isDown;
        public bool isDownDown;
        public bool isDownUp;

        public void Filter(ref Vector2 raw, bool smooth)
        {
            //handle deadzones
            if (raw.magnitude < deadZone) { raw *= 0; }
            else if (raw.magnitude > outerDeadZone) { raw.Normalize(); }
            Vector2 subVec = Vector2.zero;
            if (raw.magnitude != 0)
            {
                //post deadzone scaling
                subVec = raw.normalized;
                subVec *= deadZone;
                raw -= subVec;
                raw *= 1 - deadZone;/**/
                //put input on curve
                if (smooth)
                {
                    float mag = raw.magnitude;
                    mag = Mathf.Pow(mag, lookCurve);
                    raw = raw.normalized * mag;/**/
                }
            }
        }

        public void Filter(ref Vector3 raw, bool smooth)
        {
            if (raw.magnitude < deadZone) { raw *= 0; }
            else if (raw.magnitude > outerDeadZone) { raw.Normalize(); }
            Vector3 subVec = Vector2.zero;
            if (raw.magnitude != 0)
            {
                subVec = raw.normalized;
                subVec *= deadZone;
                raw -= subVec;
                raw *= 1 - deadZone;
                if (smooth)
                {
                    float mag = raw.magnitude;
                    mag = Mathf.Pow(mag, lookCurve);
                    raw = raw.normalized * mag;/**/
                }
            }
        }

        public Controller(ControllerType nextType, int controllerIndex)
        {
            myType = nextType;
            SetController(controllerIndex);
        }

        public void SetController(ControllerType type, int controllerIndex)
        {
            myType = type;
            SetController(controllerIndex);
        }

        public void SetController(int controllerIndex)
        {
            index = controllerIndex;
            string controllerNum = controllerIndex.ToString();
            axes.Clear();
            if (myType == ControllerType.xbox)
            {
                xString = "axisX_" + controllerNum;
                yString = "axisY_" + controllerNum;
                zString = "axis3_" + controllerNum;

                yawString = "axis4_" + controllerNum;
                pitchString = "axis5_" + controllerNum;
                rollAxis = 0;
                axes.Add(new Axis("button9_" + controllerNum, "button8_" + controllerNum));

                boostString = "button0_" + controllerNum;
                brakeString = "button2_" + controllerNum;
                fire1String = "button5_" + controllerNum;
                fire2String = "button4_" + controllerNum;
                pauseString = "button7_" + controllerNum;
                invertString = "button3_" + controllerNum;
                stealthString = "button3_" + controllerNum;
                confirmString = "button0_" + controllerNum;
                cancelString = "button1_" + controllerNum;
                leftButton = new Button("axis6_" + controllerNum, 0.5f, true);
                rightButton = new Button("axis6_" + controllerNum, 0.5f, false);
                leftString = fire2String;
                rightString = fire1String;
                upButton = new Button("axis7_" + controllerNum, 0.5f, false);
                downButton = new Button("axis7_" + controllerNum, 0.5f, true);
            }
            else if (myType == ControllerType.keyboard)
            {
                controllerNum = "0";
                xString = "axisX_" + controllerNum;
                yString = "axisY_" + controllerNum;
                zString = "axis3_" + controllerNum;

                yawString = "mouseX";
                pitchString = "mouseY";
                rollString = "axis4_" + controllerNum;

                boostString = "button0_" + controllerNum;
                brakeString = "button1_" + controllerNum;
                fire1String = "button2_" + controllerNum;
                fire2String = "button3_" + controllerNum;
                pauseString = "button4_" + controllerNum;
                invertString = "button5_" + controllerNum;
                stealthString = "button6_" + controllerNum;
                confirmString = "button7_" + controllerNum;
                cancelString = "button8_" + controllerNum;
                leftString = "button9_" + controllerNum;
                rightString = "button10_" + controllerNum;
                upString = "button11_" + controllerNum;
                downString = "button12_" + controllerNum;
                deadZone = 0;
            }
            else if (myType == ControllerType.dualShock)
            {
                xString = "axisX_" + controllerNum;
                yString = "axisY_" + controllerNum;
                zAxis = 0;
                axes.Add(new Axis("axis5_" + controllerNum, "axis4_" + controllerNum, false));

                yawString = "axis3_" + controllerNum;
                pitchString = "axis6_" + controllerNum;//axis 6 is right stick y axis
                rollAxis = 1;
                axes.Add(new Axis("button11_" + controllerNum, "button10_" + controllerNum));

                boostString = "button1_" + controllerNum;
                brakeString = "button0_" + controllerNum;
                fire1String = "button5_" + controllerNum;
                fire2String = "button4_" + controllerNum;
                pauseString = "button9_" + controllerNum;
                //invertString = "button3_" + controllerNum;
                stealthString = "button3_" + controllerNum;
                confirmString = "button1_" + controllerNum;
                cancelString = "button2_" + controllerNum;
                leftString = fire2String;
                rightString = fire1String;
                leftButton = new Button("axis7_" + controllerNum, 0.5f, true);
                rightButton = new Button("axis7_" + controllerNum, 0.5f, false);
                upButton = new Button("axis8_" + controllerNum, 0.5f, false);
                downButton = new Button("axis8_" + controllerNum, 0.5f, true);
            }
        }

        public void SetInvert(bool nextVal) { invertY = nextVal; }
        public void ToggleInvert() { invertY = !invertY; }

        public void Update()
        {
            if (!isConnected) { return; }
            if (index == -1) { Debug.LogWarning("you tried to update a bot controller silly"); return; }
            if (xAxis == -1) { movement.x = Input.GetAxisRaw(xString); }
            else { movement.x = axes[xAxis].GetValue(); }

            if (yAxis == -1) { movement.y = Input.GetAxisRaw(yString); }
            else { movement.y = axes[yAxis].GetValue(); }

            if (zAxis == -1) { movement.z = Input.GetAxisRaw(zString); }
            else { movement.z = axes[zAxis].GetValue(); }
            /////////////////////////////////
            if (pitchAxis == -1) { tempAngle.x = Input.GetAxisRaw(pitchString); }
            else { tempAngle.x = axes[pitchAxis].GetValue(); }
            if (invertY) { tempAngle.x = -tempAngle.x; }

            if (yawAxis == -1) { tempAngle.y = Input.GetAxisRaw(yawString); }
            else { tempAngle.y = axes[yawAxis].GetValue(); }

            if (useLookFilter) { Filter(ref tempAngle, useLookCurve); }
            Filter(ref movement, useMoveCurve);
            angle.x = tempAngle.x; angle.y = tempAngle.y;

            if (rollAxis == -1) { angle.z = Input.GetAxisRaw(rollString); }
            else { angle.z = axes[rollAxis].GetValue(); }

            switch (myType)
            {
                case ControllerType.keyboard:
                    angle.x = -angle.x;
                    angle.z = -angle.z;
                    isDownDown = Input.GetButtonDown(downString);
                    isUpDown = Input.GetButtonDown(upString);
                    isLeftDown = Input.GetButtonDown(leftString);
                    isRightDown = Input.GetButtonDown(rightString);
                    break;
                case ControllerType.xbox:
                    movement.y = -movement.y;
                    movement.z = -movement.z;
                    angle.z = -angle.z;
                    leftButton.Update();
                    rightButton.Update();
                    downButton.Update();
                    upButton.Update();
                    isLeftDown = leftButton.GetDown();
                    isRightDown = rightButton.GetDown();
                    isUpDown = upButton.GetDown();
                    isDownDown = downButton.GetDown();
                    break;
                case ControllerType.dualShock:
                    movement.y = -movement.y;
                    angle.z = -angle.z;
                    leftButton.Update();
                    rightButton.Update();
                    downButton.Update();
                    upButton.Update();
                    isLeftDown = leftButton.GetDown();
                    isRightDown = rightButton.GetDown();
                    isUpDown = upButton.GetDown();
                    isDownDown = downButton.GetDown();
                    break;
                default:
                    break;
            }

            isBoosting = Input.GetButton(boostString);
            isBoostingDown = Input.GetButtonDown(boostString);
            isBoostingUp = Input.GetButtonUp(boostString);

            isBraking = Input.GetButton(brakeString);
            isBrakingDown = Input.GetButtonDown(brakeString);
            isBrakingUp = Input.GetButtonUp(brakeString);

            isFire1 = Input.GetButton(fire1String);
            isFire1Down = Input.GetButtonDown(fire1String);
            isFire1Up = Input.GetButtonUp(fire1String);

            isFire2 = Input.GetButton(fire2String);
            isFire2Down = Input.GetButtonDown(fire2String);
            isFire2Up = Input.GetButtonUp(fire2String);

            isPause = Input.GetButton(pauseString);
            isPauseDown = Input.GetButtonDown(pauseString);
            isPauseUp = Input.GetButtonUp(pauseString);

            isInvertDown = Input.GetButtonDown(invertString);
            /*if (isInvertDown)
            {
                ToggleInvert();
            }*/

            isStealth = Input.GetButton(stealthString);
            isStealthDown = Input.GetButtonDown(stealthString);
            isStealthUp = Input.GetButtonUp(stealthString);

            isConfirm = Input.GetButton(confirmString);
            isConfirmDown = Input.GetButtonDown(confirmString);
            isConfirmUp = Input.GetButtonUp(confirmString);

            isCancel = Input.GetButton(cancelString);
            isCancelDown = Input.GetButtonDown(cancelString);
            isCancelUp = Input.GetButtonUp(cancelString);

            /*isLeft = Input.GetButton(leftString);
            isLeftDown = Input.GetButtonDown(leftString);
            isLeftUp = Input.GetButtonUp(leftString);

            isRight = Input.GetButton(rightString);
            isRightDown = Input.GetButtonDown(rightString);
            isRightUp = Input.GetButtonUp(rightString);*/
        }
    }

    public class ControllerManager
    {
        public List<Player> players = new List<Player>();
        public Controller uiController;
        const int maxPlayers = 4;
        public int playersEnabled = 0;
        public int playersConnected = 0;
        public int playersReady = 0;
        public bool canWrite = true;
        public bool canWithdraw = true;
        public bool canAdd = true;
        int[] connectedPlayers = new int[maxPlayers];
        string[] joystickNames;
        public bool dirty = true;
        GameObject gameObject;
        public bool isPaused = false;

        public Color[] factionColors = { Color.red, Color.blue, Color.yellow, Color.green, Color.black };

        public ControllerManager(GameObject _gameObject)
        {
            gameObject = _gameObject;
            players.Add(new Player(new Controller(ControllerType.keyboard, 0)));
            players[0].controller.useLookFilter = false;
            players[0].autoAim = false;
            for (int i = 0; i < 4; i++)
            {
                players.Add(new Player(new Controller(ControllerType.xbox, i + 1)));
            }
            uiController = new Controller(ControllerType.xbox, -1);
        }

        public void Update()
        {
            playersConnected = 0;
            joystickNames = Input.GetJoystickNames();
            playersEnabled = 0;
            playersReady = 0;
            
            uiController.isConfirmDown = false;
            uiController.isCancelDown = false;
            uiController.isUpDown = false;
            uiController.isDownDown = false;
            uiController.isLeftDown = false;
            uiController.isRightDown = false;
            uiController.isPauseDown = false;
            //isPaused = false;
            
            for (int i = 0; i < players.Count; i++)
            {
                players[i].controller.isConnected = true;
                if (i != 0)
                {
                    if (i - 1 > joystickNames.Length-1) { players[i].controller.isConnected = false; }
                    else if (joystickNames[i - 1] == "") { players[i].controller.isConnected = false; }
                    else
                    {
                        playersConnected++;
                        if (joystickNames[i - 1] == "Wireless Controller" && players[i].controller.myType!=ControllerType.dualShock) { players[i].controller.SetController(ControllerType.dualShock, players[i].controller.index); }
                    }
                }
                players[i].controller.Update();
                //update ui controller
                if (players[i].controller.isConfirmDown) { uiController.isConfirmDown = true; }
                if (players[i].controller.isCancelDown) { uiController.isCancelDown = true; }
                if (players[i].controller.isUpDown) { uiController.isUpDown = true; }
                if (players[i].controller.isDownDown) { uiController.isDownDown = true; }
                if (players[i].controller.isLeftDown) { uiController.isLeftDown = true; }
                if (players[i].controller.isRightDown) { uiController.isRightDown = true; }
                if (players[i].controller.isPauseDown) { uiController.isPauseDown = true; /*isPaused = true;*/ }
                if (canWrite)
                {
                    if (players[i].controller.isInvertDown)
                    {
                        players[i].controller.ToggleInvert();
                        dirty = true;
                    }
                    int enabledState = players[i].controller.enabledState;
                    switch (enabledState)
                    {
                        case 0:
                            if (players[i].controller.isConfirmDown && canAdd) {enabledState = 1;}
                            break;
                        case 1:
                            if(players[i].controller.isCancelDown && canWithdraw) { enabledState = 0; }
                            break;
                        default:
                            break;
                    }
                    if (enabledState != players[i].controller.enabledState) { dirty = true; }
                    players[i].controller.enabledState = enabledState;
                    players[i].controller.isEnabled = false;
                    players[i].controller.isReady = false;
                    if (enabledState > 0) { playersEnabled++; players[i].controller.isEnabled = true; }
                    if (enabledState == 2) { playersReady++; players[i].controller.isReady = true; }
                    if (playersEnabled == 4) { canAdd = false; }
                    else { canAdd = true; }
                }
                
            }
            if (uiController.isPauseDown) { isPaused = !isPaused; }
            if (isPaused)
            {
                if (uiController.isCancelDown)
                {
                    isPaused = false;
                }
            }
            if (isPaused && Time.timeScale == 1)
            {
                Time.timeScale = 0;
                gameObject.SendMessage("SetPauseMenu", true);
                //Debug.Log("got here");
            }
            else if (!isPaused && Time.timeScale == 0)
            {
                Time.timeScale = 1;
                gameObject.SendMessage("SetPauseMenu", false);
            }
        }

        public int GetPlayers(ref List<Player> _players)
        {
            _players = players;
            return playersEnabled;
        }
    }
}