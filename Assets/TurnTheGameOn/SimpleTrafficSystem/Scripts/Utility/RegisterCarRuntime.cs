namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    public class RegisterCarRuntime : MonoBehaviour
    {
        public AITrafficCar aITrafficCarPrefab;
        public AITrafficWaypointRoute aITrafficWaypointRoute;
        public AITrafficSpawnPoint aITrafficSpawnPoint;
        private AITrafficCar spawnedAITrafficCar;

        [ContextMenu("SpawnAndRegisterCar")]
        public void SpawnAndRegisterCar()
        {
            if (aITrafficSpawnPoint.isTrigger == false)
            {
                spawnedAITrafficCar = Instantiate(aITrafficCarPrefab.gameObject, aITrafficSpawnPoint.transform.position, aITrafficSpawnPoint.transform.rotation).GetComponent<AITrafficCar>();
                spawnedAITrafficCar.RegisterCar(aITrafficWaypointRoute);
            }
        }
    }
}