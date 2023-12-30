namespace TurnTheGameOn.SimpleTrafficSystem
{
    /// Attach this component to an AITrafficCar
    /// defines a custom rigidbody center of mass position
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class CenterOfMass : MonoBehaviour
    {
        public Vector3 centerOfMass;

        void Awake()
        {
            GetComponent<Rigidbody>().centerOfMass = centerOfMass;
        }
    }
}