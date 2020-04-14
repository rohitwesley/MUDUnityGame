using GameLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        // If Agent Collided
        if(other.gameObject.GetComponent<Agents>())
        {
            Debug.Log("PickUp Triggered by " + other.gameObject.GetComponent<Agents>().GetAgentName());
            //Get Pick Up if player and add points to player score
            if (other.gameObject.GetComponent<Agents>().unitType == UnitType.Player)
            {
                Debug.Log("Player PickUp");
                gameObject.SetActive(false);
                Destroy(gameObject);
                other.gameObject.GetComponent<Agents>().scoreValue += gameObject.GetComponent<Agents>().pointsValue;
            }
        }
    }

}
