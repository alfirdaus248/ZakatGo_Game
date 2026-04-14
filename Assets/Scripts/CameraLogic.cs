using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    [Header("References")]
    public Transform Player;  // Player's transform
    public Transform ViewPoint;  // Camera's viewpoint (can be the camera's parent or follow target)

    [Header("Camera Settings")]
    public float mouseSensitivity = 2f;
    public float rotationSpeed = 5f;
    public float minYAngle = -35f;
    public float maxYAngle = 60f;
    public float smoothingSpeed = 10f;  // Smoothness of camera movement
    public float followDistance = 5f;  // Distance the camera follows the player
    private float currentSpeed = 0f;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        HandleCameraRotation();
        HandleCameraPosition();
    }

    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Vertical rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minYAngle, maxYAngle);
        ViewPoint.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal rotation (player turn)
        Player.Rotate(Vector3.up * mouseX);
    }

    private void HandleCameraPosition()
    {
        // Get the player's current velocity (how fast they're moving)
        currentSpeed = Player.GetComponent<Rigidbody>().velocity.magnitude;

        // Dynamically adjust camera distance based on player's speed
        float adjustedDistance = Mathf.Lerp(followDistance, followDistance * 1.2f, currentSpeed / 10f);

        // Calculate the target position behind the player, based on the adjusted distance
        Vector3 targetPosition = Player.position - Player.forward * adjustedDistance;

        // Smoothly move the camera towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothingSpeed * Time.deltaTime);

        // Update the camera's view direction to follow the player
        Vector3 viewDir = Player.position - new Vector3(transform.position.x, Player.position.y, transform.position.z);
        ViewPoint.forward = viewDir.normalized;
    }
}
