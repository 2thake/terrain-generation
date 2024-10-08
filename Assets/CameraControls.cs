using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [Header("Camera Controls")]
    [SerializeField] private float rotateSpeed = 10;
    [SerializeField] private float zoomSpeed = 10;

    [SerializeField] private Camera mainCamera;

    void Update() {
        transform.rotation *= Quaternion.Euler(0, rotateSpeed * Time.deltaTime, 0);
    }
}
