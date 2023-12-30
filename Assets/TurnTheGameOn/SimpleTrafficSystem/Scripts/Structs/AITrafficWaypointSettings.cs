namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using System.Collections.Generic;
    using UnityEngine.Events;

    [System.Serializable]
    public struct AITrafficWaypointSettings
    {
        [Tooltip("Reference to the waypoint's parent route.")]
        public AITrafficWaypointRoute parentRoute;
        [Tooltip("Reference to the waypoint.")]
        public AITrafficWaypoint waypoint;
        [HideInInspector] public AITrafficWaypoint nextPointInRoute;
        [Tooltip("Waypoint Route array index of the waypoint.")]
        public int waypointIndexnumber;
        [Tooltip("Speed limit the car will use after reaching the waypoint.")]
        public float speedLimit;
        [Tooltip("Controls if the car will stop driving after reaching a waypoint.")]
        public bool stopDriving;
        [Tooltip("Set stop time greater than 0 to restart the car after the stop time duration.")]
        public float stopTime;
        [Tooltip("Array of route points the car can choose randomly after reaching a waypoint.")]
        public AITrafficWaypoint[] newRoutePoints;
        [Tooltip("Array of route points the car can choose randomly based on sensor conditions to change to for obstacle avoidance.")]
        public List<AITrafficWaypoint> laneChangePoints;
        [Tooltip("Array of YieldTriggers the car will check for, if the yield trigger's assigned route has a green light and is being triggered the car will stop until the trigger is empty or the light is red.")]
        public List<AITrafficWaypointRouteInfo> yieldTriggers;
        [Tooltip("Custom event that is triggered when the car reaches a waypoint.")]
        public UnityEvent OnReachWaypointEvent;
        [HideInInspector] public Vector3 position;
    }
}