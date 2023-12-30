namespace TurnTheGameOn.SimpleTrafficSystem
{
    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Jobs;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.Jobs;

    [HelpURL("https://simpletrafficsystem.turnthegameon.com/documentation/api/aitrafficcontroller")]
    public class AITrafficController : MonoBehaviour
    {
        public static AITrafficController Instance;

        #region Public Variables and Registers
        public int carCount { get; private set; }
        public int currentDensity { get; private set; }

        [Tooltip("Array of AITrafficCar prefabs to spawn.")]
        public AITrafficCar[] trafficPrefabs;

        #region Car Settings
        [Tooltip("Enables the processing of YieldTrigger logic.")]
        public bool useYieldTriggers;
        [Tooltip("Multiplier used for calculating speed; 2.23693629 by default for MPH.")]
        public float speedMultiplier = 2.23693629f;
        [Tooltip("Multiplier used to control how quickly the car's front wheels turn toward the target direction.")]
        public float steerSensitivity = 0.02f;
        [Tooltip("Maximum angle the car's front wheels are allowed to turn toward the target direction.")]
        public float maxSteerAngle = 37f;
        [Tooltip("Front detection sensor distance at which a car will start braking.")]
        public float stopThreshold = 5f;

        [Tooltip("Physics layers the detection sensors can detect.")]
        public LayerMask layerMask;
        [Tooltip("Size of the front detection sensor BoxCast.")]
        public Vector3 frontSensorSize = new Vector3(1.01f, 1f, 0.001f);
        [Tooltip("Rotates the front sensor to face the next waypoint.")]
        public bool frontSensorFacesTarget = false;

        public WheelFrictionCurve lowSidewaysWheelFrictionCurve = new WheelFrictionCurve();
        public WheelFrictionCurve highSidewaysWheelFrictionCurve = new WheelFrictionCurve();

        [Tooltip("Enables the processing of Lane Changing logic.")]
        public bool useLaneChanging;
        [Tooltip("Minimum amount of time until a car is allowed to change lanes once conditions are met.")]
        public float changeLaneTrigger = 3f;
        [Tooltip("Minimum speed required to change lanes.")]
        public float minSpeedToChangeLanes = 5f;
        [Tooltip("Minimum time required after changing lanes before allowed to change lanes again.")]
        public float changeLaneCooldown = 20f;

        [Tooltip("Dummy material used for brake light emission logic when a car does not have an assigned brake variable.")]
        public Material unassignedBrakeMaterial;
        public float brakeOnIntensityURP = 1f;
        public float brakeOnIntensityHDRP = 10f;
        public float brakeOnIntensityDP = 10f;
        public float brakeOffIntensityURP = -3f;
        public float brakeOffIntensityHDRP = 0f;
        public float brakeOffIntensityDP = -3f;
        private Color brakeColor = Color.red;
        private Color brakeOnColor;
        private Color brakeOffColor;
        private float brakeIntensityFactor;
        private string emissionColorName;

        [Tooltip("AI Cars will be parented to the 'Car Parent' transform, this AITrafficController will be the parent if a parent is not assigned.")]
        public bool setCarParent;
        [Tooltip("If 'Set Car Parent' is enabled, AI Cars will be parented to this transform, this AITrafficController will be the parent if a parent is not defined.")]
        public Transform carParent;
        #endregion

        #region Pooling
        [Tooltip("Toggle the inspector and debug warnings about how the scene camera can impact pooling behavior.")]
        public bool showPoolingWarning = true;
        [Tooltip("Enables the processing of Pooling logic.")]
        public bool usePooling;
        [Tooltip("Transform that pooling distances will be checked against.")]
        public Transform centerPoint;
        [Tooltip("When using pooling, cars will not spawn to a route if the route limit is met.")]
        public bool useRouteLimit;
        [Tooltip("Max amount of cars placed in the pooling system on scene start.")]
        public int carsInPool = 200;
        [Tooltip("Max amount of cars the pooling system is allowed to spawn, must be equal or lower than cars in pool.")]
        public int density = 200;
        [Tooltip("Frequency at which pooling spawn is performed.")]
        public float spawnRate = 2;
        [Tooltip("The position that cars are sent to when being disabled.")]
        public Vector3 disabledPosition = new Vector3(0, -2000, 0);
        [Tooltip("Cars can't spawn or despawn in this zone.")]
        public float minSpawnZone = 50;
        [Tooltip("Car headlights will be disabled outside of this zone.")]
        public float cullHeadLight = 100;
        [Tooltip("Cars only spawn if the spawn point is not visible by the camera.")]
        public float actizeZone = 225;
        [Tooltip("Cars can spawn anywhere in this zone, even if spawn point is visible by the camera. Cars outside of this zone will be despawned.")]
        public float spawnZone = 350;
        #endregion

        #region Set Array Data
        public void Set_IsDrivingArray(int _index, bool _value)
        {
            if (isDrivingNL[_index] != _value)
            {
                isBrakingNL[_index] = _value == true ? false : true;
                isDrivingNL[_index] = _value;
                if (_value == false)
                {
                    motorTorqueNL[_index] = 0;
                    brakeTorqueNL[_index] = -1;
                    moveHandBrakeNL[_index] = 1;
                    for (int j = 0; j < 4; j++) // move
                    {
                        if (j == 0)
                        {
                            currentWheelCollider = frontRightWheelColliderList[_index];
                            currentWheelCollider.steerAngle = steerAngleNL[_index];
                            currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);
                            FRwheelPositionNL[_index] = wheelPosition_Cached;
                            FRwheelRotationNL[_index] = wheelQuaternion_Cached;
                        }
                        else if (j == 1)
                        {
                            currentWheelCollider = frontLefttWheelColliderList[_index];
                            currentWheelCollider.steerAngle = steerAngleNL[_index];
                            currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);
                            FLwheelPositionNL[_index] = wheelPosition_Cached;
                            FLwheelRotationNL[_index] = wheelQuaternion_Cached;
                        }
                        else if (j == 2)
                        {
                            currentWheelCollider = backRighttWheelColliderList[_index];
                            currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);
                            BRwheelPositionNL[_index] = wheelPosition_Cached;
                            BRwheelRotationNL[_index] = wheelQuaternion_Cached;
                        }
                        else if (j == 3)
                        {
                            currentWheelCollider = backLeftWheelColliderList[_index];
                            currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);
                            BLwheelPositionNL[_index] = wheelPosition_Cached;
                            BLwheelRotationNL[_index] = wheelQuaternion_Cached;
                        }
                        currentWheelCollider.motorTorque = motorTorqueNL[_index];
                        currentWheelCollider.brakeTorque = brakeTorqueNL[_index];
                    }
                }
            }
        }
        public void Set_RouteInfo(int _index, AITrafficWaypointRouteInfo routeInfo)
        {
            carAIWaypointRouteInfo[_index] = routeInfo;
        }
        public void Set_CurrentRoutePointIndexArray(int _index, int _value, AITrafficWaypoint _nextWaypoint)
        {
            currentRoutePointIndexNL[_index] = _value;
            currentWaypointList[_index] = _nextWaypoint;
            isChangingLanesNL[_index] = false;
        }
        public void Set_RouteProgressArray(int _index, float _value)
        {
            routeProgressNL[_index] = _value;
        }
        public void Set_SpeedLimitArray(int _index, float _value)
        {
            speedLimitNL[_index] = _value;
        }
        public void Set_WaypointDataListCountArray(int _index)
        {
            waypointDataListCountNL[_index] = carRouteList[_index].waypointDataList.Count;
        }
        public void Set_RoutePointPositionArray(int _index)
        {
            routePointPositionNL[_index] = carRouteList[_index].waypointDataList[currentRoutePointIndexNL[_index]]._transform.position;
            finalRoutePointPositionNL[_index] = carRouteList[_index].waypointDataList[carRouteList[_index].waypointDataList.Count - 1]._transform.position;
        }
        public void SetVisibleState(int _index, bool _isVisible)
        {
            if (isVisibleNL.IsCreated) isVisibleNL[_index] = _isVisible;
        }
        public void Set_WaypointRoute(int _index, AITrafficWaypointRoute _route)
        {
            carRouteList[_index] = _route;
        }
        public void Set_CanProcess(int _index, bool _value)
        {
            canProcessNL[_index] = _value;
        }
        public void SetTopSpeed(int _index, float _value)
        {
            topSpeedNL[_index] = _value;
        }
        public void SetForceLaneChange(int _index, bool _value)
        {
            forceChangeLanesNL[_index] = _value;
        }
        public void SetChangeToRouteWaypoint(int _index, AITrafficWaypointSettings _onReachWaypointSettings)
        {
            carList[_index].ChangeToRouteWaypoint(_onReachWaypointSettings);
            isChangingLanesNL[_index] = true;
            canChangeLanesNL[_index] = false;
            forceChangeLanesNL[_index] = false;
            changeLaneTriggerTimer[_index] = 0f;
        }
        #endregion

        #region Get Array Data
        public float GetAccelerationInput(int _index)
        {
            return accelerationInputNL[_index];
        }
        public float GetSteeringInput(int _index)
        {
            return steerAngleNL[_index];
        }
        public float GetCurrentSpeed(int _index)
        {
            return speedNL[_index];
        }
        public bool GetIsBraking(int _index)
        {
            return isBrakingNL[_index];
        }
        public bool IsLeftSensor(int _index)
        {
            return leftHitNL[_index];
        }
        public bool IsRightSensor(int _index)
        {
            return rightHitNL[_index];
        }
        public bool IsFrontSensor(int _index)
        {
            return frontHitNL[_index];
        }
        public bool GetIsDisabled(int _index)
        {
            return isDisabledNL[_index];
        }
        public Vector3 GetFrontSensorPosition(int _index)
        {
            return frontSensorTransformPositionNL[_index];
        }
        public Vector3 GetCarPosition(int _index)
        {
            return carTransformPositionNL[_index];
        }
        public Vector3 GetCarTargetPosition(int _index)
        {
            return driveTargetTAA[_index].position;
        }
        public AITrafficWaypointRoute GetCarRoute(int _index)
        {
            return carRouteList[_index];
        }
        public AITrafficCar[] GetTrafficCars()
        {
            return carList.ToArray();
        }
        public AITrafficWaypointRoute[] GetRoutes()
        {
            return allWaypointRoutesList.ToArray();
        }
        public AITrafficSpawnPoint[] GetSpawnPoints()
        {
            return trafficSpawnPoints.ToArray();
        }
        public AITrafficWaypoint GetCurrentWaypoint(int _index)
        {
            return currentWaypointList[_index];
        }
        #endregion

        #region Registers
        public int RegisterCarAI(AITrafficCar carAI, AITrafficWaypointRoute route)
        {
            carList.Add(carAI);
            carRouteList.Add(route);
            currentWaypointList.Add(null);
            changeLaneCooldownTimer.Add(0);
            changeLaneTriggerTimer.Add(0);
            frontDirectionList.Add(Vector3.zero);
            frontRotationList.Add(Quaternion.identity);
            frontTransformCached.Add(carAI.frontSensorTransform);
            frontHitTransform.Add(null);
            frontPreviousHitTransform.Add(null);
            leftOriginList.Add(Vector3.zero);
            leftDirectionList.Add(Vector3.zero);
            leftRotationList.Add(Quaternion.identity);
            leftTransformCached.Add(carAI.leftSensorTransform);
            leftHitTransform.Add(null);
            leftPreviousHitTransform.Add(null);
            rightOriginList.Add(Vector3.zero);
            rightDirectionList.Add(Vector3.zero);
            rightRotationList.Add(Quaternion.identity);
            rightTransformCached.Add(carAI.rightSensorTransform);
            rightHitTransform.Add(null);
            rightPreviousHitTransform.Add(null);
            carAIWaypointRouteInfo.Add(null);
            if (carAI.brakeMaterial == null)
            {
                brakeMaterial.Add(unassignedBrakeMaterial);
            }
            else
            {
                brakeMaterial.Add(carAI.brakeMaterial);
                carAI.brakeMaterial.EnableKeyword("_EMISSION");
            }
            frontRightWheelColliderList.Add(carAI._wheels[0].collider);
            frontLefttWheelColliderList.Add(carAI._wheels[1].collider);
            backRighttWheelColliderList.Add(carAI._wheels[2].collider);
            backLeftWheelColliderList.Add(carAI._wheels[3].collider);
            Rigidbody rigidbody = carAI.GetComponent<Rigidbody>();
            rigidbodyList.Add(rigidbody);
            headLight.Add(carAI.headLight);
            Transform driveTarget = new GameObject("DriveTarget").transform;
            driveTarget.SetParent(carAI.transform);
            TransformAccessArray temp_driveTargetTAA = new TransformAccessArray(carCount);
            for (int i = 0; i < carCount; i++)
            {
                temp_driveTargetTAA.Add(driveTargetTAA[i]);
            }
            temp_driveTargetTAA.Add(driveTarget);
            carCount = carList.Count;
            if (carCount >= 2)
            {
                DisposeArrays(false);
            }
            #region allocation
            currentRoutePointIndexNL.Add(0);
            waypointDataListCountNL.Add(0);
            carTransformPreviousPositionNL.Add(Vector3.zero);
            carTransformPositionNL.Add(Vector3.zero);
            finalRoutePointPositionNL.Add(float3.zero);
            routePointPositionNL.Add(float3.zero);
            forceChangeLanesNL.Add(false);
            isChangingLanesNL.Add(false);
            canChangeLanesNL.Add(true);
            isDrivingNL.Add(true);
            isActiveNL.Add(true);
            speedNL.Add(0);
            routeProgressNL.Add(0);
            targetSpeedNL.Add(0);
            accelNL.Add(0);
            speedLimitNL.Add(0);
            targetAngleNL.Add(0);
            dragNL.Add(0);
            angularDragNL.Add(0);
            overrideDragNL.Add(false);
            localTargetNL.Add(Vector3.zero);
            steerAngleNL.Add(0);
            motorTorqueNL.Add(0);
            accelerationInputNL.Add(0);
            brakeTorqueNL.Add(0);
            moveHandBrakeNL.Add(0);
            overrideInputNL.Add(false);
            distanceToEndPointNL.Add(999);
            overrideAccelerationPowerNL.Add(0);
            overrideBrakePowerNL.Add(0);
            isBrakingNL.Add(false);
            FRwheelPositionNL.Add(float3.zero);
            FRwheelRotationNL.Add(Quaternion.identity);
            FLwheelPositionNL.Add(float3.zero);
            FLwheelRotationNL.Add(Quaternion.identity);
            BRwheelPositionNL.Add(float3.zero);
            BRwheelRotationNL.Add(Quaternion.identity);
            BLwheelPositionNL.Add(float3.zero);
            BLwheelRotationNL.Add(Quaternion.identity);
            frontSensorLengthNL.Add(carAI.frontSensorLength);
            sideSensorLengthNL.Add(carAI.sideSensorLength);
            sideSensorSizeNL.Add(carAI.sideSensorSize);
            frontSensorTransformPositionNL.Add(carAI.frontSensorTransform.position);
            previousFrameSpeedNL.Add(0f);
            brakeTimeNL.Add(0f);
            topSpeedNL.Add(carAI.topSpeed);
            minDragNL.Add(carAI.minDrag);
            minAngularDragNL.Add(carAI.minAngularDrag);
            frontHitDistanceNL.Add(carAI.frontSensorLength);
            leftHitDistanceNL.Add(carAI.sideSensorLength);
            rightHitDistanceNL.Add(carAI.sideSensorLength);
            frontHitNL.Add(false);
            leftHitNL.Add(false);
            rightHitNL.Add(false);
            stopForTrafficLightNL.Add(false);
            yieldForCrossTrafficNL.Add(false);
            routeIsActiveNL.Add(false);
            isVisibleNL.Add(false);
            isDisabledNL.Add(false);
            withinLimitNL.Add(false);
            distanceToPlayerNL.Add(0);
            accelerationPowerNL.Add(carAI.accelerationPower);
            isEnabledNL.Add(false);
            outOfBoundsNL.Add(false);
            lightIsActiveNL.Add(false);
            canProcessNL.Add(true);
            driveTargetTAA = new TransformAccessArray(carCount);
            carTAA = new TransformAccessArray(carCount);
            frontRightWheelTAA = new TransformAccessArray(carCount);
            frontLeftWheelTAA = new TransformAccessArray(carCount);
            backRightWheelTAA = new TransformAccessArray(carCount);
            backLeftWheelTAA = new TransformAccessArray(carCount);
            #endregion
            waypointDataListCountNL[carCount - 1] = carRouteList[carCount - 1].waypointDataList.Count;
            carAIWaypointRouteInfo[carCount - 1] = carRouteList[carCount - 1].routeInfo;
            for (int i = 0; i < carCount; i++)
            {
                driveTargetTAA.Add(temp_driveTargetTAA[i]);
                carTAA.Add(carList[i].transform);
                frontRightWheelTAA.Add(carList[i]._wheels[0].meshTransform);
                frontLeftWheelTAA.Add(carList[i]._wheels[1].meshTransform);
                backRightWheelTAA.Add(carList[i]._wheels[2].meshTransform);
                backLeftWheelTAA.Add(carList[i]._wheels[3].meshTransform);
            }
            temp_driveTargetTAA.Dispose();
            return carCount - 1;
        }
        public int RegisterSpawnPoint(AITrafficSpawnPoint _TrafficSpawnPoint)
        {
            int index = trafficSpawnPoints.Count;
            trafficSpawnPoints.Add(_TrafficSpawnPoint);
            return index;
        }
        public void RemoveSpawnPoint(AITrafficSpawnPoint _TrafficSpawnPoint)
        {
            trafficSpawnPoints.Remove(_TrafficSpawnPoint);
            availableSpawnPoints.Clear();
        }
        public int RegisterAITrafficWaypointRoute(AITrafficWaypointRoute _route)
        {
            int index = allWaypointRoutesList.Count;
            allWaypointRoutesList.Add(_route);
            return index;
        }
        public void RemoveAITrafficWaypointRoute(AITrafficWaypointRoute _route)
        {
            allWaypointRoutesList.Remove(_route);
        }
        #endregion

        #endregion

        #region Private Variables
        private List<AITrafficCar> carList = new List<AITrafficCar>();
        private List<AITrafficWaypointRouteInfo> carAIWaypointRouteInfo = new List<AITrafficWaypointRouteInfo>();
        private List<AITrafficWaypointRoute> allWaypointRoutesList = new List<AITrafficWaypointRoute>();
        private List<AITrafficWaypointRoute> carRouteList = new List<AITrafficWaypointRoute>();
        private List<AITrafficWaypoint> currentWaypointList = new List<AITrafficWaypoint>();
        private List<AITrafficSpawnPoint> trafficSpawnPoints = new List<AITrafficSpawnPoint>();
        private List<AITrafficSpawnPoint> availableSpawnPoints = new List<AITrafficSpawnPoint>();
        private List<WheelCollider> frontRightWheelColliderList = new List<WheelCollider>();
        private List<WheelCollider> frontLefttWheelColliderList = new List<WheelCollider>();
        private List<WheelCollider> backRighttWheelColliderList = new List<WheelCollider>();
        private List<WheelCollider> backLeftWheelColliderList = new List<WheelCollider>();
        private List<Rigidbody> rigidbodyList = new List<Rigidbody>();
        private List<Transform> frontTransformCached = new List<Transform>();
        private List<Transform> frontHitTransform = new List<Transform>();
        private List<Transform> frontPreviousHitTransform = new List<Transform>();
        private List<Transform> leftTransformCached = new List<Transform>();
        private List<Transform> leftHitTransform = new List<Transform>();
        private List<Transform> leftPreviousHitTransform = new List<Transform>();
        private List<Transform> rightTransformCached = new List<Transform>();
        private List<Transform> rightHitTransform = new List<Transform>();
        private List<Transform> rightPreviousHitTransform = new List<Transform>();
        private List<Material> brakeMaterial = new List<Material>();
        private List<Light> headLight = new List<Light>();
        private List<float> changeLaneTriggerTimer = new List<float>();
        private List<float> changeLaneCooldownTimer = new List<float>();
        private List<Vector3> frontDirectionList = new List<Vector3>();
        private List<Vector3> leftOriginList = new List<Vector3>();
        private List<Vector3> leftDirectionList = new List<Vector3>();
        private List<Vector3> rightOriginList = new List<Vector3>();
        private List<Vector3> rightDirectionList = new List<Vector3>();
        private List<Quaternion> leftRotationList = new List<Quaternion>();
        private List<Quaternion> frontRotationList = new List<Quaternion>();
        private List<Quaternion> rightRotationList = new List<Quaternion>();
        private List<AITrafficPoolEntry> trafficPool = new List<AITrafficPoolEntry>();
        private NativeList<int> currentRoutePointIndexNL;
        private NativeList<int> waypointDataListCountNL;
        private NativeList<bool> canProcessNL;
        private NativeList<bool> forceChangeLanesNL;
        private NativeList<bool> isChangingLanesNL;
        private NativeList<bool> canChangeLanesNL;
        private NativeList<bool> frontHitNL;
        private NativeList<bool> leftHitNL;
        private NativeList<bool> rightHitNL;
        private NativeList<bool> yieldForCrossTrafficNL;
        private NativeList<bool> stopForTrafficLightNL;
        private NativeList<bool> routeIsActiveNL;
        private NativeList<bool> isActiveNL;
        private NativeList<bool> isDrivingNL;
        private NativeList<bool> overrideDragNL;
        private NativeList<bool> overrideInputNL;
        private NativeList<bool> isBrakingNL;
        private NativeList<bool> withinLimitNL;
        private NativeList<bool> isEnabledNL;
        private NativeList<bool> outOfBoundsNL;
        private NativeList<bool> lightIsActiveNL;
        private NativeList<bool> isVisibleNL;
        private NativeList<bool> isDisabledNL;
        private NativeList<float> frontHitDistanceNL;
        private NativeList<float> leftHitDistanceNL;
        private NativeList<float> rightHitDistanceNL;
        private NativeList<Vector3> frontSensorTransformPositionNL;
        private NativeList<float> frontSensorLengthNL;
        private NativeList<float> sideSensorLengthNL;
        private NativeList<Vector3> sideSensorSizeNL;
        private NativeList<float> previousFrameSpeedNL;
        private NativeList<float> brakeTimeNL;
        private NativeList<float> topSpeedNL;
        private NativeList<float> minDragNL;
        private NativeList<float> minAngularDragNL;
        private NativeList<float> speedNL;
        private NativeList<float> routeProgressNL;
        private NativeList<float> targetSpeedNL;
        private NativeList<float> accelNL;
        private NativeList<float> speedLimitNL;
        private NativeList<float> targetAngleNL;
        private NativeList<float> dragNL;
        private NativeList<float> angularDragNL;
        private NativeList<float> steerAngleNL;
        private NativeList<float> accelerationInputNL;
        private NativeList<float> motorTorqueNL;
        private NativeList<float> brakeTorqueNL;
        private NativeList<float> moveHandBrakeNL;
        private NativeList<float> overrideAccelerationPowerNL;
        private NativeList<float> overrideBrakePowerNL;
        private NativeList<float> distanceToPlayerNL;
        private NativeList<float> accelerationPowerNL;
        private NativeList<float> distanceToEndPointNL;
        private NativeList<float3> finalRoutePointPositionNL;
        private NativeList<float3> routePointPositionNL;
        private NativeList<float3> FRwheelPositionNL;
        private NativeList<float3> FLwheelPositionNL;
        private NativeList<float3> BRwheelPositionNL;
        private NativeList<float3> BLwheelPositionNL;
        private NativeList<Vector3> carTransformPreviousPositionNL;
        private NativeList<Vector3> localTargetNL;
        private NativeList<Vector3> carTransformPositionNL;
        private NativeList<quaternion> FRwheelRotationNL;
        private NativeList<quaternion> FLwheelRotationNL;
        private NativeList<quaternion> BRwheelRotationNL;
        private NativeList<quaternion> BLwheelRotationNL;
        private TransformAccessArray driveTargetTAA;
        private TransformAccessArray carTAA;
        private TransformAccessArray frontRightWheelTAA;
        private TransformAccessArray frontLeftWheelTAA;
        private TransformAccessArray backRightWheelTAA;
        private TransformAccessArray backLeftWheelTAA;
        private JobHandle jobHandle;
        private AITrafficCarJob carAITrafficJob;
        private AITrafficCarWheelJob frAITrafficCarWheelJob;
        private AITrafficCarWheelJob flAITrafficCarWheelJob;
        private AITrafficCarWheelJob brAITrafficCarWheelJob;
        private AITrafficCarWheelJob blAITrafficCarWheelJob;
        private AITrafficCarPositionJob carTransformpositionJob;
        private AITrafficDistanceJob _AITrafficDistanceJob;
        private float3 centerPosition;
        private float spawnTimer;
        private float distanceToSpawnPoint;
        private float startTime;
        private float deltaTime;
        private float dragToAdd;
        private int currentAmountToSpawn;
        private int randomSpawnPointIndex;
        private bool canTurnLeft, canTurnRight;
        private bool isInitialized;
        private Vector3 relativePoint;
        private Vector3 wheelPosition_Cached;
        private Vector3 spawnPosition;
        private Vector3 spawnOffset = new Vector3(0, -4, 0);
        private Vector3 frontSensorEulerAngles;
        private Quaternion wheelQuaternion_Cached;
        private RaycastHit boxHit;
        private WheelCollider currentWheelCollider;
        private AITrafficCar spawncar;
        private AITrafficCar loadCar;
        private AITrafficWaypoint nextWaypoint;
        private AITrafficPoolEntry newTrafficPoolEntry = new AITrafficPoolEntry();
        private int PossibleTargetDirection(Transform _from, Transform _to)
        {
            relativePoint = _from.InverseTransformPoint(_to.position);
            if (relativePoint.x < 0.0) return -1;
            else if (relativePoint.x > 0.0) return 1;
            else return 0;
        }
        #endregion

        #region Main Methods
        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
                currentRoutePointIndexNL = new NativeList<int>(Allocator.Persistent);
                waypointDataListCountNL = new NativeList<int>(Allocator.Persistent);
                carTransformPreviousPositionNL = new NativeList<Vector3>(Allocator.Persistent);
                carTransformPositionNL = new NativeList<Vector3>(Allocator.Persistent);
                finalRoutePointPositionNL = new NativeList<float3>(Allocator.Persistent);
                routePointPositionNL = new NativeList<float3>(Allocator.Persistent);
                forceChangeLanesNL = new NativeList<bool>(Allocator.Persistent);
                isChangingLanesNL = new NativeList<bool>(Allocator.Persistent);
                canChangeLanesNL = new NativeList<bool>(Allocator.Persistent);
                isDrivingNL = new NativeList<bool>(Allocator.Persistent);
                isActiveNL = new NativeList<bool>(Allocator.Persistent);
                canProcessNL = new NativeList<bool>(Allocator.Persistent);
                speedNL = new NativeList<float>(Allocator.Persistent);
                routeProgressNL = new NativeList<float>(Allocator.Persistent);
                targetSpeedNL = new NativeList<float>(Allocator.Persistent);
                accelNL = new NativeList<float>(Allocator.Persistent);
                speedLimitNL = new NativeList<float>(Allocator.Persistent);
                targetAngleNL = new NativeList<float>(Allocator.Persistent);
                dragNL = new NativeList<float>(Allocator.Persistent);
                angularDragNL = new NativeList<float>(Allocator.Persistent);
                overrideDragNL = new NativeList<bool>(Allocator.Persistent);
                localTargetNL = new NativeList<Vector3>(Allocator.Persistent);
                steerAngleNL = new NativeList<float>(Allocator.Persistent);
                motorTorqueNL = new NativeList<float>(Allocator.Persistent);
                accelerationInputNL = new NativeList<float>(Allocator.Persistent);
                brakeTorqueNL = new NativeList<float>(Allocator.Persistent);
                moveHandBrakeNL = new NativeList<float>(Allocator.Persistent);
                overrideInputNL = new NativeList<bool>(Allocator.Persistent);
                distanceToEndPointNL = new NativeList<float>(Allocator.Persistent);
                overrideAccelerationPowerNL = new NativeList<float>(Allocator.Persistent);
                overrideBrakePowerNL = new NativeList<float>(Allocator.Persistent);
                isBrakingNL = new NativeList<bool>(Allocator.Persistent);
                FRwheelPositionNL = new NativeList<float3>(Allocator.Persistent);
                FRwheelRotationNL = new NativeList<quaternion>(Allocator.Persistent);
                FLwheelPositionNL = new NativeList<float3>(Allocator.Persistent);
                FLwheelRotationNL = new NativeList<quaternion>(Allocator.Persistent);
                BRwheelPositionNL = new NativeList<float3>(Allocator.Persistent);
                BRwheelRotationNL = new NativeList<quaternion>(Allocator.Persistent);
                BLwheelPositionNL = new NativeList<float3>(Allocator.Persistent);
                BLwheelRotationNL = new NativeList<quaternion>(Allocator.Persistent);
                previousFrameSpeedNL = new NativeList<float>(Allocator.Persistent);
                brakeTimeNL = new NativeList<float>(Allocator.Persistent);
                topSpeedNL = new NativeList<float>(Allocator.Persistent);
                frontSensorTransformPositionNL = new NativeList<Vector3>(Allocator.Persistent);
                frontSensorLengthNL = new NativeList<float>(Allocator.Persistent);
                sideSensorLengthNL = new NativeList<float>(Allocator.Persistent);
                sideSensorSizeNL = new NativeList<Vector3>(Allocator.Persistent);
                minDragNL = new NativeList<float>(Allocator.Persistent);
                minAngularDragNL = new NativeList<float>(Allocator.Persistent);
                frontHitDistanceNL = new NativeList<float>(Allocator.Persistent);
                leftHitDistanceNL = new NativeList<float>(Allocator.Persistent);
                rightHitDistanceNL = new NativeList<float>(Allocator.Persistent);
                frontHitNL = new NativeList<bool>(Allocator.Persistent);
                leftHitNL = new NativeList<bool>(Allocator.Persistent);
                rightHitNL = new NativeList<bool>(Allocator.Persistent);
                stopForTrafficLightNL = new NativeList<bool>(Allocator.Persistent);
                yieldForCrossTrafficNL = new NativeList<bool>(Allocator.Persistent);
                routeIsActiveNL = new NativeList<bool>(Allocator.Persistent);
                isVisibleNL = new NativeList<bool>(Allocator.Persistent);
                isDisabledNL = new NativeList<bool>(Allocator.Persistent);
                withinLimitNL = new NativeList<bool>(Allocator.Persistent);
                distanceToPlayerNL = new NativeList<float>(Allocator.Persistent);
                accelerationPowerNL = new NativeList<float>(Allocator.Persistent);
                isEnabledNL = new NativeList<bool>(Allocator.Persistent);
                outOfBoundsNL = new NativeList<bool>(Allocator.Persistent);
                lightIsActiveNL = new NativeList<bool>(Allocator.Persistent);
            }
            else
            {
                Debug.LogWarning("Multiple AITrafficController Instances found in scene, this is not allowed. Destroying this duplicate AITrafficController.");
                Destroy(this);
            }
        }

        private void Start()
        {
            if (usePooling)
            {
                StartCoroutine(SpawnStartupTrafficCoroutine());
                if (showPoolingWarning)
                {
                    Debug.LogWarning("NOTE: " +
                        "OnBecameVisible and OnBecameInvisible are used by cars and spawn points to determine if they are visible.\n" +
                        "These callbacks are also triggered by the editor scene camera.\n" +
                        "Hide the scene view while testing for the most accurate simulation, which is what the final build will be.\n" +
                        "Not hiding the scene view camera may cause objcets to register the wrong state, resulting in unproper behavior.");
                }
            }
            else
            {
                StartCoroutine(Initialize());
            }
            // sideways friction
            lowSidewaysWheelFrictionCurve.extremumSlip = 0.2f;
            lowSidewaysWheelFrictionCurve.extremumValue = 1f;
            lowSidewaysWheelFrictionCurve.asymptoteSlip = 0.5f;
            lowSidewaysWheelFrictionCurve.asymptoteValue = 0.75f;
            lowSidewaysWheelFrictionCurve.stiffness = 1f;
            highSidewaysWheelFrictionCurve.extremumSlip = 0.2f;
            highSidewaysWheelFrictionCurve.extremumValue = 1f;
            highSidewaysWheelFrictionCurve.asymptoteSlip = 0.5f;
            highSidewaysWheelFrictionCurve.asymptoteValue = 0.75f;
            highSidewaysWheelFrictionCurve.stiffness = 5f;
            brakeIntensityFactor = Mathf.Pow(2, RenderPipeline.IsDefaultRP ? brakeOnIntensityDP : RenderPipeline.IsURP ? brakeOnIntensityURP : brakeOnIntensityHDRP);
            brakeOnColor = new Color(brakeColor.r * brakeIntensityFactor, brakeColor.g * brakeIntensityFactor, brakeColor.b * brakeIntensityFactor);
            brakeIntensityFactor = Mathf.Pow(2, RenderPipeline.IsDefaultRP ? brakeOffIntensityDP : RenderPipeline.IsURP ? brakeOffIntensityURP : brakeOffIntensityHDRP);
            brakeOffColor = new Color(brakeColor.r * brakeIntensityFactor, brakeColor.g * brakeIntensityFactor, brakeColor.b * brakeIntensityFactor);
            emissionColorName = RenderPipeline.IsDefaultRP || RenderPipeline.IsURP ? "_EmissionColor" : "_EmissiveColor";
            unassignedBrakeMaterial = new Material(unassignedBrakeMaterial);
        }

        IEnumerator Initialize()
        {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < carCount; i++)
            {
                routePointPositionNL[i] = carRouteList[i].waypointDataList[currentRoutePointIndexNL[i]]._transform.position;
                finalRoutePointPositionNL[i] = carRouteList[i].waypointDataList[carRouteList[i].waypointDataList.Count - 1]._transform.position;
                carList[i].StartDriving();
            }
            if (setCarParent)
            {
                if (carParent == null) carParent = transform;
                for (int i = 0; i < carCount; i++)
                {
                    carList[i].transform.SetParent(carParent);
                }
            }
            isInitialized = true;
        }

        private void FixedUpdate()
        {
            if (isInitialized)
            {
                if (STSPrefs.debugProcessTime) startTime = Time.realtimeSinceStartup;
                deltaTime = Time.deltaTime;
                if (useYieldTriggers)
                {
                    for (int i = 0; i < carCount; i++)
                    {
                        yieldForCrossTrafficNL[i] = false;
                        if (currentWaypointList[i] != null)
                        {
                            if (currentWaypointList[i].onReachWaypointSettings.nextPointInRoute != null)
                            {
                                for (int j = 0; j < currentWaypointList[i].onReachWaypointSettings.nextPointInRoute.onReachWaypointSettings.yieldTriggers.Count; j++)
                                {
                                    if (currentWaypointList[i].onReachWaypointSettings.nextPointInRoute.onReachWaypointSettings.yieldTriggers[j].yieldForTrafficLight == true)
                                    {
                                        yieldForCrossTrafficNL[i] = true;
                                        break;
                                    }
                                }
                            }
                        }
                        stopForTrafficLightNL[i] = carAIWaypointRouteInfo[i].stopForTrafficLight;
                    }
                }
                else
                {
                    for (int i = 0; i < carCount; i++)
                    {
                        yieldForCrossTrafficNL[i] = false;
                        stopForTrafficLightNL[i] = carAIWaypointRouteInfo[i].stopForTrafficLight;
                        //frontSensorTransformPositionNL[i] = frontTransformCached[i].position; // make a job?
                    }
                }
                carAITrafficJob = new AITrafficCarJob
                {
                    frontSensorLengthNA = frontSensorLengthNL,
                    currentRoutePointIndexNA = currentRoutePointIndexNL,
                    waypointDataListCountNA = waypointDataListCountNL,
                    carTransformPreviousPositionNA = carTransformPreviousPositionNL,
                    carTransformPositionNA = carTransformPositionNL,
                    finalRoutePointPositionNA = finalRoutePointPositionNL,
                    routePointPositionNA = routePointPositionNL,
                    isDrivingNA = isDrivingNL,
                    isActiveNA = isActiveNL,
                    canProcessNA = canProcessNL,
                    speedNA = speedNL,
                    deltaTime = deltaTime,
                    routeProgressNA = routeProgressNL,
                    topSpeedNA = topSpeedNL,
                    targetSpeedNA = targetSpeedNL,
                    speedLimitNA = speedLimitNL,
                    accelNA = accelNL,
                    localTargetNA = localTargetNL,
                    targetAngleNA = targetAngleNL,
                    steerAngleNA = steerAngleNL,
                    motorTorqueNA = motorTorqueNL,
                    accelerationInputNA = accelerationInputNL,
                    brakeTorqueNA = brakeTorqueNL,
                    moveHandBrakeNA = moveHandBrakeNL,
                    maxSteerAngle = maxSteerAngle,
                    overrideInputNA = overrideInputNL,
                    distanceToEndPointNA = distanceToEndPointNL,
                    overrideAccelerationPowerNA = overrideAccelerationPowerNL,
                    overrideBrakePowerNA = overrideBrakePowerNL,
                    isBrakingNA = isBrakingNL,
                    speedMultiplier = speedMultiplier,
                    steerSensitivity = steerSensitivity,
                    stopThreshold = stopThreshold,
                    frontHitDistanceNA = frontHitDistanceNL,
                    frontHitNA = frontHitNL,
                    stopForTrafficLightNA = stopForTrafficLightNL,
                    yieldForCrossTrafficNA = yieldForCrossTrafficNL,
                    accelerationPowerNA = accelerationPowerNL,
                    frontSensorTransformPositionNA = frontSensorTransformPositionNL,
                };
                jobHandle = carAITrafficJob.Schedule(driveTargetTAA);
                jobHandle.Complete();

                for (int i = 0; i < carCount; i++) // operate on results
                {
                    if (isActiveNL[i] && canProcessNL[i])
                    {
                        #region sensor logic
                        if (isDrivingNL[i])
                        {
                            #region Front Sensor
                            /// Front Sensor
                            if (frontSensorFacesTarget)
                            {
                                if (currentWaypointList[i])
                                {
                                    frontTransformCached[i].LookAt(currentWaypointList[i].onReachWaypointSettings.nextPointInRoute.transform);
                                    frontSensorEulerAngles = frontTransformCached[i].rotation.eulerAngles;
                                    frontSensorEulerAngles.x = 0;
                                    frontSensorEulerAngles.z = 0;
                                    frontTransformCached[i].rotation = Quaternion.Euler(frontSensorEulerAngles);
                                }
                            }

                            frontSensorTransformPositionNL[i] = frontTransformCached[i].position;
                            frontDirectionList[i] = frontTransformCached[i].forward;
                            frontRotationList[i] = frontTransformCached[i].rotation;
                            if (Physics.BoxCast(
                                //frontOriginList[i],
                                frontSensorTransformPositionNL[i],
                                frontSensorSize,
                                frontDirectionList[i],
                                out boxHit,
                                frontRotationList[i],
                                frontSensorLengthNL[i],
                                layerMask,
                                QueryTriggerInteraction.UseGlobal))
                            {
                                frontHitTransform[i] = boxHit.transform; // cache transform lookup
                                if (frontHitTransform[i] != frontPreviousHitTransform[i])
                                {
                                    frontPreviousHitTransform[i] = frontHitTransform[i];
                                }
                                frontHitDistanceNL[i] = boxHit.distance;
                                frontHitNL[i] = true;
                            }
                            else if (frontHitNL[i] != false) //ResetHitBox
                            {
                                frontHitDistanceNL[i] = frontSensorLengthNL[i];
                                frontHitNL[i] = false;
                            }
                            #endregion

                            #region Lane Change
                            if (useLaneChanging)
                            {
                                if (speedNL[i] > minSpeedToChangeLanes)
                                {
                                    if (!canChangeLanesNL[i])
                                    {
                                        changeLaneCooldownTimer[i] += deltaTime;
                                        if (changeLaneCooldownTimer[i] > changeLaneCooldown)
                                        {
                                            canChangeLanesNL[i] = true;
                                            changeLaneCooldownTimer[i] = 0f;
                                        }
                                    }

                                    if ((forceChangeLanesNL[i] == true || frontHitNL[i] == true) && canChangeLanesNL[i] && isChangingLanesNL[i] == false)
                                    {
                                        changeLaneTriggerTimer[i] += Time.deltaTime;

                                        #region Left Sensor
                                        /// Left Sensor
                                        leftOriginList[i] = leftTransformCached[i].position;
                                        leftDirectionList[i] = leftTransformCached[i].forward;
                                        leftRotationList[i] = leftTransformCached[i].rotation;
                                        if (Physics.BoxCast(
                                            leftOriginList[i],
                                            sideSensorSizeNL[i],
                                            leftDirectionList[i],
                                            out boxHit,
                                            leftRotationList[i],
                                            sideSensorLengthNL[i],
                                            layerMask,
                                            QueryTriggerInteraction.UseGlobal))
                                        {
                                            leftHitTransform[i] = boxHit.transform; // cache transform lookup
                                            if (leftHitTransform[i] != leftPreviousHitTransform[i])
                                            {
                                                leftPreviousHitTransform[i] = leftHitTransform[i];
                                            }
                                            leftHitDistanceNL[i] = boxHit.distance;
                                            leftHitNL[i] = true;
                                        }
                                        else if (leftHitNL[i] != false) //ResetHitBox
                                        {
                                            leftHitDistanceNL[i] = sideSensorLengthNL[i];
                                            leftHitNL[i] = false;
                                        }
                                        #endregion

                                        #region Right Sensor
                                        /// Right Sensor
                                        rightOriginList[i] = rightTransformCached[i].position;
                                        rightDirectionList[i] = rightTransformCached[i].forward;
                                        rightRotationList[i] = rightTransformCached[i].rotation;
                                        if (Physics.BoxCast(
                                            rightOriginList[i],
                                            sideSensorSizeNL[i],
                                            rightDirectionList[i],
                                            out boxHit,
                                            rightRotationList[i],
                                            sideSensorLengthNL[i],
                                            layerMask,
                                            QueryTriggerInteraction.UseGlobal))
                                        {
                                            rightHitTransform[i] = boxHit.transform; // cache transform lookup
                                            if (rightHitTransform[i] != rightPreviousHitTransform[i])
                                            {
                                                rightPreviousHitTransform[i] = rightHitTransform[i];
                                            }
                                            rightHitDistanceNL[i] = boxHit.distance;
                                            rightHitNL[i] = true;
                                        }
                                        else if (rightHitNL[i] != false) //ResetHitBox
                                        {
                                            rightHitDistanceNL[i] = sideSensorLengthNL[i];
                                            rightHitNL[i] = false;
                                        }
                                        #endregion

                                        canTurnLeft = leftHitNL[i] == true ? false : true;
                                        canTurnRight = rightHitNL[i] == true ? false : true;

                                        if (changeLaneTriggerTimer[i] >= changeLaneTrigger || forceChangeLanesNL[i] == true)
                                        {
                                            canChangeLanesNL[i] = false;
                                            nextWaypoint = currentWaypointList[i];

                                            if (nextWaypoint != null)
                                            {
                                                if (nextWaypoint.onReachWaypointSettings.laneChangePoints.Count > 0)  // take the first alternate route
                                                {
                                                    for (int j = 0; j < nextWaypoint.onReachWaypointSettings.laneChangePoints.Count; j++)
                                                    {
                                                        if (
                                                            PossibleTargetDirection(carTAA[i], nextWaypoint.onReachWaypointSettings.laneChangePoints[j].transform) == -1 && canTurnLeft ||
                                                            PossibleTargetDirection(carTAA[i], nextWaypoint.onReachWaypointSettings.laneChangePoints[j].transform) == 1 && canTurnRight
                                                            )
                                                        {
                                                            for (int k = 0; k < nextWaypoint.onReachWaypointSettings.laneChangePoints[j].onReachWaypointSettings.parentRoute.vehicleTypes.Length; k++)
                                                            {
                                                                if (carList[i].vehicleType == nextWaypoint.onReachWaypointSettings.laneChangePoints[j].onReachWaypointSettings.parentRoute.vehicleTypes[k])
                                                                {
                                                                    carList[i].ChangeToRouteWaypoint(nextWaypoint.onReachWaypointSettings.laneChangePoints[j].onReachWaypointSettings);
                                                                    isChangingLanesNL[i] = true;
                                                                    canChangeLanesNL[i] = false;
                                                                    forceChangeLanesNL[i] = false;
                                                                    changeLaneTriggerTimer[i] = 0f;
                                                                }
                                                            }
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        changeLaneTriggerTimer[i] = 0f;
                                        leftHitNL[i] = false;
                                        rightHitNL[i] = false;
                                        leftHitDistanceNL[i] = sideSensorLengthNL[i];
                                        rightHitDistanceNL[i] = sideSensorLengthNL[i];
                                    }
                                }
                            }
                            #endregion
                        }
                        if ((speedNL[i] == 0 || !overrideInputNL[i]))
                        {
                            rigidbodyList[i].drag = minDragNL[i];
                            rigidbodyList[i].angularDrag = minAngularDragNL[i];
                        }
                        else if (overrideInputNL[i])
                        {
                            isBrakingNL[i] = true;
                            if (frontHitNL[i])
                            {
                                motorTorqueNL[i] = 0;
                                brakeTorqueNL[i] = Mathf.InverseLerp(0, frontSensorLengthNL[i], frontHitDistanceNL[i]) * (speedNL[i]);
                                dragToAdd = Mathf.InverseLerp(0, frontSensorLengthNL[i], frontHitDistanceNL[i]) * ((speedNL[i]));
                                if (frontHitDistanceNL[i] < 1) dragToAdd = targetSpeedNL[i] * (speedNL[i] * 50);

                                rigidbodyList[i].drag = minDragNL[i] + (Mathf.InverseLerp(0, frontSensorLengthNL[i], frontHitDistanceNL[i]) * dragToAdd);
                                rigidbodyList[i].angularDrag = minAngularDragNL[i] + Mathf.InverseLerp(0, frontSensorLengthNL[i], frontHitDistanceNL[i] * dragToAdd);
                            }
                            else
                            {
                                motorTorqueNL[i] = 0;
                                //brakeTorqueNL[i] = (speedNL[i] * 0.5f);
                                dragToAdd = Mathf.InverseLerp(5, 0, distanceToEndPointNL[i]);
                                rigidbodyList[i].drag = dragToAdd;
                                rigidbodyList[i].angularDrag = dragToAdd;
                            }
                            changeLaneTriggerTimer[i] = 0;
                        }
                        #endregion

                        for (int j = 0; j < 4; j++) // move
                        {
                            if (j == 0)
                            {
                                currentWheelCollider = frontRightWheelColliderList[i];
                                currentWheelCollider.steerAngle = steerAngleNL[i];
                                currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);
                                FRwheelPositionNL[i] = wheelPosition_Cached;
                                FRwheelRotationNL[i] = wheelQuaternion_Cached;
                            }
                            else if (j == 1)
                            {
                                currentWheelCollider = frontLefttWheelColliderList[i];
                                currentWheelCollider.steerAngle = steerAngleNL[i];
                                currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);
                                FLwheelPositionNL[i] = wheelPosition_Cached;
                                FLwheelRotationNL[i] = wheelQuaternion_Cached;
                            }
                            else if (j == 2)
                            {
                                currentWheelCollider = backRighttWheelColliderList[i];
                                currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);
                                BRwheelPositionNL[i] = wheelPosition_Cached;
                                BRwheelRotationNL[i] = wheelQuaternion_Cached;
                            }
                            else if (j == 3)
                            {
                                currentWheelCollider = backLeftWheelColliderList[i];
                                currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);
                                BLwheelPositionNL[i] = wheelPosition_Cached;
                                BLwheelRotationNL[i] = wheelQuaternion_Cached;
                            }
                            currentWheelCollider.motorTorque = motorTorqueNL[i];
                            currentWheelCollider.brakeTorque = brakeTorqueNL[i];
                            currentWheelCollider.sidewaysFriction = speedNL[i] < 1 ? lowSidewaysWheelFrictionCurve : highSidewaysWheelFrictionCurve;
                        }

                        if ((frontHitNL[i] && speedNL[i] < (previousFrameSpeedNL[i] + 5)) || overrideDragNL[i])
                            isBrakingNL[i] = true;

                        if (speedNL[i] + .5f > previousFrameSpeedNL[i] && speedNL[i] > 15 && frontHitNL[i])
                            isBrakingNL[i] = false;

                        if (isBrakingNL[i])
                        {
                            brakeTimeNL[i] += deltaTime;
                            if (brakeTimeNL[i] > 0.15f)
                            {
                                brakeMaterial[i].SetColor(emissionColorName, brakeOnColor); //brakeMaterial[i].EnableKeyword("EMISSION");
                            }
                        }
                        else
                        {
                            brakeTimeNL[i] = 0f;
                            brakeMaterial[i].SetColor(emissionColorName, brakeOffColor); //brakeMaterial[i].EnableKeyword("EMISSION");
                        }
                        previousFrameSpeedNL[i] = speedNL[i];
                    }
                }

                carTransformpositionJob = new AITrafficCarPositionJob
                {
                    canProcessNA = canProcessNL,
                    carTransformPreviousPositionNA = carTransformPreviousPositionNL,
                    carTransformPositionNA = carTransformPositionNL,
                };
                jobHandle = carTransformpositionJob.Schedule(carTAA);
                jobHandle.Complete();

                frAITrafficCarWheelJob = new AITrafficCarWheelJob
                {
                    canProcessNA = canProcessNL,
                    wheelPositionNA = FRwheelPositionNL,
                    wheelQuaternionNA = FRwheelRotationNL,
                    speedNA = speedNL,
                };
                jobHandle = frAITrafficCarWheelJob.Schedule(frontRightWheelTAA);
                jobHandle.Complete();

                flAITrafficCarWheelJob = new AITrafficCarWheelJob
                {
                    canProcessNA = canProcessNL,
                    wheelPositionNA = FLwheelPositionNL,
                    wheelQuaternionNA = FLwheelRotationNL,
                    speedNA = speedNL,
                };
                jobHandle = flAITrafficCarWheelJob.Schedule(frontLeftWheelTAA);
                jobHandle.Complete();

                brAITrafficCarWheelJob = new AITrafficCarWheelJob
                {
                    canProcessNA = canProcessNL,
                    wheelPositionNA = BRwheelPositionNL,
                    wheelQuaternionNA = BRwheelRotationNL,
                    speedNA = speedNL,
                };
                jobHandle = brAITrafficCarWheelJob.Schedule(backRightWheelTAA);
                jobHandle.Complete();

                blAITrafficCarWheelJob = new AITrafficCarWheelJob
                {
                    canProcessNA = canProcessNL,
                    wheelPositionNA = BLwheelPositionNL,
                    wheelQuaternionNA = BLwheelRotationNL,
                    speedNA = speedNL,
                };
                jobHandle = blAITrafficCarWheelJob.Schedule(backLeftWheelTAA);
                jobHandle.Complete();

                if (usePooling)
                {
                    centerPosition = centerPoint.position;
                    _AITrafficDistanceJob = new AITrafficDistanceJob
                    {
                        canProcessNA = canProcessNL,
                        playerPosition = centerPosition,
                        distanceToPlayerNA = distanceToPlayerNL,
                        isVisibleNA = isVisibleNL,
                        withinLimitNA = withinLimitNL,
                        cullDistance = cullHeadLight,
                        lightIsActiveNA = lightIsActiveNL,
                        outOfBoundsNA = outOfBoundsNL,
                        actizeZone = actizeZone,
                        spawnZone = spawnZone,
                        isDisabledNA = isDisabledNL,
                    };
                    jobHandle = _AITrafficDistanceJob.Schedule(carTAA);
                    jobHandle.Complete();
                    for (int i = 0; i < allWaypointRoutesList.Count; i++)
                    {
                        allWaypointRoutesList[i].previousDensity = allWaypointRoutesList[i].currentDensity;
                        allWaypointRoutesList[i].currentDensity = 0;
                    }
                    for (int i = 0; i < carCount; i++)
                    {
                        if (canProcessNL[i])
                        {
                            if (isDisabledNL[i] == false)
                            {
                                carRouteList[i].currentDensity += 1;
                                if (outOfBoundsNL[i])
                                {
                                    MoveCarToPool(carList[i].assignedIndex);
                                }
                            }
                            else if (outOfBoundsNL[i] == false)
                            {
                                if (lightIsActiveNL[i])
                                {
                                    if (isEnabledNL[i] == false)
                                    {
                                        isEnabledNL[i] = true;
                                        headLight[i].enabled = true;
                                    }
                                }
                                else
                                {
                                    if (isEnabledNL[i])
                                    {
                                        isEnabledNL[i] = false;
                                        headLight[i].enabled = false;
                                    }
                                }
                            }
                        }
                    }
                    if (spawnTimer >= spawnRate) SpawnTraffic();
                    else spawnTimer += deltaTime;
                }

                if (STSPrefs.debugProcessTime) Debug.Log((("AI Update " + (Time.realtimeSinceStartup - startTime) * 1000f)) + "ms");
            }
        }

        private void OnDestroy()
        {
            DisposeArrays(true);
        }

        void DisposeArrays(bool _isQuit)
        {
            if (_isQuit)
            {
                currentRoutePointIndexNL.Dispose();
                waypointDataListCountNL.Dispose();
                carTransformPreviousPositionNL.Dispose();
                carTransformPositionNL.Dispose();
                finalRoutePointPositionNL.Dispose();
                routePointPositionNL.Dispose();
                forceChangeLanesNL.Dispose();
                isChangingLanesNL.Dispose();
                canChangeLanesNL.Dispose();
                isDrivingNL.Dispose();
                isActiveNL.Dispose();
                speedNL.Dispose();
                routeProgressNL.Dispose();
                targetSpeedNL.Dispose();
                accelNL.Dispose();
                speedLimitNL.Dispose();
                targetAngleNL.Dispose();
                dragNL.Dispose();
                angularDragNL.Dispose();
                overrideDragNL.Dispose();
                localTargetNL.Dispose();
                steerAngleNL.Dispose();
                motorTorqueNL.Dispose();
                accelerationInputNL.Dispose();
                brakeTorqueNL.Dispose();
                moveHandBrakeNL.Dispose();
                overrideInputNL.Dispose();
                distanceToEndPointNL.Dispose();
                overrideAccelerationPowerNL.Dispose();
                overrideBrakePowerNL.Dispose();
                isBrakingNL.Dispose();
                FRwheelPositionNL.Dispose();
                FRwheelRotationNL.Dispose();
                FLwheelPositionNL.Dispose();
                FLwheelRotationNL.Dispose();
                BRwheelPositionNL.Dispose();
                BRwheelRotationNL.Dispose();
                BLwheelPositionNL.Dispose();
                BLwheelRotationNL.Dispose();
                previousFrameSpeedNL.Dispose();
                brakeTimeNL.Dispose();
                topSpeedNL.Dispose();
                frontSensorTransformPositionNL.Dispose();
                frontSensorLengthNL.Dispose();
                sideSensorLengthNL.Dispose();
                sideSensorSizeNL.Dispose();
                minDragNL.Dispose();
                minAngularDragNL.Dispose();
                frontHitDistanceNL.Dispose();
                leftHitDistanceNL.Dispose();
                rightHitDistanceNL.Dispose();
                frontHitNL.Dispose();
                leftHitNL.Dispose();
                rightHitNL.Dispose();
                stopForTrafficLightNL.Dispose();
                yieldForCrossTrafficNL.Dispose();
                routeIsActiveNL.Dispose();
                isVisibleNL.Dispose();
                isDisabledNL.Dispose();
                withinLimitNL.Dispose();
                distanceToPlayerNL.Dispose();
                accelerationPowerNL.Dispose();
                isEnabledNL.Dispose();
                outOfBoundsNL.Dispose();
                lightIsActiveNL.Dispose();
                canProcessNL.Dispose();
            }
            driveTargetTAA.Dispose();
            carTAA.Dispose();
            frontRightWheelTAA.Dispose();
            frontLeftWheelTAA.Dispose();
            backRightWheelTAA.Dispose();
            backLeftWheelTAA.Dispose();
        }
        #endregion

        #region Gizmos
        private bool spawnPointsAreHidden;
        private Vector3 gizmoOffset;
        private Matrix4x4 cubeTransform;
        private Matrix4x4 oldGizmosMatrix;

        void OnDrawGizmos()
        {
            if (STSPrefs.sensorGizmos && Application.isPlaying)
            {
                for (int i = 0; i < carTransformPositionNL.Length; i++)
                {
                    if (isActiveNL[i] && canProcessNL[i])
                    {
                        ///// Front Sensor Gizmo
                        Gizmos.color = frontHitDistanceNL[i] == frontSensorLengthNL[i] ? STSPrefs.normalColor : STSPrefs.detectColor;
                        gizmoOffset = new Vector3(frontSensorSize.x * 2.0f, frontSensorSize.y * 2.0f, frontHitDistanceNL[i]);
                        DrawCube(frontSensorTransformPositionNL[i] + frontDirectionList[i] * (frontHitDistanceNL[i] / 2), frontRotationList[i], gizmoOffset);
                        if (STSPrefs.sideSensorGizmos)
                        {
                            #region Left Sensor
                            /// Left Sensor
                            leftOriginList[i] = leftTransformCached[i].position;
                            leftDirectionList[i] = leftTransformCached[i].forward;
                            leftRotationList[i] = leftTransformCached[i].rotation;
                            if (Physics.BoxCast(
                                leftOriginList[i],
                                sideSensorSizeNL[i],
                                leftDirectionList[i],
                                out boxHit,
                                leftRotationList[i],
                                sideSensorLengthNL[i],
                                layerMask,
                                QueryTriggerInteraction.UseGlobal))
                            {
                                leftHitTransform[i] = boxHit.transform; // cache transform lookup
                                if (leftHitTransform[i] != leftPreviousHitTransform[i])
                                {
                                    leftPreviousHitTransform[i] = leftHitTransform[i];
                                }
                                leftHitDistanceNL[i] = boxHit.distance;
                                leftHitNL[i] = true;
                            }
                            else if (leftHitNL[i] != false) //ResetHitBox
                            {
                                leftHitDistanceNL[i] = sideSensorLengthNL[i];
                                leftHitNL[i] = false;
                            }
                            ///// Left Sensor Gizmo
                            Gizmos.color = leftHitDistanceNL[i] == sideSensorLengthNL[i] ? STSPrefs.normalColor : STSPrefs.detectColor;
                            gizmoOffset = new Vector3(sideSensorSizeNL[i].x * 2.0f, sideSensorSizeNL[i].y * 2.0f, leftHitDistanceNL[i]);
                            DrawCube(leftOriginList[i] + leftDirectionList[i] * (leftHitDistanceNL[i] / 2), leftRotationList[i], gizmoOffset);
                            #endregion

                            #region Right Sensor
                            /// Right Sensor
                            rightOriginList[i] = rightTransformCached[i].position;
                            rightDirectionList[i] = rightTransformCached[i].forward;
                            rightRotationList[i] = rightTransformCached[i].rotation;
                            if (Physics.BoxCast(
                                rightOriginList[i],
                                sideSensorSizeNL[i],
                                rightDirectionList[i],
                                out boxHit,
                                rightRotationList[i],
                                sideSensorLengthNL[i],
                                layerMask,
                                QueryTriggerInteraction.UseGlobal))
                            {
                                rightHitTransform[i] = boxHit.transform; // cache transform lookup
                                if (rightHitTransform[i] != rightPreviousHitTransform[i])
                                {
                                    rightPreviousHitTransform[i] = rightHitTransform[i];
                                }
                                rightHitDistanceNL[i] = boxHit.distance;
                                rightHitNL[i] = true;
                            }
                            else if (rightHitNL[i] != false) //ResetHitBox
                            {
                                rightHitDistanceNL[i] = sideSensorLengthNL[i];
                                rightHitNL[i] = false;
                            }
                            ///// Right Sensor Gizmo
                            Gizmos.color = rightHitDistanceNL[i] == sideSensorLengthNL[i] ? STSPrefs.normalColor : STSPrefs.detectColor;
                            gizmoOffset = new Vector3(sideSensorSizeNL[i].x * 2.0f, sideSensorSizeNL[i].y * 2.0f, rightHitDistanceNL[i]);
                            DrawCube(rightOriginList[i] + rightDirectionList[i] * (rightHitDistanceNL[i] / 2), rightRotationList[i], gizmoOffset);
                            #endregion
                        }
                        else
                        {
                            if (leftHitNL[i])//(isChangingLanesNL[i] == false && canChangeLanesNL[i]) || m_AITrafficDebug.alwaysSideSensorGizmos)
                            {
                                ///// Left Sensor Gizmo
                                Gizmos.color = leftHitDistanceNL[i] == sideSensorLengthNL[i] ? STSPrefs.normalColor : STSPrefs.detectColor;
                                gizmoOffset = new Vector3(sideSensorSizeNL[i].x * 2.0f, sideSensorSizeNL[i].y * 2.0f, leftHitDistanceNL[i]);
                                DrawCube(leftOriginList[i] + leftDirectionList[i] * (leftHitDistanceNL[i] / 2), leftRotationList[i], gizmoOffset);
                            }
                            else if (rightHitNL[i])
                            {
                                ///// Right Sensor Gizmo
                                Gizmos.color = rightHitDistanceNL[i] == sideSensorLengthNL[i] ? STSPrefs.normalColor : STSPrefs.detectColor;
                                gizmoOffset = new Vector3(sideSensorSizeNL[i].x * 2.0f, sideSensorSizeNL[i].y * 2.0f, rightHitDistanceNL[i]);
                                DrawCube(rightOriginList[i] + rightDirectionList[i] * (rightHitDistanceNL[i] / 2), rightRotationList[i], gizmoOffset);
                            }
                        }
                    }
                }
            }
            if (STSPrefs.hideSpawnPointsInEditMode && spawnPointsAreHidden == false)
            {
                spawnPointsAreHidden = true;
                AITrafficSpawnPoint[] spawnPoints = FindObjectsOfType<AITrafficSpawnPoint>();
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    spawnPoints[i].GetComponent<MeshRenderer>().enabled = false;
                }
            }
            else if (STSPrefs.hideSpawnPointsInEditMode == false && spawnPointsAreHidden)
            {
                spawnPointsAreHidden = false;
                AITrafficSpawnPoint[] spawnPoints = FindObjectsOfType<AITrafficSpawnPoint>();
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    spawnPoints[i].GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }
        void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            cubeTransform = Matrix4x4.TRS(position, rotation, scale);
            oldGizmosMatrix = Gizmos.matrix;
            Gizmos.matrix *= cubeTransform;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = oldGizmosMatrix;
        }
        #endregion

        #region TrafficPool
        public AITrafficCar GetCarFromPool(AITrafficWaypointRoute parentRoute)
        {
            loadCar = null;
            for (int i = 0; i < trafficPool.Count; i++)
            {
                for (int j = 0; j < parentRoute.vehicleTypes.Length; j++)
                {
                    if (trafficPool[i].trafficPrefab.vehicleType == parentRoute.vehicleTypes[j])
                    {
                        loadCar = trafficPool[i].trafficPrefab;
                        isDisabledNL[trafficPool[i].assignedIndex] = false;
                        rigidbodyList[trafficPool[i].assignedIndex].isKinematic = false;
                        EnableCar(carList[trafficPool[i].assignedIndex].assignedIndex, parentRoute);
                        trafficPool.RemoveAt(i);
                        return loadCar;
                    }
                }
            }
            return loadCar;
        }

        public AITrafficCar GetCarFromPool(AITrafficWaypointRoute parentRoute, AITrafficVehicleType vehicleType)
        {
            loadCar = null;
            for (int i = 0; i < trafficPool.Count; i++)
            {
                for (int j = 0; j < parentRoute.vehicleTypes.Length; j++)
                {
                    if (trafficPool[i].trafficPrefab.vehicleType == parentRoute.vehicleTypes[j] &&
                        trafficPool[i].trafficPrefab.vehicleType == vehicleType &&
                        canProcessNL[trafficPool[i].assignedIndex])
                    {
                        loadCar = trafficPool[i].trafficPrefab;
                        isDisabledNL[trafficPool[i].assignedIndex] = false;
                        rigidbodyList[trafficPool[i].assignedIndex].isKinematic = false;
                        EnableCar(carList[trafficPool[i].assignedIndex].assignedIndex, parentRoute);
                        trafficPool.RemoveAt(i);
                        return loadCar;
                    }
                }
            }
            return loadCar;
        }

        public void EnableCar(int _index, AITrafficWaypointRoute parentRoute)
        {
            isActiveNL[_index] = true;
            carList[_index].gameObject.SetActive(true);
            carRouteList[_index] = parentRoute;
            carAIWaypointRouteInfo[_index] = parentRoute.routeInfo;
            carList[_index].StartDriving();
        }

        public void MoveCarToPool(int _index)
        {
            canChangeLanesNL[_index] = false;
            isChangingLanesNL[_index] = false;
            forceChangeLanesNL[_index] = false;
            isDisabledNL[_index] = true;
            isActiveNL[_index] = false;
            carList[_index].StopDriving();
            carList[_index].transform.position = disabledPosition;
            StartCoroutine(MoveCarToPoolCoroutine(_index));
        }

        IEnumerator MoveCarToPoolCoroutine(int _index)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            carList[_index].gameObject.SetActive(false);
            newTrafficPoolEntry = new AITrafficPoolEntry();
            newTrafficPoolEntry.assignedIndex = _index;
            newTrafficPoolEntry.trafficPrefab = carList[_index];
            trafficPool.Add(newTrafficPoolEntry);
        }

        public void MoveAllCarsToPool()
        {
            for (int i = 0; i < isActiveNL.Length; i++)
            {
                if (isActiveNL[i])
                {
                    canChangeLanesNL[i] = false;
                    isChangingLanesNL[i] = false;
                    forceChangeLanesNL[i] = false;
                    isDisabledNL[i] = true;
                    isActiveNL[i] = false;
                    carList[i].StopDriving();
                    StartCoroutine(MoveCarToPoolCoroutine(i));
                }
            }
        }

        void SpawnTraffic()
        {
            spawnTimer = 0f;
            availableSpawnPoints.Clear();
            for (int i = 0; i < trafficSpawnPoints.Count; i++) // Get Available Spawn Points From All Zones
            {
                distanceToSpawnPoint = Vector3.Distance(centerPosition, trafficSpawnPoints[i].transformCached.position);
                if ((distanceToSpawnPoint > actizeZone || (distanceToSpawnPoint > minSpawnZone && trafficSpawnPoints[i].isVisible == false))
                    && distanceToSpawnPoint < spawnZone && trafficSpawnPoints[i].isTrigger == false)
                {
                    availableSpawnPoints.Add(trafficSpawnPoints[i]);
                }
            }
            currentDensity = carList.Count - trafficPool.Count;
            if (currentDensity < density) //Spawn Traffic
            {
                currentAmountToSpawn = density - currentDensity;
                for (int i = 0; i < currentAmountToSpawn; i++)
                {
                    if (availableSpawnPoints.Count == 0 || trafficPool.Count == 0) break;
                    randomSpawnPointIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
                    if (availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.currentDensity < availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.maxDensity)
                    {
                        spawncar = GetCarFromPool(availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute);
                        if (spawncar != null)
                        {
                            availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.currentDensity += 1;
                            spawnPosition = availableSpawnPoints[randomSpawnPointIndex].transformCached.position + spawnOffset;
                            spawncar.transform.SetPositionAndRotation(
                                spawnPosition,
                                availableSpawnPoints[randomSpawnPointIndex].transformCached.rotation
                                );
                            spawncar.transform.LookAt(availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.waypointDataList[availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.waypointIndexnumber]._transform);
                            availableSpawnPoints.RemoveAt(randomSpawnPointIndex);
                        }
                    }
                }
            }
        }

        IEnumerator SpawnStartupTrafficCoroutine()
        {
            yield return new WaitForEndOfFrame();
            availableSpawnPoints.Clear();
            currentDensity = 0;
            currentAmountToSpawn = density - currentDensity;
            for (int i = 0; i < trafficSpawnPoints.Count; i++) // Get Available Spawn Points From All Zones
            {
                distanceToSpawnPoint = Vector3.Distance(centerPosition, trafficSpawnPoints[i].transformCached.position);
                if (trafficSpawnPoints[i].isTrigger == false)
                {
                    availableSpawnPoints.Add(trafficSpawnPoints[i]);
                }
            }
            for (int i = 0; i < density; i++) // Spawn Traffic
            {
                for (int j = 0; j < trafficPrefabs.Length; j++)
                {
                    if (availableSpawnPoints.Count == 0) break;
                    randomSpawnPointIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
                    spawnPosition = availableSpawnPoints[randomSpawnPointIndex].transformCached.position + spawnOffset;
                    for (int k = 0; k < availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.vehicleTypes.Length; k++)
                    {
                        if (currentAmountToSpawn == 0) break;
                        if (availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.vehicleTypes[k] == trafficPrefabs[j].vehicleType)
                        {
                            GameObject spawnedTrafficVehicle = Instantiate(trafficPrefabs[j].gameObject, spawnPosition, availableSpawnPoints[randomSpawnPointIndex].transformCached.rotation);
                            spawnedTrafficVehicle.GetComponent<AITrafficCar>().RegisterCar(availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute);
                            spawnedTrafficVehicle.transform.LookAt(availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.waypointDataList[availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.waypointIndexnumber]._transform);
                            availableSpawnPoints.RemoveAt(randomSpawnPointIndex);
                            currentAmountToSpawn -= 1;
                            break;
                        }
                    }
                    if (currentAmountToSpawn <= 0) break;
                }
            }
            for (int i = 0; i < carsInPool; i++)
            {
                if (carCount >= carsInPool) break;
                for (int j = 0; j < trafficPrefabs.Length; j++)
                {
                    if (carCount >= carsInPool) break;
                    GameObject spawnedTrafficVehicle = Instantiate(trafficPrefabs[j].gameObject, Vector3.zero, Quaternion.identity);
                    spawnedTrafficVehicle.GetComponent<AITrafficCar>().RegisterCar(carRouteList[0]);
                    MoveCarToPool(spawnedTrafficVehicle.GetComponent<AITrafficCar>().assignedIndex);
                }
            }
            for (int i = 0; i < carCount; i++)
            {
                routePointPositionNL[i] = carRouteList[i].waypointDataList[currentRoutePointIndexNL[i]]._transform.position;
                finalRoutePointPositionNL[i] = carRouteList[i].waypointDataList[carRouteList[i].waypointDataList.Count - 1]._transform.position;
                carList[i].StartDriving();
            }
            if (setCarParent)
            {
                if (carParent == null) carParent = transform;
                for (int i = 0; i < carCount; i++)
                {
                    carList[i].transform.SetParent(carParent);
                }
            }
            isInitialized = true;
        }

        public void EnableRegisteredTrafficEverywhere()
        {
            availableSpawnPoints.Clear();
            for (int i = 0; i < trafficSpawnPoints.Count; i++) // Get Available Spawn Points From All Zones
            {
                distanceToSpawnPoint = Vector3.Distance(centerPosition, trafficSpawnPoints[i].transformCached.position);
                if (trafficSpawnPoints[i].isTrigger == false)
                {
                    availableSpawnPoints.Add(trafficSpawnPoints[i]);
                }
            }
            for (int i = 0; i < density; i++) // Spawn Traffic
            {
                for (int j = 0; j < trafficPrefabs.Length; j++)
                {
                    if (availableSpawnPoints.Count == 0) break;
                    randomSpawnPointIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
                    spawnPosition = availableSpawnPoints[randomSpawnPointIndex].transformCached.position + spawnOffset;
                    for (int k = 0; k < availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.vehicleTypes.Length; k++)
                    {
                        if (availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.vehicleTypes[k] == trafficPrefabs[j].vehicleType)
                        {
                            spawncar = GetCarFromPool(availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute);
                            if (spawncar != null)
                            {
                                availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.currentDensity += 1;
                                spawnPosition = availableSpawnPoints[randomSpawnPointIndex].transformCached.position + spawnOffset;
                                spawncar.transform.SetPositionAndRotation(

                                    spawnPosition,
                                    availableSpawnPoints[randomSpawnPointIndex].transformCached.rotation
                                    );
                                spawncar.transform.LookAt(availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.waypointDataList[availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.waypointIndexnumber]._transform);
                                availableSpawnPoints.RemoveAt(randomSpawnPointIndex);
                            }
                            break;
                        }
                    }
                }
            }
        }
        #endregion

        #region Runtime API for Dynamic Content - Some Require Pooling
        /// <summary>
        /// Requires pooling, disables and moves all cars into the pool.
        /// </summary>
        public void DisableAllCars()
        {
            usePooling = false;
            for (int i = 0; i < carList.Count; i++)
            {
                MoveCarToPool(i);
                Set_CanProcess(i, false);
            }
        }

        /// <summary>
        /// Clears the spawn points list.
        /// </summary>
        public void RemoveSpawnPoints()
        {
            for (int i = trafficSpawnPoints.Count - 1; i < trafficSpawnPoints.Count - 1; i--)
            {
                trafficSpawnPoints[i].RemoveSpawnPoint();
            }
        }

        /// <summary>
        /// Clears the route list.
        /// </summary>
        public void RemoveRoutes()
        {
            for (int i = allWaypointRoutesList.Count - 1; i < allWaypointRoutesList.Count - 1; i--)
            {
                allWaypointRoutesList[i].RemoveRoute();
            }
        }

        /// <summary>
        /// Enables processing on all registered cars.
        /// </summary>
        public void EnableAllCars()
        {
            for (int i = 0; i < carList.Count; i++)
            {
                carList[i].EnableAIProcessing();
            }
            usePooling = true;
            EnableRegisteredTrafficEverywhere();
        }
        #endregion
    }
}