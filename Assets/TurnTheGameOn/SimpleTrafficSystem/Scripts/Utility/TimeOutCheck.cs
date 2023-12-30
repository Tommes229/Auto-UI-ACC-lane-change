namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    [RequireComponent(typeof(AITrafficCar))]
    public class TimeOutCheck : MonoBehaviour
    {
        public float disableTime = 90;
        private AITrafficCar car;
        private float timer;

        private void Awake() { car = GetComponent<AITrafficCar>(); }

        private void Update()
        {
            if (Mathf.Approximately(car.CurrentSpeed(), 0))
            {
                timer += Time.deltaTime;
                if (timer >= disableTime)
                {
                    if (AITrafficController.Instance.usePooling)
                    {
                        car.MoveCarToPool();
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                timer = 0;
            }
        }
    }
}