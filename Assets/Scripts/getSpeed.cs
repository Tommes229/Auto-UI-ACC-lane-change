using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GetSpeed : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    private int wCounted = 0;
    private int sCounted = 0;
    private int aCounted = 0;
    private int dCounted = 0;


    // Update is called once per frame
    void Update()
    {
        float speed = GetComponent<Collider>().transform.parent.transform.parent.GetComponent<Rigidbody>().velocity.magnitude*3.6f;
        speed = Mathf.Round(speed);
        speedText.text = "Speed: " + speed + "km/h";
    }

    void FixedUpdate() {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            wCounted++;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            sCounted++; 
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            aCounted++; 
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            dCounted++;
        }

        if (Input.GetKey(KeyCode.Escape)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
    }

    public int getWCounted() {
        return wCounted;
    }

    public int getSCounted() {
        return sCounted;
    }

    public int getACounted() {
        return aCounted;
    }

    public int getDCounted() {
        return dCounted;
    }

    public void resetCounted() {
       
        wCounted = 0;
        sCounted = 0;
        aCounted = 0;
        dCounted = 0;
    }
    
}
    