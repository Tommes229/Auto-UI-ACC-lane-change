namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using System.Collections.Generic;

    [HelpURL("https://simpletrafficsystem.turnthegameon.com/documentation/api/aitrafficlight")]
    public class AITrafficLight : MonoBehaviour
    {
        [Tooltip("Red light mesh, disabled for green and yellow.")]
        public MeshRenderer redMesh;
        [Tooltip("Yellow light mesh, disabled for green and red.")]
        public MeshRenderer yellowMesh;
        [Tooltip("Green light mesh, disabled for red and yellow.")]
        public MeshRenderer greenMesh;
        [Tooltip("Cars can't exit assigned route if light is red or yellow.")]
        public AITrafficWaypointRoute waypointRoute;
        [Tooltip("Array for multiple routes, cars can't exit assigned route if light is red or yellow.")]
        public List<AITrafficWaypointRoute> waypointRoutes;

        public void EnableRedLight()
        {
            if (waypointRoute) waypointRoute.StopForTrafficlight(true);
            for (int i = 0; i < waypointRoutes.Count; i++)
            {
                waypointRoutes[i].StopForTrafficlight(true);
            }
            redMesh.enabled = true;
            yellowMesh.enabled = false;
            greenMesh.enabled = false;
        }

        public void EnableYellowLight()
        {
            if (waypointRoute) waypointRoute.StopForTrafficlight(true);
            for (int i = 0; i < waypointRoutes.Count; i++)
            {
                waypointRoutes[i].StopForTrafficlight(true);
            }
            redMesh.enabled = false;
            yellowMesh.enabled = true;
            greenMesh.enabled = false;
        }

        public void EnableGreenLight()
        {
            if (waypointRoute) waypointRoute.StopForTrafficlight(false);
            for (int i = 0; i < waypointRoutes.Count; i++)
            {
                waypointRoutes[i].StopForTrafficlight(false);
            }
            redMesh.enabled = false;
            yellowMesh.enabled = false;
            greenMesh.enabled = true;
        }

    }
}