using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassSelectorBossScript : MonoBehaviour {

	List<GameInput.Controller> controllers;

    public void GetControllers(ref List<GameInput.Controller> _controllers)
    {
        controllers = _controllers;
    }

    public void SetControllers()
    {
        

    }
}
