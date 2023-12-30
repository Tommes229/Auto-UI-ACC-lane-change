namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using Unity.Mathematics;
    using Unity.Collections;
    using Unity.Burst;
    using UnityEngine.Jobs;

    [BurstCompile]
    public struct AITrafficCarJob : IJobParallelForTransform
    {
        public float deltaTime;
        public float maxSteerAngle;
        public float speedMultiplier;
        public float steerSensitivity;
        public float stopThreshold;
        public NativeArray<float> frontSensorLengthNA;
        public NativeArray<int> currentRoutePointIndexNA;
        public NativeArray<int> waypointDataListCountNA;
        public NativeArray<bool> isDrivingNA;
        public NativeArray<bool> isActiveNA;
        public NativeArray<bool> canProcessNA;
        public NativeArray<bool> overrideInputNA;
        public NativeArray<bool> isBrakingNA;
        public NativeArray<bool> frontHitNA;
        public NativeArray<bool> stopForTrafficLightNA;
        public NativeArray<bool> yieldForCrossTrafficNA;
        public NativeArray<float> accelerationPowerNA;
        public NativeArray<float> accelerationInputNA;
        public NativeArray<float> speedNA;
        public NativeArray<float> topSpeedNA;
        public NativeArray<float> routeProgressNA;
        public NativeArray<float> targetSpeedNA;
        public NativeArray<float> speedLimitNA;
        public NativeArray<float> accelNA;
        public NativeArray<float> targetAngleNA;
        public NativeArray<float> steerAngleNA;
        public NativeArray<float> motorTorqueNA;
        public NativeArray<float> brakeTorqueNA;
        public NativeArray<float> moveHandBrakeNA;
        public NativeArray<float> overrideAccelerationPowerNA;
        public NativeArray<float> overrideBrakePowerNA;
        public NativeArray<float> frontHitDistanceNA;
        public NativeArray<float> distanceToEndPointNA;
        public NativeArray<float3> finalRoutePointPositionNA;
        public NativeArray<float3> routePointPositionNA;
        public NativeArray<Vector3> carTransformPreviousPositionNA;
        public NativeArray<Vector3> carTransformPositionNA;
        public NativeArray<Vector3> localTargetNA;

        public NativeArray<Vector3> frontSensorTransformPositionNA;

        public void Execute(int index, TransformAccess driveTargetTransformAccessArray)
        {
            if (isActiveNA[index] && canProcessNA[index])
            {
                driveTargetTransformAccessArray.position = routePointPositionNA[index];

                #region StopThreshold
                if (stopForTrafficLightNA[index] && routeProgressNA[index] > 0 && currentRoutePointIndexNA[index] >= waypointDataListCountNA[index] - 1)
                {
                    //distanceToEndPointNA[index] = Vector3.Distance(frontSensorTransformPositionNA[index], routePointPositionNA[index]);
                    distanceToEndPointNA[index] = Vector3.Distance(frontSensorTransformPositionNA[index], finalRoutePointPositionNA[index]);
                    //if (overrideInputNA[index])
                    //{
                    overrideInputNA[index] = true;
                    overrideBrakePowerNA[index] = 1f;
                    overrideAccelerationPowerNA[index] = 0f;
                    //}
                }
                else if (stopForTrafficLightNA[index] && routeProgressNA[index] > 0 && currentRoutePointIndexNA[index] >= waypointDataListCountNA[index] - 2 && !frontHitNA[index])
                {
                    //distanceToEndPointNA[index] = Vector3.Distance(frontSensorTransformPositionNA[index], routePointPositionNA[index]);
                    distanceToEndPointNA[index] = Vector3.Distance(frontSensorTransformPositionNA[index], finalRoutePointPositionNA[index]);
                    //if (overrideInputNA[index])
                    //{
                    overrideInputNA[index] = true;
                    overrideBrakePowerNA[index] = distanceToEndPointNA[index] < 3 || speedNA[index] > 10 ? 1f : 0f;
                    overrideAccelerationPowerNA[index] = distanceToEndPointNA[index] < 3 || speedNA[index] > 10 ? 0f : 0.3f;
                    //}
                }
                else if (frontHitNA[index] && frontHitDistanceNA[index] < stopThreshold)
                {
                    if (!overrideInputNA[index])
                    {
                        overrideInputNA[index] = true;
                        overrideBrakePowerNA[index] = 1f;
                        overrideAccelerationPowerNA[index] = 0f;
                    }
                }
                else if (yieldForCrossTrafficNA[index])
                {
                    if (!overrideInputNA[index])
                    {
                        overrideInputNA[index] = true;
                        overrideBrakePowerNA[index] = 1f;
                        overrideAccelerationPowerNA[index] = 0f;
                    }
                }
                else if (overrideInputNA[index])
                {
                    overrideBrakePowerNA[index] = 0f;
                    overrideAccelerationPowerNA[index] = 0f;
                    overrideInputNA[index] = false;
                }
                #endregion

                #region move
                if (isDrivingNA[index])
                {
                    targetSpeedNA[index] = topSpeedNA[index];
                    if (targetSpeedNA[index] > speedLimitNA[index]) targetSpeedNA[index] = speedLimitNA[index];
                    if (frontHitNA[index]) targetSpeedNA[index] = Mathf.InverseLerp(0, frontSensorLengthNA[index], frontHitDistanceNA[index]) * targetSpeedNA[index];
                    accelNA[index] = targetSpeedNA[index] - speedNA[index];
                    localTargetNA[index] = driveTargetTransformAccessArray.localPosition;
                    targetAngleNA[index] = math.atan2(localTargetNA[index].x, localTargetNA[index].z) * 52.29578f;
                    steerAngleNA[index] = math.clamp(targetAngleNA[index] * steerSensitivity, -1, 1) * math.sign(speedNA[index]);
                    steerAngleNA[index] *= maxSteerAngle;
                    if (speedNA[index] > topSpeedNA[index] || speedNA[index] > speedLimitNA[index])
                    {
                        motorTorqueNA[index] = 0;
                        accelerationInputNA[index] = 0;
                        overrideInputNA[index] = true;
                        overrideBrakePowerNA[index] = 0.5f;
                        overrideAccelerationPowerNA[index] = 0f;
                    }
                    else
                    {
                        accelerationInputNA[index] = math.clamp(accelNA[index], 0, 1);
                        motorTorqueNA[index] = accelerationInputNA[index] * accelerationPowerNA[index];
                    }
                    brakeTorqueNA[index] = (-1 * math.clamp(accelNA[index], -1, 0));
                    moveHandBrakeNA[index] = 0;
                }
                else
                {
                    if (speedNA[index] > 2)
                    {
                        localTargetNA[index] = driveTargetTransformAccessArray.localPosition;
                        targetAngleNA[index] = math.atan2(localTargetNA[index].x, localTargetNA[index].z) * 52.29578f;
                        steerAngleNA[index] = math.clamp(targetAngleNA[index] * steerSensitivity, -1, 1) * math.sign(speedNA[index]);
                        steerAngleNA[index] *= maxSteerAngle;
                        accelerationInputNA[index] = 0;
                        motorTorqueNA[index] = 0;
                        brakeTorqueNA[index] = -1;
                        moveHandBrakeNA[index] = 1;
                    }
                    else
                    {
                        steerAngleNA[index] = 0;
                        accelerationInputNA[index] = 0;
                        motorTorqueNA[index] = 0;
                        brakeTorqueNA[index] = -1;
                        moveHandBrakeNA[index] = 1;
                    }
                }

                if (overrideInputNA[index])
                {
                    accelerationInputNA[index] = overrideAccelerationPowerNA[index];
                    motorTorqueNA[index] = overrideAccelerationPowerNA[index] * accelerationPowerNA[index];
                    brakeTorqueNA[index] = overrideBrakePowerNA[index];
                    isBrakingNA[index] = true;
                }
                else if (brakeTorqueNA[index] > 0.0f) isBrakingNA[index] = true;
                else if (brakeTorqueNA[index] == 0.0f) isBrakingNA[index] = false;

                speedNA[index] = ((carTransformPositionNA[index] - carTransformPreviousPositionNA[index]).magnitude / deltaTime) * speedMultiplier;
                #endregion
            }
        }
    }
}