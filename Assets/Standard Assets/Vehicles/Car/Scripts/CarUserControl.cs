using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{

    [RequireComponent(typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {
        public float vACC = 0f;
        public float hACC = 0f;
        public bool ACCONOFF = false;
        private float v = 0f;
        private float h = 0f;
        private float handbrake = 0f;
        private CarController m_Car; // the car controller we want to use

        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }


        private void FixedUpdate()
        {
            if (ACCONOFF) {
                v = vACC;     
            } else {
                v = CrossPlatformInputManager.GetAxis("Vertical");
            }

            if (Input.GetKey(KeyCode.Space)) {
                handbrake = 1f;
            } else {
                handbrake = 0f;
            }

            h = CrossPlatformInputManager.GetAxis("Horizontal");

            m_Car.Move(h, v, v, handbrake);
        }
    }    
}
