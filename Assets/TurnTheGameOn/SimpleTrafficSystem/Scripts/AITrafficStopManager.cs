namespace TurnTheGameOn.SimpleTrafficSystem
{
    using System.Collections;
    using UnityEngine;

    [HelpURL("https://simpletrafficsystem.turnthegameon.com/documentation/api/aitrafficstopmanager")]
    public class AITrafficStopManager : MonoBehaviour
    {
        [Tooltip("Array of AITrafficStopCycles played as a looped sequence.")]
        public AITrafficStopCycle[] stopCycles;

        private void Start()
        {
            StopAllRoutes();
            StartCoroutine(StartTrafficLightCycles());
        }

        private void StopAllRoutes()
        {
            for (int i = 0; i < stopCycles.Length; i++)
            {
                for (int j = 0; j < stopCycles[i].trafficStops.Length; j++)
                {
                    stopCycles[i].trafficStops[j].StopTraffic();
                }
            }
        }

        IEnumerator StartTrafficLightCycles()
        {
            while (true)
            {
                for (int i = 0; i < stopCycles.Length; i++)
                {
                    for (int j = 0; j < stopCycles[i].trafficStops.Length; j++)
                    {
                        stopCycles[i].trafficStops[j].AllowCarToProceed();
                    }
                    yield return new WaitForSeconds(stopCycles[i].activeTime);
                    for (int j = 0; j < stopCycles[i].trafficStops.Length; j++)
                    {
                        stopCycles[i].trafficStops[j].StopTraffic();
                    }
                    yield return new WaitForSeconds(stopCycles[i].waitTime);
                }
            }
        }

    }
}