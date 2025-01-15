using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


/**
 * From Fusion tutorial https://doc.photonengine.com/fusion/current/tutorials/shared-mode-basics/3-movement-and-camera
 */

public class FirstPersonCamera : MonoBehaviour
{
    [SerializeField] public Transform target;
    [SerializeField] float mouseSensitivity = 10f;

    private float verticalRotation;
    private float horizontalRotation;

    // Adjust the position of the camera relative to the player
    [SerializeField] Vector3 cameraOffset = new Vector3(0, 1.8f, 0); // Adjust this as needed to get the desired camera height and distance from the player

    const string mouseXStr = "Mouse X";
    const string mouseYStr = "Mouse Y";

    [SerializeField] float minimumVerticalRotation = -70f;
    [SerializeField] float maximumVerticalRotation = 70f;
    [SerializeField] float zAxisRotation = 0;

    internal void SetTarget(Transform transform)
    {
        this.target = transform;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.position + cameraOffset;


        float mouseX = Input.GetAxis(mouseXStr);
        float mouseY = Input.GetAxis(mouseYStr);

        verticalRotation -= mouseY * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, minimumVerticalRotation, maximumVerticalRotation);

        horizontalRotation += mouseX * mouseSensitivity;

        transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, zAxisRotation);
    }
}