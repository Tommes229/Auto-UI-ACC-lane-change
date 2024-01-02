using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccTrigger : MonoBehaviour
{
    private int inTriggerCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    //if something is in the trigger, set the trigger bool to true
    void OnTriggerEnter(Collider other) {

        //if other has the tag "AICar"
        if (other.gameObject.CompareTag("AICar")) { 
            inTriggerCounter++;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponent<AccSpeedControl>().Trigger = true;
            player.GetComponent<AccSpeedControl>().carsInTrigger.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other) {
        //if other has the tag "AICar"
        if (other.gameObject.CompareTag("AICar")) { 
            inTriggerCounter--;
            if (inTriggerCounter == 0) {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                player.GetComponent<AccSpeedControl>().Trigger = false;
                player.GetComponent<AccSpeedControl>().carsInTrigger.Remove(other.gameObject);
            }
        }
    }
}
