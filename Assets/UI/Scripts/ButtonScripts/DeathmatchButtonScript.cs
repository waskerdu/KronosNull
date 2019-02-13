using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathmatchButtonScript : MonoBehaviour {
    void Confirm()
    {
        SceneManager.LoadScene("6dofArena");
    }
}
