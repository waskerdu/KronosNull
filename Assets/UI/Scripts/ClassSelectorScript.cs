using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectorScript : MonoBehaviour {

    public string[,] info;
    public string[] invert;
    public Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow, Color.black };
    public Color orange = new Color(0, 132, 0);
    public GameObject baseObject;
    public GameObject pointer;
    public int pointerIndex = 1;
    public int colorIndex = 0;
    public int numElements = 5;
    public int index = 0;
    public int numAlliedBots = 0;
    public int maxBots = 4;
    public int sensitivityIndex = 3;
    public int maxSensitivity = 6;
    int size = 2;
    int fields = 2;
    GameInput.Controller controller;
    public GameObject invertText;
    float selectionDelay = 0.5f;
    float selectionClock = 0;
    public Rect oldRect = new Rect();
    public Rect rectDesired = new Rect(0, 0, 0, 0);
    public Rect currentRect = new Rect();
    float rectDelay = 1;
    public float rectClock = 0;
    public bool done = false;
    public int playerIndex = -2;
    int invertInd = 0;
    public bool launched = false;
    public bool canWrite = false;

    void Start()
    {
        info = new string[size, fields];
        info[0, 0] = "SABER";
        info[0, 1] = "An all around balanced design. \nPrimary weapon is a railgun with a low rate of fire and high damage. \nSecondary weapon is an emp equipped homing mine";
        info[1, 0] = "CENTURION";
        info[1, 1] = "A front line fighter, very effective in formations. \nPrimary weapon is a four shot burst rifle. \nSecondary weapon is a gravity bomb that does direct hull damage.";
        invert = new string[2];
        invert[0] = "\nInvert: off";
        invert[1] = "\nInvert: on";
        SetInfo();
        controller = new GameInput.Controller(GameInput.ControllerType.xbox, -2);
        invertText.GetComponent<TMPro.TextMeshProUGUI>().text = invert[invertInd];
        ChangeSensitivity(0);
    }

    void ResetPointer()
    {
        pointerIndex = 1;
        MovePointer(0);
    }

    void Update()
    {
        if (rectClock > 0)
        {
            currentRect.x = Mathf.Lerp(rectDesired.x, oldRect.x, rectClock);
            currentRect.y = Mathf.Lerp(rectDesired.y, oldRect.y, rectClock);
            currentRect.width = Mathf.Lerp(rectDesired.width, oldRect.width, rectClock);
            currentRect.height = Mathf.Lerp(rectDesired.height, oldRect.height, rectClock);
            GetComponent<Camera>().rect = currentRect;
            rectClock -= Time.deltaTime;
        }
        else if(!done)
        {
            currentRect = rectDesired;
            GetComponent<Camera>().rect = currentRect;
            oldRect = currentRect;
            rectClock = 0;
            done = true;
        }
        if (!canWrite) { return; }
        if (controller.isUpDown) { MovePointer(-1); }
        if (controller.isDownDown) { MovePointer(1); }
        if (controller.isConfirmDown && pointerIndex == numElements - 1) { Launch();return; }
        switch (pointerIndex)
        {
            case 0:
                if (controller.isLeftDown || controller.isRightDown)
                {
                    controller.ToggleInvert();
                }
                break;
            case 1:
                if (controller.isLeftDown) { ChangeColor(-1); }
                if (controller.isRightDown) { ChangeColor(1); }
                break;
            case 2:
                if (controller.isLeftDown) { ChangeNumBots(-1); }
                if (controller.isRightDown) { ChangeNumBots(1); }
                break;
            case 3:
                if (controller.isLeftDown) { ChangeSensitivity(-1); }
                if (controller.isRightDown) { ChangeSensitivity(1); }
                break;
            case 4:
                if (controller.isLeftDown || controller.isRightDown)
                {
                    Launch();
                }
                    break;
            default:
                break;
        }
        if (controller.invertY && invertInd == 0)  { invertInd = 1;invertText.GetComponent<TMPro.TextMeshProUGUI>().text = invert[invertInd]; }
        else if (!controller.invertY && invertInd == 1) { invertInd = 0; invertText.GetComponent<TMPro.TextMeshProUGUI>().text = invert[invertInd]; }
    }

    void SetIndex(int _index) { index = _index; }

    public void Launch()
    {
        pointerIndex = numElements - 1;
        MovePointer(0);
        pointer.transform.parent.gameObject.GetComponent<Image>().color = Color.gray;
        pointer.transform.parent.GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Waiting";
        pointer.SetActive(false);
        launched = true;
        SendMessageUpwards("MakeDirty", index);
    }

    public void ChangeSensitivity(int move)
    {
        sensitivityIndex += move;
        sensitivityIndex = Mathf.Clamp(sensitivityIndex, 0, maxSensitivity);
        Color newColor;
        for (int i = 0; i < pointer.transform.parent.GetChild(1).childCount; i++)
        {
            newColor = Color.yellow;
            if (i == sensitivityIndex) { newColor = Color.red; }
            pointer.transform.parent.GetChild(1).GetChild(i).gameObject.GetComponent<Image>().color = newColor;
        }
    }

    public void ChangeNumBots(int move)
    {
        numAlliedBots += move;
        numAlliedBots = Mathf.Clamp(numAlliedBots, 0, maxBots);
        pointer.transform.parent.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Allied Bots: " + numAlliedBots;
    }

    public void ChangeColor(int move)
    {
        colorIndex += move;
        if (colorIndex < 0) { colorIndex = colors.Length - 1; }
        if (colorIndex > colors.Length - 1) { colorIndex = 0; }
        pointer.transform.parent.gameObject.GetComponent<Image>().color = colors[colorIndex];
        if (colorIndex == colors.Length - 1)
        {
            //hide team text
            transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).gameObject.SetActive(false);
            transform.GetChild(0).GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(false);
        }
    }
    public void MovePointer(int move)
    {
        pointerIndex += move;
        if (pointerIndex < 0) { pointerIndex = numElements - 1; }
        if (pointerIndex > numElements - 1) { pointerIndex = 0; }
        pointer.transform.SetParent(baseObject.transform.GetChild(pointerIndex));
        //pointer.transform.localPosition = Vector3.zero;
        //pointer.GetComponent<RectTransform>().localPosition = Vector3.zero;
        pointer.transform.position = pointer.transform.parent.position;
    }

    public void SetController(GameInput.Controller _controller)
    {
        controller = _controller;
        playerIndex = controller.index;
    }

    public int GetClassIndex()
    {
        return index;
    }

    public void SetRect(Rect _rectDesired)
    {
        rectClock = rectDelay;
        rectDesired = _rectDesired;
        done = false;
    }

    public void SetOldRect(Rect _oldRect)
    {
        if (oldRect == Rect.zero) { oldRect = _oldRect; }
    }

    public void SetRectImmediate(Rect _rect)
    {
        GetComponent<Camera>().rect = _rect; oldRect = _rect; rectDesired = _rect; done = true;
    }
    

    void NextElement()
    {
        index++;
        if (index == size) { index = 0; }
    }

    void LastElement()
    {
        index--;
        if (index == -1) { index = size - 1; }
    }

    void SetInfo()
    {
        //title.GetComponent<TMPro.TextMeshProUGUI>().text = info[index,0];
        //description.GetComponent<TMPro.TextMeshProUGUI>().text = info[index, 1];
    }

    //void ClassSelectorGetDescriptions(string[] newDescriptions) { descriptions = newDescriptions; }

}
