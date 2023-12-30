namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    public class ToggleCarAIProcessing : MonoBehaviour
    {
        public AITrafficCar car;
        public AITrafficWaypoint targetWaypoint;

        [ContextMenu("DisableAIProcessing")]
        public void DisableAIProcessing()
        {
            car.DisableAIProcessing();
        }

        [ContextMenu("EnableAIProcessing")]
        public void EnableAIProcessing()
        {
            car.EnableAIProcessing();
            car.ChangeToRouteWaypoint(targetWaypoint.onReachWaypointSettings);
        }
    }
}