using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The target that the camera will follow (e.g., the player)
    public float smoothSpeed = 0.125f; // The speed with which the camera will follow
    public Vector3 offset; // Offset from the target position
    private Vector3 correctedOffset;

    void FixedUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        if (target.localScale.x < 0)
        {
            correctedOffset = new Vector3(offset.x * target.localScale.x, offset.y * target.localScale.y, offset.z);
            desiredPosition = target.position + correctedOffset;
        }
        // Calculate the desired position

        // Smoothly interpolate between the current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Set the camera's position to the smoothed position
        transform.position = smoothedPosition;
    }
}

