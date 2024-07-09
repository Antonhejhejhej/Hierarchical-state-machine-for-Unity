using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachinePickerBehaviour : MonoBehaviour
{
    [HideInInspector] public BlackboardBehaviour blackboard;
    
    private StateMachine currentMachine = null;

    public CombatMachine combatMachine = new CombatMachine();

    public PanicMachine panicMachine = new PanicMachine();

    public FlightMachine flightMachine = new FlightMachine();

    public HidingMachine hidingMachine = new HidingMachine();

    //public RestingMachine restingMachine = new RestingMachine();

    [HideInInspector] public List<StateMachine> AvailableMachines;


    private void Awake()
    {
        AvailableMachines = new List<StateMachine>() {combatMachine, panicMachine, flightMachine, hidingMachine};
        
        blackboard = GetComponent<BlackboardBehaviour>();




        foreach (var machine in AvailableMachines)
        {
            machine.Awake(this);
        }


    }

    void Start()
    {
        foreach (var machine in AvailableMachines)
        {
            machine.Start();
        }
        
        currentMachine = combatMachine;
        currentMachine.Enter();
    }

    
    void Update()
    {
        currentMachine.Update();
    }

    private void FixedUpdate()
    {
        currentMachine.FixedUpdate();
    }

    public void Transit(StateMachine targetMachine)
    {
        currentMachine.Exit();
        currentMachine = targetMachine;
        currentMachine.Enter();
    }
}
