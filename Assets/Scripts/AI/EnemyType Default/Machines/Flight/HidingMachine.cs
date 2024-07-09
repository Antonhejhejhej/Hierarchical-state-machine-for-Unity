using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class HidingMachine : StateMachine
{
    public HideState hideState = new HideState();
    public override void Awake(MachinePickerBehaviour _picker)
    {
        AvailableStates = new List<State>() {hideState};
        _currentState = hideState;

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
        
        if (blackboard.shield == blackboard.fullShield && blackboard.timeSincePlayerHidden > 3f)
        {
            blackboard.StopAllCoroutines();
            var nextMachine = picker.AvailableMachines.OfType<CombatMachine>().First();
            picker.Transit(nextMachine);
        }else if (!blackboard.confident && blackboard.aggressive && blackboard.panic)
        {
            blackboard.StopAllCoroutines();
            var nextMachine = picker.AvailableMachines.OfType<PanicMachine>().First();
            picker.Transit(nextMachine);
        }else if (!blackboard.confident && !blackboard.aggressive && blackboard.panic)
        {
            blackboard.StopAllCoroutines();
            var nextMachine = picker.AvailableMachines.OfType<FlightMachine>().First();
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
