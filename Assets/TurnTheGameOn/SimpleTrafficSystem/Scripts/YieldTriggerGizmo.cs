using TurnTheGameOn.SimpleTrafficSystem;
using UnityEngine;

public class YieldTriggerGizmo : MonoBehaviour
{
    private BoxCollider m_collider;

    private void OnDrawGizmos() { DrawGizmos(false); }
    private void OnDrawGizmosSelected() { DrawGizmos(true); }

    private void DrawGizmos(bool selected)
    {
        if (m_collider == null)
        {
            m_collider = GetComponent<BoxCollider>();
        }
        if (m_collider != null)
        {
            if (selected)
            {
                Gizmos.color = STSPrefs.selectedYieldTriggerColor;
            }
            else
            {
                Gizmos.color = STSPrefs.yieldTriggerColor;
            }
            DrawCube
                (
                transform.position,
                transform.rotation,
                transform.localScale,
                m_collider.center,
                m_collider.size
                );
        }
    }

    void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 center, Vector3 size)
    {
        Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
        Gizmos.matrix *= cubeTransform;
        Gizmos.DrawCube(center, size);
        Gizmos.matrix = oldGizmosMatrix;
    }
}