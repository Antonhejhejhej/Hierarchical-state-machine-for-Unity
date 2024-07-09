using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : MonoBehaviour
{

    [Header("Navigation")]
    
    [SerializeField, Range(2, 5)] private int numOfWaypoints;

    [SerializeField] private float maxWaypointDistance;
    
    

    private NavMeshAgent _navMeshAgent;
    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
