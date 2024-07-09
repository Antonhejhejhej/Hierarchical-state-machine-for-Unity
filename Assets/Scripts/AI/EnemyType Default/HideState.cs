using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[System.Serializable]
[RequireComponent(typeof(NavMeshAgent))]
public class HideState : State
{

    bool activeState = false;

    
    public LayerMask HidableLayers;
    private NavMeshAgent Agent;
    [Range(-1, 1)]
    [Tooltip("Lower is a better hiding spot")]
    public float HideSensitivity = 0;
    [Range(1, 30)]
    public float MinPlayerDistance = 5f;
    [Range(0, 5f)]
    public float MinObstacleHeight = 1.25f;
    [Range(0.01f, 1f)]
    public float UpdateFrequency = 0.25f;

    [SerializeField] private float hideSpeed;

    private Coroutine MovementCoroutine;
    private Collider[] Colliders = new Collider[10]; // more is less performant, but more options

    

    private bool _localInsight;
    private float _agentBaseSpeed;
    
    
    //LOOKAT
    private Vector3 _hidingLookNormal;

    [SerializeField] private float turningSmoothing;
    private Vector3 lookAtSmoothRef;

    public override void Awake(StateMachine _machine, BlackboardBehaviour _blackboard)
    {
        base.Awake(_machine, _blackboard);
        
        
    }

    public override void Start()
    {
        Agent = blackboard.GetComponent<NavMeshAgent>();
        _agentBaseSpeed = Agent.speed;
    }

    public override void Update()
    {
        LookAtTarget();
    }

    public override void FixedUpdate()
    {
        if (!_localInsight && blackboard.playerInSight)
        {
            if (MovementCoroutine != null)
            {
                blackboard.StopCoroutine(MovementCoroutine);
            }

            blackboard.StartCoroutine(Hide(blackboard.lastKnownPlayerPos));
            
            
            

            _localInsight = true;
        }else if (_localInsight && !blackboard.playerInSight)
        {
            if (MovementCoroutine != null)
            {
                blackboard.StopCoroutine(MovementCoroutine);
                Debug.Log("STOP HIDE");
            }

            _localInsight = false;
        }

        if (!_localInsight && Agent.remainingDistance < 1f)
        {
            blackboard.StopAllCoroutines();
            var nextState = machine.AvailableStates.OfType<HideState>().First();
            machine.Transit(nextState);
        }
        
        
    }
    
    private void LookAtTarget()
    {
        var lookAtTarget = (new Vector3(blackboard.lastKnownPlayerPos.x, blackboard.bodyTransform.position.y, blackboard.lastKnownPlayerPos.z) - blackboard.bodyTransform.position);

        if (!_localInsight && blackboard.playerDetected)
        {
            var rndSign = Random.Range(-10,10);
            var planeNormal = blackboard.eyeTransform.up;
            if(rndSign > 0)
            {
                planeNormal *= -1;
            }

            var lookAngle = Vector3.Cross((_hidingLookNormal).normalized, planeNormal).normalized;

            lookAngle.y = blackboard.eyeTransform.position.y;

            lookAtTarget = (lookAngle - blackboard.transform.position).normalized * 10f;
        }

        var smoothLookAt = Vector3.SmoothDamp(blackboard.bodyTransform.forward, lookAtTarget, ref lookAtSmoothRef, turningSmoothing);



        blackboard.bodyTransform.forward = smoothLookAt;

    }

    

    /*private void HandleGainSight(Transform Target)
    {
        if (MovementCoroutine != null)
        {
            StopCoroutine(MovementCoroutine);
        }
        MovementCoroutine = StartCoroutine(Hide(Target));
    }

    private void HandleLoseSight(Transform Target)
    {
        if (MovementCoroutine != null)
        {
            StopCoroutine(MovementCoroutine);
        }
        
    }*/
    public override void Exit()
    {
        if (MovementCoroutine != null)
        {
            blackboard.StopCoroutine(MovementCoroutine);
        }
        activeState = false;
        Agent.speed = _agentBaseSpeed;
    }
    public override void Enter()
    {
        activeState = true;
        Agent.speed = hideSpeed;
    }
    private IEnumerator Hide(Vector3 target)
    {
        
        Debug.Log("START HIDE");
        if (activeState)
        {
            WaitForSeconds Wait = new WaitForSeconds(UpdateFrequency);

            var outerLoop = true;
            
            while (outerLoop)
            {
                for (int i = 0; i < Colliders.Length; i++)
                {
                    Colliders[i] = null;
                }

                int hits = Physics.OverlapSphereNonAlloc(Agent.transform.position, blackboard.viewRadius, Colliders, HidableLayers);

                int hitReduction = 0;
                
                for (int i = 0; i < hits; i++)
                {
                    var ray = new Ray(blackboard.eyeTransform.position,
                        blackboard.lastKnownPlayerPos - blackboard.eyeTransform.position);
                    var rayHit = new RaycastHit();
                    bool playerVisual = Physics.Raycast(ray, out rayHit, 1000f) && rayHit.collider.CompareTag("Player");

                    if (Vector3.Distance(Colliders[i].transform.position, target) < MinPlayerDistance || Colliders[i].bounds.size.y < MinObstacleHeight || !playerVisual)
                    {
                        Colliders[i] = null;
                        hitReduction++;
                    }
                }
                hits -= hitReduction;

                System.Array.Sort(Colliders, ColliderArraySortComparer);

                for (int i = 0; i < hits; i++)
                {
                    if (NavMesh.SamplePosition(Colliders[i].transform.position, out NavMeshHit hit, 5f, Agent.areaMask))
                    {
                        if (!NavMesh.FindClosestEdge(hit.position, out hit, Agent.areaMask))
                        {
                            Debug.LogError($"Unable to find edge close to {hit.position}");
                        }

                        if (Vector3.Dot(hit.normal, (target - hit.position).normalized) < HideSensitivity && Vector3.Distance(blackboard.transform.position, hit.position) > 5)
                        {
                            _hidingLookNormal = hit.normal;
                            if (Agent.remainingDistance > 1f) break;
                            Agent.SetDestination(hit.position);
                            break;
                        }
                        else
                        {
                            // Since the previous spot wasn't facing "away" enough from teh target, we'll try on the other side of the object
                            if (NavMesh.SamplePosition(Colliders[i].transform.position - (target - hit.position).normalized * 2, out NavMeshHit hit2, 2f, Agent.areaMask))
                            {
                                if (!NavMesh.FindClosestEdge(hit2.position, out hit2, Agent.areaMask))
                                {
                                    Debug.LogError($"Unable to find edge close to {hit2.position} (second attempt)");
                                }

                                if (Vector3.Dot(hit2.normal, (target - hit2.position).normalized) < HideSensitivity && Vector3.Distance(blackboard.transform.position, hit2.position) > 5)
                                {
                                    _hidingLookNormal = hit2.normal;
                                    if (Agent.remainingDistance > 1f) break;
                                    Agent.SetDestination(hit2.position);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"Unable to find NavMesh near object {Colliders[i].name} at {Colliders[i].transform.position}");
                    }
                }
                yield return Wait;

                /*while (Agent.remainingDistance < 1f)
                {
                    if (_blackBoard.playerInSight)
                    {
                        MovementCoroutine = StartCoroutine(Hide(_blackBoard.lastKnownPlayerPos));
                        outerLoop = false;
                        break;
                        
                    }

                    yield return new WaitForEndOfFrame();
                }*/




            }
        }
        
    }

    public int ColliderArraySortComparer(Collider A, Collider B)
    {
        if (A == null && B != null)
        {
            return 1;
        }
        else if (A != null && B == null)
        {
            return -1;
        }
        else if (A == null && B == null)
        {
            return 0;
        }
        else
        {
            return Vector3.Distance(Agent.transform.position, A.transform.position).CompareTo(Vector3.Distance(Agent.transform.position, B.transform.position));
        }
    }
}