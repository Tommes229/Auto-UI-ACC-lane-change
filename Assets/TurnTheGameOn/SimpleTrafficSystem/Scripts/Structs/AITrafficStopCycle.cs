namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    [System.Serializable]
    public struct AITrafficStopCycle
    {
        [Tooltip("Amount of time cars are allowed to proceed before next cycle.")]
        public float activeTime;
        [Tooltip("Amount of time before next cycle becomes active.")]
        public float waitTime;
        [Tooltip("Array of AITrafficLights that follow this sequence.")]
        public AITrafficStop[] trafficStops;
    }
}