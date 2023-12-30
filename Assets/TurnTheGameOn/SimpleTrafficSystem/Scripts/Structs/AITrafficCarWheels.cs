namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    [System.Serializable]
    public struct AITrafficCarWheels
    {
        [Tooltip("Wheel name (used for inspector organization only)")]
        public string name;
        [Tooltip("Wheel mesh Transform reference.")]
        public Transform meshTransform;
        [Tooltip("WheelCollider reference.")]
        public WheelCollider collider;
    }
}