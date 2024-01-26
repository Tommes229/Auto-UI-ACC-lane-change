using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using System.IO;
using UnityEngine.SceneManagement;

public class Test2Trigger : MonoBehaviour
{
    private GetSpeed speed;
    // Start is called before the first frame update
    void Start()
    {
    }
    
    void OnTriggerEnter(Collider other) {
         if (other.gameObject.CompareTag("Collider") && PlayerPrefs.GetInt("ACC") == 0) { 
            speed = other.gameObject.GetComponent<GetSpeed>();

            string filePath = Application.dataPath + "/Test2.csv";
            StreamWriter writer = new StreamWriter(filePath);

           writer.WriteLine("KeyTime pressed");
            writer.WriteLine("w: " + speed.getWCounted().ToString());
            writer.WriteLine("s: " + speed.getSCounted().ToString());
            writer.WriteLine("a: " + speed.getACounted().ToString());
            writer.WriteLine("d: " + speed.getDCounted().ToString());

            speed.resetCounted();
            writer.Close();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1); 

        } else if (other.gameObject.CompareTag("Collider") && PlayerPrefs.GetInt("ACC") == 1) {
            speed = other.gameObject.GetComponent<GetSpeed>();

            string filePath = Application.dataPath + "/Test2ACC.csv";
            StreamWriter writer = new StreamWriter(filePath);

            writer.WriteLine("KeyTime pressed");
            writer.WriteLine("w: " + speed.getWCounted().ToString());
            writer.WriteLine("s: " + speed.getSCounted().ToString());
            writer.WriteLine("a: " + speed.getACounted().ToString());
            writer.WriteLine("d: " + speed.getDCounted().ToString());

            speed.resetCounted();
            writer.Close();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1); 
        } 

    }
}
