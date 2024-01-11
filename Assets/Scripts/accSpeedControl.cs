using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AccSpeedControl : MonoBehaviour
{
    private float ACCInputSpeed = 50f;
    public TextMeshProUGUI ACCInputSpeedText;

    private bool ACCONOFF = false;
    public TextMeshProUGUI ACCONOFFText;
    public bool Trigger = false;


    //array list of all the cars in the trigger
    public List<GameObject> carsInTrigger = new List<GameObject>();
    public List<GameObject> carsInTriggerToRemove = new List<GameObject>();
    private float slowDownFactor = 0f;



    // Start is called before the first frame update
    void Start()
    {

        switch(PlayerPrefs.GetInt("GameValue")) {
            case 0:
                transform.position = new Vector3(0f, 0f, -117f);
                break;
            case 1:
                transform.position = new Vector3(-63f, 0f, -3f);
                break;
            case 2:
                transform.position = new Vector3(-69f, 0f, 104f);
                break;
            default:
                Debug.Log("Error: GameValue not set");
                break;
        }
    }




    void FixedUpdate() {

        if (ACCONOFF) {
            //if the trigger is true and faster than 1 km/h, decrease the velocity of the object
            if (Trigger) {

                //remove all cars that are not in the trigger anymore
                foreach (GameObject car in carsInTriggerToRemove) {
                    carsInTrigger.Remove(car);
                    carsInTriggerToRemove.Remove(car);  
                }

                //for all cars in the trigger, get the closest one  
                float closestDistance = Mathf.Infinity;
                foreach (GameObject car in carsInTrigger) {
                    float distance = Vector3.Distance(transform.position, car.transform.position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                    }
                }

                switch(closestDistance) {
                    case float n when (n < 8f):
                        slowDownFactor = 1f;
                        break;
                    case float n when (n < 9f):
                        slowDownFactor = 0.9f;
                        break;
                    case float n when (n < 11f):
                        slowDownFactor = 0.6f;
                        break;
                    case float n when (n < 13f):
                        slowDownFactor = 0.4f;
                        break;
                    case float n when (n < 15f):
                        slowDownFactor = 0.2f;
                        break;
                    default:
                        slowDownFactor = 0.1f;
                        break;
                }

                //check if the car gets negative velocity
                if (closestDistance < 6f && GetComponent<Rigidbody>().velocity.magnitude <= 1f) {
                    GetComponent<Rigidbody>().velocity = Vector3.zero;
                
                } else if (slowDownFactor == 1f) {
                    GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity * 0.4f;
                } else {
                    GetComponent<Rigidbody>().velocity -= GetComponent<Rigidbody>().velocity.normalized * slowDownFactor;
                }
            

            //if the trigger is false, increase the velocity of the object    
            } else if (GetComponent<Rigidbody>().velocity.magnitude * 3.6f < ACCInputSpeed && GetComponent<Rigidbody>().velocity.magnitude > 1f) {
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

        if (Input.GetKeyDown(KeyCode.B)) {
            ACCONOFF = !ACCONOFF;
            ACCONOFFText.text = "ACC: " + ACCONOFF;
        }
        ACCInputSpeedText.text = "ACC: " + ACCInputSpeed + "km/h";  
    }
}
