using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]

public class FlightMachine : StateMachine
{
    public FleeingState fleeingState = new FleeingState();
    public override void Awake(MachinePickerBehaviour _picker)
    {
        AvailableStates = new List<State>() {fleeingState};
        _currentState = fleeingState;

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
        
        if (blackboard.timeSincePlayerHidden > 16f)
        {
            blackboard.StopAllCoroutines();
            var nextMachine = picker.AvailableMachines.OfType<CombatMachine>().First();
            picker.Transit(nextMachine);
        }
    }
    
    public override void Transit(State targetState)
    {
        _currentState.Exit();
        _currentState = targetState;
        _currentState.Enter();
        Debug.Log(targetState);
    }
}
