using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldRechargeScript : MonoBehaviour {
    public bool isShield = true;

	void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Shield")
        {
            if (isShield)
            {
                collider.gameObject.SendMessage("Restore", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                collider.transform.parent.gameObject.BroadcastMessage("ResetAmmo");
            }
            Destroy(gameObject);
            //Debug.Log("got here");
        }
        
    }
}
