using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StateMachineTester : StateMachine
{

    public HideState hideState = new HideState();
    public FireState fireState = new FireState();
    public override void Awake(MachinePickerBehaviour _picker)
    {
        _currentState = hideState;

        base.Awake(_picker);
        
        hideState.Awake(this, _picker.blackboard);
        fireState.Awake(this, _picker.blackboard);
    }

    public override void Start()
    {
        hideState.Start();
    }

    public virtual void Enter()
    {
        _currentState.Enter();
    }
    public virtual void Exit(){}

    public override void Update()
    {
        _currentState.Update();
    }

    public override void FixedUpdate()
    {
        _currentState.FixedUpdate();
    }
    
    public override void Transit(State targetState)
    {
        _currentState.Exit();
        _currentState = targetState;
        _currentState.Enter();
    }
}
