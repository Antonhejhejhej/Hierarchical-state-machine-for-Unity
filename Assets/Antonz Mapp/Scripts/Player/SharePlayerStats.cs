using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharePlayerStats : MonoBehaviour
{

    private Rigidbody _rigidbody;
    [SerializeField] private Transform cameraTarget;

    [SerializeField] private PlayerStats playerStats;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        playerStats.playerPos = Vector3.zero;
        playerStats.playerLookDirection = Vector3.zero;
        playerStats.playerMovementDirection = Vector3.zero;
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        playerStats.playerPos = transform.position;
        playerStats.playerLookDirection = cameraTarget.forward;
        var velocity = _rigidbody.velocity;
        playerStats.playerMovementDirection = new Vector3(velocity.x, 0, velocity.z).normalized;
    }
}
