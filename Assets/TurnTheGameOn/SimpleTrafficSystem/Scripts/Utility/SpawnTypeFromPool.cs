namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    public class SpawnTypeFromPool : MonoBehaviour
    {
        public AITrafficVehicleType type;
        public AITrafficSpawnPoint spawnPoint;
        public bool spawnCars = true;
        public float spawnRate = 10f;
        private float timer;
        private AITrafficWaypointRoute route;
        private AITrafficCar spawnCar;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private Vector3 spawnOffset = new Vector3(0, -4, 0);
        private Transform nextPoint;

        private void Start()
        {
            timer = 0f;
            route = GetComponent<AITrafficWaypointRoute>();
        }

        private void Update()
        {
            if (spawnCars)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    timer = spawnRate;
                    if (spawnPoint.isTrigger == false)
                    {
                        spawnCar = AITrafficController.Instance.GetCarFromPool(route, type);
                        if (spawnCar != null)
                        {
                            spawnPosition = spawnPoint.transform.position + spawnOffset;
                            spawnRotation = spawnPoint.transform.rotation;
                            spawnCar.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
                            nextPoint = spawnPoint.waypoint.onReachWaypointSettings.nextPointInRoute.transform;
                            spawnCar.transform.LookAt(nextPoint);
                        }
                    }
                }
            }
        }

    }
}