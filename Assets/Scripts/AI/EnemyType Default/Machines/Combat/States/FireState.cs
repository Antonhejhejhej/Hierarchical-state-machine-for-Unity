using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


[System.Serializable]

public class FireState : State
{

    [Header("Weapon")]

    [SerializeField, Tooltip("Shots per second")] private float fireRate;
    [SerializeField] private int magazineCount;
    [SerializeField] private int maxBurstCount;
    [SerializeField] private float maxIdleTime;
    [SerializeField] private float minIdleTime;

    [SerializeField] private GameObject projectilePrefab;

    [Header("Movement")]

    [SerializeField] private float turningSmoothing;

    [SerializeField] private float strafeSpeed;

    [SerializeField] private float strafeAcceleration;

    [SerializeField] private float strafeStepSize;

    [SerializeField, Range(0f, 100f)] private float chanceOfStrafe;


    //Navigation

    private NavMeshAgent navMeshAgent;
    private Vector3 strafeTargetPos;

    private float agentBaseSpeed;

    private float agentBaseAcceleration;


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
        agentBaseSpeed = navMeshAgent.speed;
        agentBaseAcceleration = navMeshAgent.acceleration;
        if (maxBurstCount > magazineCount) maxBurstCount = magazineCount;
    }

    

    public override void Update()
    {
        LookAtTarget();

        
    }

    public override void FixedUpdate()
    {
        if (blackboard.playerInSight && !isShooting && projectileCount > 0 && blackboard.combatRange != CombatRange.outOfRange)
        {
            blackboard.StartCoroutine(FireWeapon());
        }
        
        

        if (projectileCount <= 0 || blackboard.shield <= 0)
        {
            if (blackboard.playerInSight)
            {
                blackboard.StopAllCoroutines();
                var nextState = machine.AvailableStates.OfType<SeekCoverState>().First();
                machine.Transit(nextState);
            }
            else
            {
                blackboard.StopAllCoroutines();
                var nextState = machine.AvailableStates.OfType<ReloadState>().First();
                machine.Transit(nextState);
            }
            
        }else if (blackboard.timeSincePlayerHidden > 4)
        {
            blackboard.StopAllCoroutines();
            var nextState = machine.AvailableStates.OfType<RepositionState>().First();
            machine.Transit(nextState);
        }
        
        if (blackboard.playerInSight && blackboard.combatRange == CombatRange.outOfRange)
        {
            blackboard.StopAllCoroutines();
            var nextState = machine.AvailableStates.OfType<RepositionState>().First();
            machine.Transit(nextState);
        }

        
    }

    public override void Enter()
    {
        navMeshAgent.SetDestination(blackboard.transform.position);
        projectileCount = magazineCount;
    }

    public override void Exit()
    {
        isShooting = false;
        navMeshAgent.SetDestination(blackboard.transform.position);
        navMeshAgent.speed = agentBaseSpeed;
        navMeshAgent.acceleration = agentBaseAcceleration;
    }

    private void LookAtTarget()
    {
        var lookAtTarget = (new Vector3(blackboard.lastKnownPlayerPos.x, blackboard.bodyTransform.position.y, blackboard.lastKnownPlayerPos.z) - blackboard.bodyTransform.position);

        var smoothLookAt = Vector3.SmoothDamp(blackboard.bodyTransform.forward, lookAtTarget, ref lookAtSmoothRef, turningSmoothing);



        blackboard.bodyTransform.forward = smoothLookAt;

    }

    private void Shoot()
    {
        //Debug.Log("shoot");
        if (projectileCount > 0)
        {
            projectileCount--;
        }

        var rndX = Random.Range(-.25f, .25f);
        var rndY = Random.Range(-.25f, .25f);
        var rndZ = Random.Range(-.25f, .25f);

        var targetDir = (blackboard.lastKnownPlayerPos - blackboard.projectileOrigin.position + new Vector3(rndX, rndY, rndZ)).normalized;

        var projectileInstance = Object.Instantiate(projectilePrefab);
        projectileInstance.transform.position = blackboard.projectileOrigin.position;
        projectileInstance.GetComponent<ProjectileBehaviour>().LaunchProjectile(targetDir);
    }

    private bool GetStrafePosition()
    {
        var rndSign = Random.Range(-10,10);
        var planeNormal = blackboard.eyeTransform.up;
        if(rndSign > 0)
        {
            planeNormal *= -1;
        }

        var strafeDirection = Vector3.Cross((blackboard.lastKnownPlayerPos - blackboard.projectileOrigin.position).normalized, planeNormal).normalized;

        if (blackboard.combatRange == CombatRange.melee)
        {
            strafeDirection = (strafeDirection + blackboard.transform.forward).normalized;
        }

        NavMeshHit navHit;

        if(NavMesh.SamplePosition(blackboard.transform.position + strafeDirection * strafeStepSize, out navHit, 1f, NavMesh.AllAreas))
        {
            strafeTargetPos = navHit.position;

            return true;

        }else
        {
            planeNormal *= -1;

            strafeDirection = Vector3.Cross((blackboard.lastKnownPlayerPos - blackboard.projectileOrigin.position).normalized, planeNormal).normalized;

            if(NavMesh.SamplePosition(blackboard.transform.position + strafeDirection * strafeStepSize, out navHit, 1f, NavMesh.AllAreas))
            {
                strafeTargetPos = navHit.position;
                
                return true;
            }
        }

        return false;


    }

    private IEnumerator FireWeapon()
    {

        isShooting = true;
        while(projectileCount > 0)
        {
            burstCount = Random.Range(1, maxBurstCount);
            if (burstCount > projectileCount) burstCount = projectileCount;

            for (int i = 0; i < burstCount; i++)
            {
                if (blackboard.playerInSight)
                {
                    Shoot();
                }
                yield return new WaitForSeconds(timeBetweenShots);
            }

            if (Random.Range(0, 100) <= chanceOfStrafe)
            {
                if (GetStrafePosition())
                {

                    

                    navMeshAgent.speed = strafeSpeed;
                    navMeshAgent.acceleration = strafeAcceleration;

                    navMeshAgent.SetDestination(strafeTargetPos);

                    var strafeTimer = 0f;

                    while (true)
                    {
                        yield return new WaitForEndOfFrame();
                        strafeTimer += Time.deltaTime;

                        if(strafeTimer > 2f)
                        {
                            navMeshAgent.speed = agentBaseSpeed;
                            navMeshAgent.acceleration = agentBaseAcceleration;
                            break;
                        }

                        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                        {
                            if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                            {
                                navMeshAgent.speed = agentBaseSpeed;
                                navMeshAgent.acceleration = agentBaseAcceleration;
                                break;
                            }
                        }
                    }

                }
                else
                {
                    var rndWait = Random.Range(minIdleTime, maxIdleTime);

                    yield return new WaitForSeconds(rndWait);
                }

            }
            else
            {
                var rndWait = Random.Range(minIdleTime, maxIdleTime);

                yield return new WaitForSeconds(rndWait);
            }




        }

        isShooting = false;

        yield break;
    }

    
}
