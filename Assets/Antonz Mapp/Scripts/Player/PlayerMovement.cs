using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement properties")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float airSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float additionalCrouchForce;
    [SerializeField] private float horizontalDrag;

    [Header("Spring physics")]
    [SerializeField] private float standingHeight = 1.5f;
    [SerializeField] private float crouchingHeight = .75f;
    [SerializeField] private float springStrength = 100.0f;
    [SerializeField] private float springDamping = 10.0f;
    [SerializeField, Tooltip("")] private float downForceDistance = .25f;
    [SerializeField] private float downForceResetTime;
    [SerializeField] private LayerMask rayCastIgnore;

    [Header("Camera")]
    [SerializeField] private Transform cameraTargetTransform;
    


    //Component references
    private Rigidbody _rigidbody;

    //Movement status
    private bool _isGrounded;
    
    //SpringPhysics
    private Vector3 _position;
    private float _distanceToGround;
    private Vector3 _neutralForce;
    private float _heightDifference;
    private float _springForce;
    private float _currentDownForceDistance;
    private float _downForceCounter;
    private Ray _ray;
    private RaycastHit _rayHit;
    
    //Lateral movement
    private Vector3 _movementDir;
    private Vector3 _velocity;
    private Vector3 _horizontalVelocity;

    //Jumping
    private bool _wantsToJump;
    private float _currentSpringHeightMultiplier;
    
    //Crouching
    private bool _wantsToCrouch;

    //Inputs
    private Vector3 _movementInput;
    private float _currentHeight;
    private float _currentSpeed;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        PlayerInput();
    }

    private void FixedUpdate()
    {
        SpringPhysics();
        LateralMovement();
        CustomDrag();
    }


    private void PlayerInput()
    {
        _movementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (_isGrounded && _currentHeight > crouchingHeight)
            {
                _wantsToCrouch = true;
            }
            _currentSpeed = crouchSpeed;
            _currentHeight = crouchingHeight;
        }
        else
        {
            _currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            _currentHeight = standingHeight;
        }

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _wantsToJump = true;
        }
    }


    private void SpringPhysics()
    {
        if (_wantsToJump)
        {
            _rigidbody.AddForce(_rigidbody.transform.up * (jumpForce - (_rigidbody.velocity.y * _rigidbody.mass)), ForceMode.Impulse);
            _wantsToJump = false;
            _downForceCounter = 0f;
        }
        if (_downForceCounter < downForceResetTime)
        {
            _downForceCounter += Time.fixedDeltaTime;
            
            _currentSpringHeightMultiplier = _downForceCounter / downForceResetTime;
        }
        else
        {
            _currentSpringHeightMultiplier = 1.0f;
        }

        if (_wantsToCrouch)
        {
            _rigidbody.AddForce(Vector3.down * (additionalCrouchForce + (_rigidbody.velocity.y * _rigidbody.mass)), ForceMode.Impulse);
            _wantsToCrouch = false;
        }

        _currentDownForceDistance = _currentSpringHeightMultiplier * downForceDistance;
        _currentHeight = _currentSpringHeightMultiplier * _currentHeight;
        
        _ray = new Ray(_position, Vector3.down);
        _position = _rigidbody.position;
        
        

        if (Physics.Raycast(_ray, out _rayHit, _currentHeight + _currentDownForceDistance, rayCastIgnore))
        {
            _neutralForce = _rigidbody.mass * Physics.gravity * 0.2f;

            _heightDifference = (_rayHit.point.y + _currentHeight) - _position.y;

            _springForce = _heightDifference * springStrength;

            _springForce -= _rigidbody.GetPointVelocity(_position).y * springDamping;
                
            _rigidbody.AddForceAtPosition(_neutralForce + _springForce * Vector3.up, _position);

            _isGrounded = true;

            if (_rayHit.collider.TryGetComponent(out Rigidbody rigBod))
            {
                rigBod.AddForceAtPosition(_rigidbody.velocity*_rigidbody.mass, _rayHit.point);
            }

            Debug.DrawLine(_position, _rayHit.point, Color.green);
        }
        else
        {
            _isGrounded = false;
            Debug.DrawRay(_position, Vector3.down * (_currentHeight +  _currentDownForceDistance), Color.red);
        }

    }


    private void LateralMovement()
    {
        if(_isGrounded)
        {
            _movementDir = (cameraTargetTransform.forward * _movementInput.z + cameraTargetTransform.right * _movementInput.x);

            _movementDir = Vector3.ProjectOnPlane(_movementDir, _rayHit.normal);
            
            _rigidbody.AddForce(_movementDir * _currentSpeed, ForceMode.Acceleration);
        }
        else
        {
            _movementDir = (cameraTargetTransform.forward * _movementInput.z + cameraTargetTransform.right * _movementInput.x);

            _rigidbody.AddForce(_movementDir * airSpeed, ForceMode.Force);
        }
        
        
        Debug.DrawRay(_rayHit.point + Vector3.up * .25f, _movementDir, Color.magenta);

    }
    
    private void CustomDrag()
    {
        if(!_isGrounded) return;
        
        _velocity = _rigidbody.velocity;
        _horizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);
        _rigidbody.AddForce(-_horizontalVelocity * horizontalDrag);
    }
}
