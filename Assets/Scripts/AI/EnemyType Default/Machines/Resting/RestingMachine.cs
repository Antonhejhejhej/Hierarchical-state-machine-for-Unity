using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RestingMachine : StateMachine
{
    public RestingState restingState = new RestingState();
    public override void Awake(MachinePickerBehaviour _picker)
    {
        AvailableStates = new List<State>() {restingState};
        _currentState = restingState;

        base.Awake(_picker);

        foreach (var state in AvailableStates)
        {
            state.Awake(this, _picker.blackboard);
        }
    }

    public override void Start()
    {
        foreach (var state in AvailableStates)
        {
            state.Start();
        }
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
        Debug.Log(targetState);
    }
}
