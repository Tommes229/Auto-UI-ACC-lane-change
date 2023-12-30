namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    [HelpURL("https://simpletrafficsystem.turnthegameon.com/documentation/api/aitrafficwaypointrouteinfo")]
    public class AITrafficWaypointRouteInfo : MonoBehaviour
    {
        [Tooltip("Controls if cars can exit route, set by traffic lights.")]
        public bool stopForTrafficLight;
        [Tooltip("Controls if this route requires cross traffic to yield, set by AITrafficController.")]
        public bool yieldForTrafficLight;
        [Tooltip("Box Collider used to set bounds for yield area.")]
        public BoxCollider yieldTrigger;
        private Collider[] hitColliders;

        private void Update()
        {
            if (yieldTrigger != null)
            {
                hitColliders = Physics.OverlapBox(yieldTrigger.transform.position, yieldTrigger.size / 2, Quaternion.identity, AITrafficController.Instance.layerMask);
                yieldForTrafficLight = hitColliders.Length > 0 ? true : false;
            }
        }

        private void OnDisable()
        {
            yieldForTrafficLight = false;
        }
    }
}