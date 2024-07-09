using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamBehaviour : MonoBehaviour
{
    [Header("Camera & mouse")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField, Range(0.0f, .1f)] private float mouseSmoothing = .1f;
    [SerializeField] private float cameraMinMaxPitch = 90f;
    [SerializeField] private Transform targetTransform;

    [Header("Resolution & FrameRate (TEMPORARY)")]
    [SerializeField] private Vector2Int resolution;
    [SerializeField] private int frameRate;
    
    
    //MouseLook
    private float _yaw, _pitch;
    private Vector3 _targetRotation;
    private Vector3 _smoothRotation;
    private Vector3 _finalRotation;
    private Vector3 _smoothingVelocity;
    

    private void Awake()
    {
        transform.parent = null;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Application.targetFrameRate = frameRate;
        Screen.SetResolution(resolution.x, resolution.y, FullScreenMode.MaximizedWindow);
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        
        transform.position = targetTransform.position;
        
        GetMouseInput();
        
        CameraYawPitch();
        
    }


    private void GetMouseInput()
    {
        _pitch = (_pitch - Input.GetAxis("Mouse Y") * mouseSensitivity);
        _yaw = (_yaw + Input.GetAxis("Mouse X") * mouseSensitivity);
        _pitch = Mathf.Clamp(_pitch, -cameraMinMaxPitch, cameraMinMaxPitch);
    }

    private void CameraYawPitch()
    {
        _targetRotation = new Vector3(_pitch, _yaw, 0);

        _smoothRotation = Vector3.SmoothDamp(_smoothRotation, _targetRotation, ref _smoothingVelocity, mouseSmoothing);

        _finalRotation = new Vector3(_smoothRotation.x % 360, _smoothRotation.y % 360, 0);

        var thisEuler = transform.eulerAngles = _finalRotation;
        targetTransform.eulerAngles = new Vector3(0, thisEuler.y, 0);
    }
}
