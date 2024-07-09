using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable]
public class LookAroundState : State
{
    [Header("Settings")]
    [SerializeField] private int numOfTurns;
    [SerializeField] private float timeBetweenTurns;
    [SerializeField] private float turningSmoothing;
    
    [SerializeField] Vector3 restPosition;
    [SerializeField] bool activeState = false;
    private NavMeshAgent agent;
    
    private Vector3 lookAtSmoothRef;

    private Vector2 rndDirection;

    private bool isLooking;
    
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
        isLooking = true;
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
        LookAtRandom();
    }
    
    public void Rest()
    {
        agent.SetDestination(restPosition);
        blackboard.StartCoroutine(RandomLooking());

        
    }
    
    private void LookAtRandom()
    {
        
        
        var lookAtTarget = (new Vector3(rndDirection.x, blackboard.transform.position.y,rndDirection.y).normalized - blackboard.bodyTransform.position);

        var smoothLookAt = Vector3.SmoothDamp(blackboard.bodyTransform.forward, lookAtTarget, ref lookAtSmoothRef, turningSmoothing);



        blackboard.bodyTransform.forward = smoothLookAt;

    }

    private IEnumerator RandomLooking()
    {
        for (int i = 0; i < numOfTurns; i++)
        {
            yield return new WaitForSeconds(timeBetweenTurns);

            
            rndDirection = Random.insideUnitCircle;
            /*var oldDir = rndDirection;
            while (Vector3.Dot(oldDir, rndDirection) < 0.5f)
            {
                rndDirection = Random.insideUnitCircle;
            }*/
            
        }
        
        isLooking = false;

        agent.SetDestination(blackboard.transform.position);
        blackboard.StopAllCoroutines();
        var nextState = machine.AvailableStates.OfType<PursuingState>().First();
        machine.Transit(nextState);
        
        
        yield break;
    }
}
