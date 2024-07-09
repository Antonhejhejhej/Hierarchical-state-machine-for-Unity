using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    [SerializeField] private List<BlackboardBehaviour> entityList = new List<BlackboardBehaviour>();

    private bool _allEntitiesActive;
   

    private void OnEnable()
    {
        EventHandler.RegisterEntityEvent += OnRegisterEntityEvent;
    }

    private void OnDisable()
    {
        EventHandler.RegisterEntityEvent -= OnRegisterEntityEvent;
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    private void OnRegisterEntityEvent(BlackboardBehaviour blackboard)
    {
        Debug.Log("EntityAdded");

        entityList.Add(blackboard);

    }
}
