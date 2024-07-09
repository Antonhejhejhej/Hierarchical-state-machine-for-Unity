using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable]

public class PursuingState : State
{
    [Header("Movement")]
    [SerializeField] private float repositionSpeed;

    [SerializeField] private float turningSmoothing;

    [SerializeField] private float lookForPositionInsideRadius;

    [SerializeField] private LayerMask obstacleLayer;



    private float _agentBaseSpeed;

    private bool positionFound;

    private bool transitionStarted;
    
    private Vector3 lookAtSmoothRef;

    private NavMeshAgent _navMeshAgent;
    public override void Awake(StateMachine _machine, BlackboardBehaviour _blackboard)
    {
        base.Awake(_machine, _blackboard);
    }

    public override void Start()
    {
        _navMeshAgent = blackboard.GetComponent<NavMeshAgent>();
        _agentBaseSpeed = _navMeshAgent.speed;
    }

    public override void Enter()
    {
        if (!blackboard.playerDetected)
        {
            _navMeshAgent.SetDestination(blackboard.transform.position);
            blackboard.StopAllCoroutines();
            var nextState = machine.AvailableStates.OfType<LookAroundState>().First();
            machine.Transit(nextState);
        }
        FindCombatPosition();
        transitionStarted = false;
    }

    public override void Exit()
    {
        _navMeshAgent.speed = _agentBaseSpeed;
    }

    public override void Update()
    {
        LookAtTarget();
    }

    public override void FixedUpdate()
    {
        if (positionFound && _navMeshAgent.remainingDistance < .5 || blackboard.playerInSight)
        {
            if(transitionStarted) return;
            transitionStarted = true;
            blackboard.StopAllCoroutines();
            blackboard.StartCoroutine(Transition());
        }
    }

    private void FindCombatPosition()
    {

        positionFound = false;
        while (!positionFound)
        {
            var rndPos = Random.insideUnitCircle * lookForPositionInsideRadius;

            NavMeshHit hit = new NavMeshHit();

            var path = new NavMeshPath();

            if (NavMesh.SamplePosition(blackboard.lastKnownPlayerPos + new Vector3(rndPos.x, 0f, rndPos.y), out hit, 5f,
                NavMesh.AllAreas) && Vector3.Distance(blackboard.transform.position, hit.position) > 5f && Vector3.Distance(blackboard.lastKnownPlayerPos, hit.position) > 5f)
            {
                _navMeshAgent.CalculatePath(hit.position, path);

                var ray = new Ray(hit.position, blackboard.lastKnownPlayerPos - hit.position);

                if (path.status == NavMeshPathStatus.PathComplete && !Physics.Raycast(ray,
                    Vector3.Distance(blackboard.lastKnownPlayerPos, hit.position), obstacleLayer))
                {
                    Debug.Log("Found Tactical Pos");
                    _navMeshAgent.SetDestination(hit.position);
                    positionFound = true;
                }
            }
            
        }
        
    }
    
    private void LookAtTarget()
    {
        var lookAtTarget = (new Vector3(blackboard.lastKnownPlayerPos.x, blackboard.bodyTransform.position.y, blackboard.lastKnownPlayerPos.z) - blackboard.bodyTransform.position);

        var smoothLookAt = Vector3.SmoothDamp(blackboard.bodyTransform.forward, lookAtTarget, ref lookAtSmoothRef, turningSmoothing);



        blackboard.bodyTransform.forward = smoothLookAt;

    }

    private IEnumerator Transition()
    {
        yield return new WaitForSeconds(.5f);
        
        _navMeshAgent.SetDestination(blackboard.transform.position);
        blackboard.StopAllCoroutines();
        var nextState = machine.AvailableStates.OfType<LookAroundState>().First();
        machine.Transit(nextState);
        
        yield break;
    }
}
