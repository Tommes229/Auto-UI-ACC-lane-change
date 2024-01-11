using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using UnityStandardAssets.Vehicles.Car;

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
    private Vector3 lastDirection = Vector3.zero;
    //create car controller
    private CarController carC;
    private bool RecentInput = false;
    private CarUserControl carUserControl;
    private bool carStopped = false;




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

        carC = GetComponent<CarController>();
        carUserControl = carC.GetComponent<CarUserControl>();
    }




    void FixedUpdate() {

        //if the key s is pressed, set recentinput to true
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Space)) {
            RecentInput = true;
            Invoke("ResetRecentInput", 0.5f);
        }

        if (ACCONOFF && RecentInput == false) {
            carUserControl.ACCONOFF = true;
            
            //if the trigger is true, slow down the car
            if (Trigger) {
                //remove all cars that are not in the trigger anymore
                foreach (GameObject car in carsInTriggerToRemove) {
                    carsInTrigger.Remove(car);
                    carsInTriggerToRemove.Remove(car);  
                }

                //for all cars in the trigger, get the closest one  
                float closestDistance = Mathf.Infinity;
                GameObject closestCar = null;
                float closestCarVelocity = Mathf.Infinity;

                foreach (GameObject car in carsInTrigger) {
                    float distance = Vector3.Distance(transform.position, car.transform.position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestCar = car;
                    }
                }

                //get the velocity of the closest car
                closestCarVelocity = closestCar.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;

                switch(closestDistance) {
                    case float n when (n < 8f):
                        if (GetComponent<Rigidbody>().velocity.magnitude > 2f) {
                            carUserControl.vACC = -1f;
                        } else {
                            carUserControl.vACC = 0f;
                            GetComponent<Rigidbody>().velocity = Vector3.zero;
                            carStopped = true;
                        }
                        print("case 1");
                        break;

                    case float n when (n < 11f):
                        if (GetComponent<Rigidbody>().velocity.magnitude < 2f) {
                            carUserControl.vACC = -1f;
                            GetComponent<Rigidbody>().velocity = Vector3.zero;
                            carStopped = true;
                        } else if (GetComponent<Rigidbody>().velocity.magnitude * 3.6f > closestCarVelocity) {
                            carUserControl.vACC = -0.8f;
                        } else {
                            carUserControl.vACC = 0f;
                        }
                        print("case 2");
                        break;

                    case float n when (n < 18f):
                        if (GetComponent<Rigidbody>().velocity.magnitude * 3.6f > closestCarVelocity && GetComponent<Rigidbody>().velocity.magnitude > 2f) {
                            if (GetComponent<Rigidbody>().velocity.magnitude * 3.6f - closestCarVelocity > 10f) {
                                carUserControl.vACC = -1f;
                            } else {
                                carUserControl.vACC = -0.5f;
                            }
                        } else if (GetComponent<Rigidbody>().velocity.magnitude * 3.6f < closestCarVelocity-2f) {         
                            carUserControl.vACC = 1f;
                        } else {
                            carUserControl.vACC = 0f;
                        }
                        print("case 3");
                        break;
                    default:
                        if (GetComponent<Rigidbody>().velocity.magnitude * 3.6f > closestCarVelocity) {
                            if (GetComponent<Rigidbody>().velocity.magnitude * 3.6f - closestCarVelocity > 30f) {
                                carUserControl.vACC = -1f;
                            } else {
                                carUserControl.vACC = -0.3f;
                            }
                        } else if (GetComponent<Rigidbody>().velocity.magnitude * 3.6f < closestCarVelocity-2f) {         
                            carUserControl.vACC = 1f;
                        } else {
                            carUserControl.vACC = 0f;
                        }
                        print("case 4");
                        break;
                }
            

            //if the trigger is false, increase the velocity of the object    
            } else if (GetComponent<Rigidbody>().velocity.magnitude * 3.6f < ACCInputSpeed) {   
                if (carStopped) {
                    carUserControl.hACC = -1f;
                    carStopped = false;
                } 
                carUserControl.vACC = 1f;        
            } else {
                carUserControl.vACC = 0f;
            }
        } else {
            carUserControl.ACCONOFF = false;
        }
    }


    // Update is called once per frame
    void Update () {

        //increase accinputspeed once per keypress of c
        if (Input.GetKeyDown(KeyCode.V) && ACCInputSpeed < 100f) {
            ACCInputSpeed += 5f;
        } else if (Input.GetKeyDown(KeyCode.C) && ACCInputSpeed > 0f) {
            ACCInputSpeed -= 5f;
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            ACCONOFF = !ACCONOFF;
            ACCONOFFText.text = "ACC: " + ACCONOFF;
        }
        ACCInputSpeedText.text = "ACC: " + ACCInputSpeed + "km/h";  
    }

    void ResetRecentInput() {
        RecentInput = false;
    }
}
