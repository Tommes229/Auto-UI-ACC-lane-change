namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    public class PlayerTriggerLaneChange : MonoBehaviour
    {
        public AITrafficCar trafficCar;
        public string triggerTag = "Player";
        private bool isTriggered;

        private void OnTriggerStay(Collider other)
        {
            if (isTriggered == false)
            {
                if (other.tag == triggerTag)
                {
                    trafficCar.SetForceLaneChange(true);
                    isTriggered = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isTriggered)
            {
                if (other.tag == triggerTag)
                {
                    trafficCar.SetForceLaneChange(false);
                    isTriggered = false;
                }
            }
        }
    }
}