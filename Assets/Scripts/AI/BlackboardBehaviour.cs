using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;


public enum CombatRange
{
    melee,
    ranged,
    outOfRange
};

public class BlackboardBehaviour : MonoBehaviour
{

    [Header("World State")] [SerializeField]
    public bool playerInSight;

    public bool playerDetected;
    public Vector3 lastKnownPlayerPos;
    public double timeSincePlayerHidden;
    public Vector3 lastKnownPlayerLookDir;
    public Vector3 lastKnownPlayerVelocity;

    public CombatRange combatRange = CombatRange.outOfRange;
    public Vector3 lastDamageDirection;

    public int teammatesNearby;

    [Header("Sensory")] [Tooltip("White Circle")]
    public float viewRadius;

    public float viewAngle;
    [Tooltip("Magenta Circle")] public float hearingRadius;

    [Tooltip("The time after enemy visual is lost before position of enemy stops updating")]
    public float spatialPlayerDetectionTime;

    [Tooltip("Green Circle")] public float teamDetectionRange;
    public int maxTeammateMemory;
    [Tooltip("Yellow Circle")] public float meleeRange;
    [Tooltip("Red Circle")] public float fireRangeLimit;
    public Transform eyeTransform;
    public Transform bodyTransform;
    public Transform projectileOrigin;

    [Header("Emotion")]
    
    [SerializeField] private float timeBetweenEmotions;

    public bool confident;
    public bool aggressive;
    public bool panic;
    
    private float _emotionCounter;
    


    [Header("Entity Status")] public int team;
    public float shield;
    public float health;
    public float shieldDownTime;
    [Tooltip("Points per second")] public float shieldRechargeRate;


    [Header("Shared")] [SerializeField] private PlayerStats _playerStats;


    //TIMERS
    private float timeSinceHit;

    //SHIELD
    [HideInInspector] public float fullShield;
    [HideInInspector] public float fullHealth;

    //Layers & Tags

    private LayerMask entityLayerMask;

    void OnValidate()
    {
        if (meleeRange >= fireRangeLimit) meleeRange = fireRangeLimit - 1f;
    }

    private void OnEnable()
    {
        EventHandler.PlayerWeaponFiredEvent += OnPlayerWeaponFiredEvent;
    }

    private void OnDisable()
    {
        EventHandler.PlayerWeaponFiredEvent -= OnPlayerWeaponFiredEvent;
    }


    private void Awake()
    {
    }

    void Start()
    {
        EventHandler.RegisterEntityEvent(this);
        fullShield = shield;
        fullHealth = health;
        entityLayerMask = LayerMask.GetMask("Entity");
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            //Destroy(gameObject);

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            StopAllCoroutines();

            gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        UpdateWorldState();
        UpdateEmotionState();
    }

    private void UpdateWorldState()
    {
        playerInSight = CheckPlayerInSight();

        if (timeSinceHit <= 100000f) timeSinceHit += Time.fixedDeltaTime;



        if (playerInSight)
        {
            if (!playerDetected) playerDetected = true;
            lastKnownPlayerPos = _playerStats.playerPos;
            lastKnownPlayerVelocity = _playerStats.playerMovementDirection;
            lastKnownPlayerLookDir = _playerStats.playerLookDirection;
            timeSincePlayerHidden = 0f;
        }
        else
        {
            if (playerDetected)
            {
                if (timeSincePlayerHidden < spatialPlayerDetectionTime) lastKnownPlayerPos = _playerStats.playerPos;
                timeSincePlayerHidden += Time.fixedDeltaTime;
            }
            else
            {
                
            }
        }
        

        CalculateCombatRange();

        UpdateShield();

        teammatesNearby = LookForTeamMates();
    }

    private void UpdateEmotionState()
    {
        _emotionCounter += Time.fixedDeltaTime;

        if (_emotionCounter > timeBetweenEmotions)
        {
            _emotionCounter = 0;
            
            CalculateEmotionalFeelings();
            
            
        }
    }

    private void CalculateEmotionalFeelings()
    {
        if (teammatesNearby > 0 || health == fullHealth)
        {
            confident = true;
        }
        else
        {
            confident = false;
        }

        if (health != fullHealth && shield <= fullShield * .75f)
        {
            aggressive = true;
        }
        else
        {
            aggressive = false;
        }

        if (health <= fullHealth * .5f && shield < fullShield)
        {
            panic = true;
        }
        else
        {
            panic = false;
        }
    }

    private void CalculateCombatRange()
    {
        var distanceToPlayer = Vector3.Distance(transform.position, _playerStats.playerPos);

        if (distanceToPlayer > fireRangeLimit)
        {
            combatRange = CombatRange.outOfRange;
            return;
        }
        else
        {
            if (distanceToPlayer > meleeRange)
            {
                combatRange = CombatRange.ranged;
                return;
            }
            else
            {
                combatRange = CombatRange.melee;
            }
        }
    }

    private int LookForTeamMates()
    {
        var mates = 0;

        Collider[] colliders = new Collider[maxTeammateMemory];

        var numOfColliders =
            Physics.OverlapSphereNonAlloc(eyeTransform.position, teamDetectionRange, colliders, entityLayerMask);

        for (int i = 0; i < numOfColliders; i++)
        {
            if (colliders[i].transform.parent.TryGetComponent<BlackboardBehaviour>(out var board))
            {
                if (board.team == team && board.gameObject != gameObject) mates++;
            }
        }

        return mates;
    }

    private bool CheckPlayerInSight()
    {
        var distanceToPlayer = Vector3.Distance(eyeTransform.position, _playerStats.playerPos);

        if (distanceToPlayer > viewRadius) return false;

        var dirToPlayer = (_playerStats.playerPos - eyeTransform.position).normalized;

        if (Vector3.Angle(eyeTransform.forward, dirToPlayer) > viewAngle * 0.5f) return false;

        var ray = new Ray(eyeTransform.position, dirToPlayer);

        if (Physics.Raycast(ray, out var rayHit, distanceToPlayer))
        {
            if (!rayHit.collider.gameObject.CompareTag("Player"))
            {
                Debug.DrawLine(ray.origin, rayHit.point, Color.red);
                return false;
            }
        }

        Debug.DrawLine(ray.origin, rayHit.point, Color.green);
        return true;
    }

    private void UpdateShield()
    {
        if (timeSinceHit >= shieldDownTime && shield < fullShield)
        {
            shield += shieldRechargeRate * Time.fixedDeltaTime;
        }
    }

    public void RecieveDamage(float damage, Vector3 dir)
    {
        if (shield > 0 && shield >= damage)
        {
            shield -= damage;
        }
        else if (shield > 0 && shield < damage)
        {
            damage -= shield;
            shield = 0f;
            health -= damage;
        }
        else if (shield <= 0)
        {
            health -= damage;
        }

        lastDamageDirection = -dir.normalized;
        timeSinceHit = 0f;
        
    }

    private void OnPlayerWeaponFiredEvent(Vector3 pos)
    {

        if (!playerDetected)
        {
            if (Vector3.Distance(transform.position, pos) < hearingRadius)
            {
                playerDetected = true;
                lastKnownPlayerPos = pos;
            }
        }else if (!playerInSight)
        {
            if (Vector3.Distance(transform.position, pos) < hearingRadius)
            {
                lastKnownPlayerPos = pos;
            }
            
        }
        
    }

    public void RunCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public void AbortCoroutine(IEnumerator coroutine)
    {
        StopCoroutine(coroutine);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += eyeTransform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BlackboardBehaviour))]
public class EnemyBehaviourEditor : Editor
{
    private void OnSceneGUI()
    {
        BlackboardBehaviour blackboard = target as BlackboardBehaviour;
        if (blackboard == null) return;
        var eyePos = blackboard.eyeTransform.position;
        Handles.color = Color.white;
        Handles.DrawWireArc(eyePos, Vector3.up, Vector3.forward, 360,
            blackboard.viewRadius);

        Vector3 viewAngleA = blackboard.DirFromAngle(-blackboard.viewAngle * 0.5f, false);
        Vector3 viewAngleB = blackboard.DirFromAngle(blackboard.viewAngle * 0.5f, false);

        Handles.color = Color.blue;

        Handles.DrawLine(eyePos, eyePos + viewAngleA * blackboard.viewRadius);
        Handles.DrawLine(eyePos, eyePos + viewAngleB * blackboard.viewRadius);


        Handles.color = Color.yellow;

        Handles.DrawWireDisc(blackboard.transform.position, Vector3.up, blackboard.meleeRange);

        Handles.color = Color.red;

        Handles.DrawWireDisc(blackboard.transform.position, Vector3.up, blackboard.fireRangeLimit);

        Handles.color = Color.green;

        Handles.DrawWireDisc(blackboard.transform.position, Vector3.up, blackboard.teamDetectionRange);

        Handles.color = Color.magenta;
        
        Handles.DrawWireDisc(blackboard.transform.position, Vector3.up, blackboard.hearingRadius);



    }
}
#endif