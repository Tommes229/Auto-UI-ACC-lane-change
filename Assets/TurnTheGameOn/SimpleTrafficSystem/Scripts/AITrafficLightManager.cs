namespace TurnTheGameOn.SimpleTrafficSystem
{
    using System.Collections;
    using UnityEngine;

    [HelpURL("https://simpletrafficsystem.turnthegameon.com/documentation/api/aitrafficlightmanager")]
    public class AITrafficLightManager : MonoBehaviour
    {
        [Tooltip("Array of AITrafficLightCycles played as a looped sequence.")]
        public AITrafficLightCycle[] trafficLightCycles;

        private void Start()
        {
            if (trafficLightCycles.Length > 0)
            {
                EnableRedLights();
                StartCoroutine(StartTrafficLightCycles());
            }
            else
            {
                Debug.LogWarning("There are no lights assigned to this TrafficLightManger, it will be disabled.");
                enabled = false;
            }
        }

        private void EnableRedLights()
        {
            for (int i = 0; i < trafficLightCycles.Length; i++)
            {
                for (int j = 0; j < trafficLightCycles[i].trafficLights.Length; j++)
                {
                    trafficLightCycles[i].trafficLights[j].EnableRedLight();
                }
            }
        }

        IEnumerator StartTrafficLightCycles()
        {
            while (true)
            {
                for (int i = 0; i < trafficLightCycles.Length; i++)
                {
                    for (int j = 0; j < trafficLightCycles[i].trafficLights.Length; j++)
                    {
                        trafficLightCycles[i].trafficLights[j].EnableGreenLight();
                    }
                    yield return new WaitForSeconds(trafficLightCycles[i].greenTimer);
                    for (int j = 0; j < trafficLightCycles[i].trafficLights.Length; j++)
                    {
                        trafficLightCycles[i].trafficLights[j].EnableYellowLight();
                    }
                    yield return new WaitForSeconds(trafficLightCycles[i].yellowTimer);
                    for (int j = 0; j < trafficLightCycles[i].trafficLights.Length; j++)
                    {
                        trafficLightCycles[i].trafficLights[j].EnableRedLight();
                    }
                    yield return new WaitForSeconds(trafficLightCycles[i].redtimer);
                }
            }
        }

    }
}