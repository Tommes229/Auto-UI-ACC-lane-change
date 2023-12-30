namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using Unity.Collections;
    using Unity.Burst;
    using UnityEngine.Jobs;

    [BurstCompile]
    public struct AITrafficCarFrontSensorPositionJob : IJobParallelForTransform
    {
        public NativeArray<bool> canProcessNA;
        public NativeArray<Vector3> frontSensorTransformPositionNA;

        public void Execute(int index, TransformAccess frontSensorTransformAccessArray)
        {
            if (canProcessNA[index])
            {
                frontSensorTransformPositionNA[index] = frontSensorTransformAccessArray.position;
            }
        }
    }
}