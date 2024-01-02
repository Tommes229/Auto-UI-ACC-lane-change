using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class accSpeedControl : MonoBehaviour
{
    private float ACCInputSpeed = 50f;
    public TextMeshProUGUI ACCInputSpeedText;
    public bool Trigger = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate() {

        //if the trigger is true, decrease the velocity of the object
        if (Trigger) {
            Debug.Log("Trigger is true");
            GetComponent<Rigidbody>().velocity -= GetComponent<Rigidbody>().velocity.normalized * 0.2f;
        } else {
            //if the trigger is false, increase the velocity of the object
            if (GetComponent<Rigidbody>().velocity.magnitude * 3.6f < ACCInputSpeed) {
                GetComponent<Rigidbody>().velocity += GetComponent<Rigidbody>().velocity.normalized * 0.1f;
            }
        }
        

    }

    // Update is called once per frame
    void Update () {
        //increase accinputspeed once per keypress of c
        if (Input.GetKeyDown(KeyCode.V)) {
            ACCInputSpeed += 5f;
        } else if (Input.GetKeyDown(KeyCode.C)) {
            ACCInputSpeed -= 5f;
        }

        ACCInputSpeedText.text = "ACC: " + ACCInputSpeed + "km/h";  
    }
}
