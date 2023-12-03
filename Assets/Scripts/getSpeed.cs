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
        float speed = GetComponent<Collider>().transform.parent.transform.parent.GetComponent<Rigidbody>().velocity.magnitude;
        speed = Mathf.Round(speed * 1f) / 1f;
        speedText.text = "Speed: " + speed + "m/s";
    }
    
}
    