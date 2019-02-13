using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetUIScript : MonoBehaviour {
    List<GameObject> allies;
    List<GameObject> enemies;
    public void SetAllies(ref List<GameObject> _allies) { allies = _allies; }
    public void SetEnemies(ref List<GameObject> _enemies) { enemies = _enemies; }
    public GameObject targetHealthBar;
    List<GameObject> healthbars = new List<GameObject>();
    List<int> healthbarRef = new List<int>();
    public GameObject playerCam;
    Camera cam;
    GameObject tempObj;
    public GameObject victory;
    public int numEnemies;

    void Start ()
    {
        cam = playerCam.GetComponent<Camera>();
        numEnemies = enemies.Count;
	}
	
	void Update ()
    {
        while (healthbars.Count < allies.Count - 1 + enemies.Count)
        {
            tempObj = Instantiate(targetHealthBar, transform);
            tempObj.SetActive(true);
            healthbars.Add(tempObj);
        }
        int iter = 0;
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].GetInstanceID() != transform.root.gameObject.GetInstanceID())
            {
                Vector3 newPos = cam.WorldToScreenPoint(allies[i].transform.position);
                float xOffset = Screen.width / 2 * cam.rect.width;
                float yOffset = Screen.height / 2 * cam.rect.height;
                newPos.x -= xOffset + xOffset * 4 * cam.rect.x;
                newPos.y -= yOffset + yOffset * 4 * cam.rect.y - 30;
                healthbars[iter].transform.localPosition = newPos;
                healthbars[iter].transform.GetChild(2).GetComponent<Image>().color = allies[i].GetComponent<movement>().color;
                healthbars[iter].transform.GetChild(3).GetComponent<Image>().color = allies[i].GetComponent<movement>().color;
                float health = allies[i].GetComponent<movement>().health / allies[i].GetComponent<movement>().maxHealth;
                healthbars[iter].transform.GetChild(1).GetComponent<HealthBar>().SetHealthDisplay(health);
                if (health < 0) { healthbars[iter].SetActive(false); }
                iter++;
            }
        }
        movement script;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].activeInHierarchy)
            {
                Vector3 newPos = cam.WorldToScreenPoint(enemies[i].transform.position);
                float xOffset = Screen.width / 2 * cam.rect.width;
                float yOffset = Screen.height / 2 * cam.rect.height;
                newPos.x -= xOffset + xOffset * 4 * cam.rect.x;
                newPos.y -= yOffset + yOffset * 4 * cam.rect.y - 30;
                healthbars[iter].transform.localPosition = newPos;
                script = enemies[i].GetComponent<movement>();
                healthbars[iter].transform.GetChild(2).GetComponent<Image>().color = script.color;
                healthbars[iter].transform.GetChild(3).GetComponent<Image>().color = script.color;
                float health = script.health / script.maxHealth;
                healthbars[iter].transform.GetChild(1).GetComponent<HealthBar>().SetHealthDisplay(health);
                if (script.health < 0) { healthbars[iter].SetActive(false); numEnemies--; }
            }
            iter++;
        }

        if (numEnemies == 0)
        {
            victory.SetActive(true);
        }
    }
}
