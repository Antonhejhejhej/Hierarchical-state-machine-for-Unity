using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    [HideInInspector] public MachinePickerBehaviour picker = null;
    [HideInInspector] public BlackboardBehaviour blackboard = null;
    
    [HideInInspector] public State _currentState = null;

    [HideInInspector] public List<State> AvailableStates;

    public virtual void Awake(MachinePickerBehaviour _picker)
    {
        picker = _picker;
        blackboard = picker.blackboard;
        //kör alla awake(this, picker.blackboard)
    }

    public virtual void Start()
    {
        //kör alla Start
    }

    public virtual void Enter()
    {
        _currentState.Enter();
    }
    public virtual void Exit(){}

    public virtual void Update()
    {
        _currentState.Update();
    }

    public virtual void FixedUpdate()
    {
        _currentState.FixedUpdate();
    }
    
    public virtual void Transit(State targetState)
    {
        _currentState.Exit();
        _currentState = targetState;
        _currentState.Enter();
    }
}
