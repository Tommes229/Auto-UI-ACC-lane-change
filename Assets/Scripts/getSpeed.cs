using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class getSpeed : MonoBehaviour
{
    public TextMeshProUGUI speedText;

    // Update is called once per frame
    void Update()
    {
        float speed = GetComponent<Collider>().transform.parent.transform.parent.GetComponent<Rigidbody>().velocity.magnitude*3.6f;
        speed = Mathf.Round(speed);
        speedText.text = "Speed: " + speed + "km/h";
    }
    
}
    