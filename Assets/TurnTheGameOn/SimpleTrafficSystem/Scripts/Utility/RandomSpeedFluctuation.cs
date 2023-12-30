namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    [RequireComponent(typeof(AITrafficCar))]
    public class RandomSpeedFluctuation : MonoBehaviour
    {
        public float minSpeed = 15f;
        public float maxSpeed = 35f;
        public float updateFrequency = 5f;
        private AITrafficCar trafficCar;
        private float topSpeed;
        private float timer;

        private void Start()
        {
            trafficCar = GetComponent<AITrafficCar>();
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= updateFrequency)
            {
                topSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
                trafficCar.SetTopSpeed(topSpeed);
                timer = 0;
            }
        }
    }
}