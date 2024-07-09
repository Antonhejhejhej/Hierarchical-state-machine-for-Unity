using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[System.Serializable]

public class BerserkState : State
{
    [SerializeField] float distanceFromTargetStop = 2f;
    [SerializeField] private bool activeState = true;
    NavMeshAgent navMeshAgent;
    float stoppingDist;
    
    
    [Header("Weapon")]

    [SerializeField, Tooltip("Shots per second")] private float fireRate;
    [SerializeField] private int magazineCount;
    [SerializeField] private int maxBurstCount;
    [SerializeField] private float maxIdleTime;
    [SerializeField] private float minIdleTime;

    [SerializeField] private GameObject projectilePrefab;


    //Firing
    private float timeBetweenShots;
    private int burstCount;
    private int projectileCount;
    private float idleTime;
    private float idleCounter;
    private bool isShooting;

    private Coroutine fireWeaponCoroutine;
    

    //Look At Target
    private Vector3 lookAtSmoothRef;

    public override void Awake(StateMachine _machine, BlackboardBehaviour _blackboard)
    {
        base.Awake(_machine, _blackboard);
    }

    public override void Start()
    {
        navMeshAgent = blackboard.GetComponent<NavMeshAgent>();
        timeBetweenShots = 1 / fireRate;
        projectileCount = magazineCount;
        if (maxBurstCount > magazineCount) maxBurstCount = magazineCount;
    }

    // Update is called once per frame
    public override void Update()
    {
        Debug.Log("hello");
        navMeshAgent.SetDestination(blackboard.lastKnownPlayerPos);
        if (blackboard.combatRange != CombatRange.outOfRange)
        {
            if (blackboard.playerInSight && !isShooting && projectileCount > 0)
            {
                Debug.Log("FIRE");
                blackboard.StopAllCoroutines();
                blackboard.StartCoroutine(FireWeapon());
            }

            if (isShooting && !blackboard.playerInSight)
            {
                blackboard.StopAllCoroutines();
                isShooting = false;
            }
        }
        
    }

    public override void Enter()
    {
        blackboard.bodyTransform.forward = navMeshAgent.transform.forward;
        stoppingDist = navMeshAgent.stoppingDistance;
        navMeshAgent.stoppingDistance = distanceFromTargetStop;
        isShooting = false;
    }

    public override void Exit()
    {
        navMeshAgent.stoppingDistance = stoppingDist;
    }
    
    private void Shoot()
    {
        //Debug.Log("shoot");
        if (projectileCount > 0)
        {
            projectileCount--;
        }
        

        var rndX = Random.Range(-2f, 2f);
        var rndY = Random.Range(-2f, 2f);
        var rndZ = Random.Range(-2f, 2f);

        var targetDir = (blackboard.lastKnownPlayerPos - blackboard.projectileOrigin.position + new Vector3(rndX, rndY, rndZ)).normalized;

        var projectileInstance = Object.Instantiate(projectilePrefab);
        projectileInstance.transform.position = blackboard.projectileOrigin.position;
        projectileInstance.GetComponent<ProjectileBehaviour>().LaunchProjectile(targetDir);
    }


    private IEnumerator FireWeapon()
    {
        Debug.Log("fire");

        isShooting = true;
        while(projectileCount > 0)
        {
            burstCount = Random.Range(1, maxBurstCount);
            if (burstCount > projectileCount) burstCount = projectileCount;

            for (int i = 0; i < burstCount; i++)
            {
                Shoot();
                yield return new WaitForSeconds(timeBetweenShots);
            }

            var rndWait = Random.Range(minIdleTime, maxIdleTime);

            yield return new WaitForSeconds(rndWait);

        }

        isShooting = false;

        yield break;
    }
}