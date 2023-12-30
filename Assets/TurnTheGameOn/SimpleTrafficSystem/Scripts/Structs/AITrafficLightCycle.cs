namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    [System.Serializable]
    public struct AITrafficLightCycle
    {
        [Tooltip("Green light duration.")]
        public float greenTimer;
        [Tooltip("Yellow light duration.")]
        public float yellowTimer;
        [Tooltip("Red light duration.")]
        public float redtimer;
        [Tooltip("Array of AITrafficLights that follow this sequence.")]
        public AITrafficLight[] trafficLights;
    }
}