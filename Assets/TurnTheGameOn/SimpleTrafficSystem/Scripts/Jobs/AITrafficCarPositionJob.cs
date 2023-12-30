namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using Unity.Collections;
    using Unity.Burst;
    using UnityEngine.Jobs;

    [BurstCompile]
    public struct AITrafficCarPositionJob : IJobParallelForTransform
    {
        public NativeArray<bool> canProcessNA;
        public NativeArray<Vector3> carTransformPositionNA;
        public NativeArray<Vector3> carTransformPreviousPositionNA;

        public void Execute(int index, TransformAccess carTransformAccessArray)
        {
            if (canProcessNA[index])
            {
                carTransformPreviousPositionNA[index] = carTransformPositionNA[index];
                carTransformPositionNA[index] = carTransformAccessArray.position;
            }
        }
    }
}