using UnityEngine;

public class RotateOnlyX : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // Get current rotation
        Vector3 euler = transform.rotation.eulerAngles;

        // Keep only X, reset Y and Z
        transform.rotation = Quaternion.Euler(euler.x, 0f, 0f);
    }
}