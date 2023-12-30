namespace TurnTheGameOn.SimpleTrafficSystem
{
    using Unity.Mathematics;
    using Unity.Collections;
    using Unity.Burst;
    using UnityEngine.Jobs;

    [BurstCompile]
    public struct AITrafficCarWheelJob : IJobParallelForTransform
    {
        public NativeArray<bool> canProcessNA;
        public NativeArray<float3> wheelPositionNA;
        public NativeArray<quaternion> wheelQuaternionNA;
        public NativeArray<float> speedNA;

        public void Execute(int index, TransformAccess carWheelTransform)
        {
            if (canProcessNA[index])
            {
                carWheelTransform.position = wheelPositionNA[index];
                if (speedNA[index] > 0.5f)
                {
                    carWheelTransform.rotation = wheelQuaternionNA[index];
                }
            }
        }
    }
}