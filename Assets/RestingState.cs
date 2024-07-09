using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]

public class RestingState : State
{
    [SerializeField] Vector3 restPosition;
    [SerializeField] bool activeState = false;
    private NavMeshAgent agent;
    public override void Awake(StateMachine _machine, BlackboardBehaviour _blackboard)
    {
        base.Awake(_machine, _blackboard);
        
        
    }

    public override void Start()
    {
        agent = blackboard.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        restPosition = blackboard.transform.position;
        Rest();
    }

    public override void Exit()
    {
        base.Exit();
    }

    // Update is called once per frame
    public override void FixedUpdate()
    {
        
    }
    
    public void Rest()
    {
        agent.SetDestination(restPosition);
    }
}
