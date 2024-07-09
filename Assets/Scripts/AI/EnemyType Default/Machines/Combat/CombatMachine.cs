using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
public class CombatMachine : StateMachine
{
    public SeekCoverState seekCoverState = new SeekCoverState();
    public FireState fireState = new FireState();
    public ReloadState reloadState = new ReloadState();
    public RepositionState repositionState = new RepositionState();
    public override void Awake(MachinePickerBehaviour _picker)
    {
        AvailableStates = new List<State>() {seekCoverState, fireState, reloadState, repositionState};
        _currentState = fireState;

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

        if (picker.blackboard.playerDetected)
        {
            if (!blackboard.confident && !blackboard.aggressive && !blackboard.panic)
            {
                blackboard.StopAllCoroutines();
                var nextMachine = picker.AvailableMachines.OfType<HidingMachine>().First();
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
    }
    
    public override void Transit(State targetState)
    {
        _currentState.Exit();
        _currentState = targetState;
        _currentState.Enter();
        Debug.Log(targetState);
    }
}
