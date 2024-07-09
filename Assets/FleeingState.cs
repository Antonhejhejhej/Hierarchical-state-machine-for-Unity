using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]

public class FleeingState : State
{
    [SerializeField] float distanceFlee = 2000f;
    [SerializeField] bool activeState = false;
    NavMeshAgent agent;
    
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
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    // Update is called once per frame
    public override void Update()
    {
        if(Vector3.Distance(blackboard.transform.position, blackboard.lastKnownPlayerPos) < distanceFlee)
        {
            Debug.Log("Fleeing update");
            Vector3 direction = blackboard.transform.position - blackboard.lastKnownPlayerPos;
            Vector3 newPos = blackboard.transform.position + direction;
            agent.SetDestination(newPos);
        }
        else
        {
            Debug.Log("else");
            agent.SetDestination(blackboard.transform.position);
                
        }
    }


    
}
