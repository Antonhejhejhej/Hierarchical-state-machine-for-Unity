using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]

public class ReloadState : State
{

    [Header("Settings")]
    [SerializeField] private float reloadTime;
    [SerializeField] private float turningSmoothing;
    
    [SerializeField] Vector3 restPosition;
    [SerializeField] bool activeState = false;
    private NavMeshAgent agent;
    
    private Vector3 lookAtSmoothRef;

    private bool isReloading;
    
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
        isReloading = true;
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
        LookAtTarget();
    }
    
    public void Rest()
    {
        agent.SetDestination(restPosition);
        blackboard.StartCoroutine(ReloadWeapon());

        
    }
    
    private void LookAtTarget()
    {
        var lookAtTarget = (new Vector3(blackboard.lastKnownPlayerPos.x, blackboard.bodyTransform.position.y, blackboard.lastKnownPlayerPos.z) - blackboard.bodyTransform.position);

        var smoothLookAt = Vector3.SmoothDamp(blackboard.bodyTransform.forward, lookAtTarget, ref lookAtSmoothRef, turningSmoothing);



        blackboard.bodyTransform.forward = smoothLookAt;

    }

    private IEnumerator ReloadWeapon()
    {
        yield return new WaitForSeconds(reloadTime);
        isReloading = false;

        if (blackboard.playerInSight && blackboard.shield > 0)
        {
            blackboard.StopAllCoroutines();
            var nextState = machine.AvailableStates.OfType<FireState>().First();
            machine.Transit(nextState);
        }
        else
        {
            if (blackboard.shield > 0)
            {
                blackboard.StopAllCoroutines();
                var nextState = machine.AvailableStates.OfType<RepositionState>().First();
                machine.Transit(nextState);
            }
            else
            {
                blackboard.StopAllCoroutines();
                var nextState = machine.AvailableStates.OfType<SeekCoverState>().First();
                machine.Transit(nextState);
            }
            
        }
        
        
        yield break;
    }
}
