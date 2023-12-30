namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Collections;

    [HelpURL("https://simpletrafficsystem.turnthegameon.com/documentation/api/aitrafficcar")]
    public class AITrafficCar : MonoBehaviour
    {
        public int assignedIndex { get; private set; }
        [Tooltip("Vehicles will only spawn, and merge onto routes with matching vehicle types.")]
        public AITrafficVehicleType vehicleType = AITrafficVehicleType.Default;
        [Tooltip("Amount of torque that is passed to car Wheel Colliders when not braking.")]
        public float accelerationPower = 1500;
        [Tooltip("Respawn the car to the first route point on it's spawn route when the car comes to a stop.")]
        public bool goToStartOnStop;
        [Tooltip("Car max speed, assigned to AITrafficController when car is registered.")]
        public float topSpeed = 25f;
        [Tooltip("Minimum amount of drag applied to car Rigidbody when not braking.")]
        public float minDrag = 0.3f;
        [Tooltip("Minimum amount of angular drag applied to car Rigidbody when not braking.")]
        public float minAngularDrag = 0.3f;

        [Tooltip("Size of the front detection sensor BoxCast.")]
        public Vector3 frontSensorSize = new Vector3(1.3f, 1f, 0.001f);
        [Tooltip("Length of the front detection sensor BoxCast.")]
        public float frontSensorLength = 10f;
        [Tooltip("Size of the side detection sensor BoxCasts.")]
        public Vector3 sideSensorSize = new Vector3(15f, 1f, 0.001f);
        [Tooltip("Length of the side detection sensor BoxCasts.")]
        public float sideSensorLength = 5f;

        [Tooltip("Material used for brake light emission. If unassigned, the material assigned to the brakeMaterialMesh will be used.")]
        public Material brakeMaterial;
        [Tooltip("If brakeMaterial is unassigned, the material assigned to the brakeMaterialIndex will be used.")]
        public MeshRenderer brakeMaterialMesh;
        [Tooltip("Mesh Renderer material array index to get brakeMaterial from.")]
        public int brakeMaterialIndex;
        [Tooltip("Control point to orient/position the front detection sensor. ")]
        public Transform frontSensorTransform;
        [Tooltip("Control point to orient/position the left detection sensor.")]
        public Transform leftSensorTransform;
        [Tooltip("Control point to orient/position the right detection sensor.")]
        public Transform rightSensorTransform;
        [Tooltip("Light toggled on/off based on pooling cullHeadLight zone.")]
        public Light headLight;
        [Tooltip("References to car wheel mesh object, transform, and collider.")]
        public AITrafficCarWheels[] _wheels;
        private AITrafficWaypointRoute startRoute;
        private Vector3 goToPointWhenStoppedVector3;
        private Rigidbody rb;
        private List<int> newRoutePointsMatchingType = new List<int>();
        private int randomIndex;

        public void RegisterCar(AITrafficWaypointRoute route)
        {
            if (brakeMaterial == null && brakeMaterialMesh != null)
            {
                brakeMaterial = brakeMaterialMesh.materials[brakeMaterialIndex];
            }
            assignedIndex = AITrafficController.Instance.RegisterCarAI(this, route);
            startRoute = route;
            rb = GetComponent<Rigidbody>();
        }

        #region Public API Methods
        /// These methods can be used to get AITrafficCar variables and call functions
        /// intended to be used by other MonoBehaviours.

        /// <summary>
        /// Returns current acceleration input as a float 0-1.
        /// </summary>
        /// <returns></returns>
        public float AccelerationInput()
        {
            return AITrafficController.Instance.GetAccelerationInput(assignedIndex);
        }

        /// <summary>
        /// Returns current steering input as a float -1 to 1.
        /// </summary>
        /// <returns></returns>
        public float SteeringInput()
        {
            return AITrafficController.Instance.GetSteeringInput(assignedIndex);
        }

        /// <summary>
        /// Returns current speed as a float.
        /// </summary>
        /// <returns></returns>
        public float CurrentSpeed()
        {
            return AITrafficController.Instance.GetCurrentSpeed(assignedIndex);
        }

        /// <summary>
        /// Returns current breaking input state as a bool.
        /// </summary>
        /// <returns></returns>
        public bool IsBraking()
        {
            return AITrafficController.Instance.GetIsBraking(assignedIndex);
        }

        /// <summary>
        /// Returns true if left sensor is triggered.
        /// </summary>
        /// <returns></returns>
        public bool IsLeftSensor()
        {
            return AITrafficController.Instance.IsLeftSensor(assignedIndex);
        }

        /// <summary>
        /// Returns true if right sensor is triggered.
        /// </summary>
        /// <returns></returns>
        public bool IsRightSensor()
        {
            return AITrafficController.Instance.IsRightSensor(assignedIndex);
        }

        /// <summary>
        /// Returns true if front sensor is triggered.
        /// </summary>
        /// <returns></returns>
        public bool IsFrontSensor()
        {
            return AITrafficController.Instance.IsFrontSensor(assignedIndex);
        }

        /// <summary>
        /// The AITrafficCar will start driving.
        /// </summary>
        [ContextMenu("StartDriving")]
        public void StartDriving()
        {
            AITrafficController.Instance.Set_IsDrivingArray(assignedIndex, true);
        }

        /// <summary>
        /// The AITrafficCar will stop driving.
        /// </summary>
        [ContextMenu("StopDriving")]
        public void StopDriving()
        {
            if (goToStartOnStop)
            {
                ChangeToRouteWaypoint(startRoute.waypointDataList[0]._waypoint.onReachWaypointSettings);
                goToPointWhenStoppedVector3 = startRoute.waypointDataList[0]._transform.position;
                goToPointWhenStoppedVector3.y += 1;
                transform.position = goToPointWhenStoppedVector3;
                transform.LookAt(startRoute.waypointDataList[1]._transform);
                rb.velocity = Vector3.zero;
            }
            else
            {
                AITrafficController.Instance.Set_IsDrivingArray(assignedIndex, false);
            }
        }

        /// <summary>
        /// Disables the AITrafficCar and returns it to the AITrafficController pool.
        /// </summary>
        [ContextMenu("MoveCarToPool")]
        public void MoveCarToPool()
        {
            AITrafficController.Instance.MoveCarToPool(assignedIndex);
        }

        /// <summary>
        /// Disables the AITrafficCar and returns it to the AITrafficController pool.
        /// </summary>
        [ContextMenu("EnableAIProcessing")]
        public void EnableAIProcessing()
        {
            AITrafficController.Instance.Set_CanProcess(assignedIndex, true);
        }

        /// <summary>
        /// Disables the AITrafficCar and returns it to the AITrafficController pool.
        /// </summary>
        [ContextMenu("DisableAIProcessing")]
        public void DisableAIProcessing()
        {
            AITrafficController.Instance.Set_CanProcess(assignedIndex, false);
        }

        /// <summary>
        /// Updates the AITrafficController top speed value for this AITrafficCar.
        /// </summary>
        public void SetTopSpeed(float _value)
        {
            topSpeed = _value;
            AITrafficController.Instance.SetTopSpeed(assignedIndex, topSpeed);
        }

        /// <summary>
        /// Controls an override flag that requests the car to attempt a lane change when able.
        /// </summary>
        public void SetForceLaneChange(bool _value)
        {
            AITrafficController.Instance.SetForceLaneChange(assignedIndex, _value);
        }
        #endregion

        #region Waypoint Trigger Methods
        /// <summary>
        /// Callback triggered when the AITrafficCar reaches a waypoint.
        /// </summary>
        /// <param name="onReachWaypointSettings"></param>
        public void OnReachedWaypoint(AITrafficWaypointSettings onReachWaypointSettings)
        {
            if (onReachWaypointSettings.parentRoute == AITrafficController.Instance.GetCarRoute(assignedIndex))
            {
                onReachWaypointSettings.OnReachWaypointEvent.Invoke();
                AITrafficController.Instance.Set_SpeedLimitArray(assignedIndex, onReachWaypointSettings.speedLimit);
                AITrafficController.Instance.Set_RouteProgressArray(assignedIndex, onReachWaypointSettings.waypointIndexnumber - 1);
                AITrafficController.Instance.Set_WaypointDataListCountArray(assignedIndex);
                if (onReachWaypointSettings.newRoutePoints.Length > 0)
                {
                    newRoutePointsMatchingType.Clear();
                    for (int i = 0; i < onReachWaypointSettings.newRoutePoints.Length; i++)
                    {
                        for (int j = 0; j < onReachWaypointSettings.newRoutePoints[i].onReachWaypointSettings.parentRoute.vehicleTypes.Length; j++)
                        {
                            if (onReachWaypointSettings.newRoutePoints[i].onReachWaypointSettings.parentRoute.vehicleTypes[j] == vehicleType)
                            {
                                newRoutePointsMatchingType.Add(i);
                                break;
                            }
                        }
                    }
                    if (newRoutePointsMatchingType.Count > 0 && onReachWaypointSettings.waypointIndexnumber != onReachWaypointSettings.parentRoute.waypointDataList.Count)
                    {
                        randomIndex = UnityEngine.Random.Range(0, newRoutePointsMatchingType.Count);
                        if (randomIndex == newRoutePointsMatchingType.Count) randomIndex -= 1;
                        randomIndex = newRoutePointsMatchingType[randomIndex];
                        AITrafficController.Instance.Set_WaypointRoute(assignedIndex, onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.parentRoute);
                        AITrafficController.Instance.Set_RouteInfo(assignedIndex, onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.parentRoute.routeInfo);
                        AITrafficController.Instance.Set_RouteProgressArray(assignedIndex, onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.waypointIndexnumber - 1);
                        AITrafficController.Instance.Set_CurrentRoutePointIndexArray
                            (
                            assignedIndex,
                            onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.waypointIndexnumber - 1,
                            onReachWaypointSettings.newRoutePoints[randomIndex]
                            );
                    }
                    else if (onReachWaypointSettings.waypointIndexnumber == onReachWaypointSettings.parentRoute.waypointDataList.Count)
                    {
                        randomIndex = UnityEngine.Random.Range(0, onReachWaypointSettings.newRoutePoints.Length);
                        if (randomIndex == onReachWaypointSettings.newRoutePoints.Length) randomIndex -= 1;
                        AITrafficController.Instance.Set_WaypointRoute(assignedIndex, onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.parentRoute);
                        AITrafficController.Instance.Set_RouteInfo(assignedIndex, onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.parentRoute.routeInfo);
                        AITrafficController.Instance.Set_RouteProgressArray(assignedIndex, onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.waypointIndexnumber - 1);
                        AITrafficController.Instance.Set_CurrentRoutePointIndexArray
                            (
                            assignedIndex,
                            onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.waypointIndexnumber - 1,
                            onReachWaypointSettings.newRoutePoints[randomIndex]
                            );
                    }
                    else
                    {
                        AITrafficController.Instance.Set_CurrentRoutePointIndexArray
                        (
                        assignedIndex,
                        onReachWaypointSettings.waypointIndexnumber,
                        onReachWaypointSettings.waypoint
                        );
                    }
                }
                else if (onReachWaypointSettings.waypointIndexnumber < onReachWaypointSettings.parentRoute.waypointDataList.Count)
                {
                    AITrafficController.Instance.Set_CurrentRoutePointIndexArray
                        (
                        assignedIndex,
                        onReachWaypointSettings.waypointIndexnumber,
                        onReachWaypointSettings.waypoint
                        );
                }
                AITrafficController.Instance.Set_RoutePointPositionArray(assignedIndex);
                if (onReachWaypointSettings.stopDriving)
                {
                    StopDriving();
                    if (onReachWaypointSettings.stopTime > 0)
                    {
                        StartCoroutine(ResumeDrivingTimer(onReachWaypointSettings.stopTime));
                    }
                }
            }
        }

        /// <summary>
        /// Used by AITrafficController to instruct the AITrafficCar to change lanes.
        /// </summary>
        /// <param name="onReachWaypointSettings"></param>
        public void ChangeToRouteWaypoint(AITrafficWaypointSettings onReachWaypointSettings)
        {
            onReachWaypointSettings.OnReachWaypointEvent.Invoke();
            AITrafficController.Instance.Set_SpeedLimitArray(assignedIndex, onReachWaypointSettings.speedLimit);
            AITrafficController.Instance.Set_WaypointDataListCountArray(assignedIndex);
            AITrafficController.Instance.Set_WaypointRoute(assignedIndex, onReachWaypointSettings.parentRoute);
            AITrafficController.Instance.Set_RouteInfo(assignedIndex, onReachWaypointSettings.parentRoute.routeInfo);
            AITrafficController.Instance.Set_RouteProgressArray(assignedIndex, onReachWaypointSettings.waypointIndexnumber - 1);
            AITrafficController.Instance.Set_CurrentRoutePointIndexArray
                (
                assignedIndex,
                onReachWaypointSettings.waypointIndexnumber,
                onReachWaypointSettings.waypoint
                );

            AITrafficController.Instance.Set_RoutePointPositionArray(assignedIndex);
        }
        #endregion

        #region Callbacks
        void OnBecameInvisible()
        {
#if UNITY_EDITOR
            if (Camera.current != null)
            {
                if (Camera.current.name == "SceneCamera")
                    return;
            }
#endif
            AITrafficController.Instance.SetVisibleState(assignedIndex, false);
        }

        void OnBecameVisible()
        {
#if UNITY_EDITOR
            if (Camera.current != null)
            {
                if (Camera.current.name == "SceneCamera")
                    return;
            }
#endif
            AITrafficController.Instance.SetVisibleState(assignedIndex, true);
        }
        #endregion

        IEnumerator ResumeDrivingTimer(float _stopTime)
        {
            yield return new WaitForSeconds(_stopTime);
            StartDriving();
        }
    }
}