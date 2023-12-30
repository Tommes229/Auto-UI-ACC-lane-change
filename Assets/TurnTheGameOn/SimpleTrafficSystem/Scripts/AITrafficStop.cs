namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using System.Collections.Generic;

    [HelpURL("https://simpletrafficsystem.turnthegameon.com/documentation/api/aitrafficstop")]
    public class AITrafficStop : MonoBehaviour
    {
        [Tooltip("Cars can't exit assigned route until AITrafficStopManager allows it.")]
        public AITrafficWaypointRoute waypointRoute;
        public List<AITrafficWaypointRoute> waypointRoutes;
        public bool stopForTraffic { get; private set; }

        public void StopTraffic()
        {
            if (waypointRoute != null) waypointRoute.StopForTrafficlight(true);
            for (int i = 0; i < waypointRoutes.Count; i++)
            {
                waypointRoutes[i].StopForTrafficlight(true);
            }
            stopForTraffic = true;
        }

        public void AllowCarToProceed()
        {
            if (waypointRoute != null) waypointRoute.StopForTrafficlight(false);
            for (int i = 0; i < waypointRoutes.Count; i++)
            {
                waypointRoutes[i].StopForTrafficlight(false);
            }
            stopForTraffic = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = stopForTraffic ? Color.red : Color.green;
            Gizmos.DrawCube(transform.position, new Vector3(1, 2, 1));
        }

    }
}