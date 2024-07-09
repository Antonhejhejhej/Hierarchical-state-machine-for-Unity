using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class State
{
    [HideInInspector] public StateMachine machine = null;
    [HideInInspector] public BlackboardBehaviour blackboard = null;

    public virtual void Awake(StateMachine _machine, BlackboardBehaviour _blackboard)
    {
        machine = _machine;
        blackboard = _blackboard;
    }
    public virtual void Start(){}
    public virtual void Enter(){}
    public virtual void Exit(){}
    public virtual void Update(){}
    public virtual void FixedUpdate(){}
}
