using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNPC : GeneralNPC
{
    // NPC type shouldn't be directly accessed outside of the npc script
    private NpcType type = NpcType.Enemy;
    public NpcType Type { get { return type; } }

    // Fields used for chasing/attacking behavior
    private GameObject player;
    [SerializeField] private float startChaseRange;
    [SerializeField] private float stopChaseRange;
    [SerializeField] private float chaseSpeedModifier = 1.5f;
    [SerializeField] private NpcAttack[] attacks;
    private NpcAttack currentAttack;
    private float currentAttackRange;
    private float[] attackRanges;
    private float maxAttackRange;

    // Navmesh fields
    private NavMeshAgent agent;
    public LayerMask groundLayer, playerLayer;
    private Vector3 desPoint;
    protected bool desPointSet = false;
    public float desPointMin;
    public float desPointMax;
    [SerializeField] private float walkInterval;
    private float lastWalkTime = -Mathf.Infinity;

    // Fields used in the state machine
    private enum Mode
    {
        Wandering,
        Chasing,
        Attacking
    }
    private Mode currentMode = Mode.Wandering;




    // State machine method
    public override void CheckMovementMode()
    {
        switch (currentMode)
        {
            case Mode.Wandering:

                WanderMovementScript();
                break;
            case Mode.Chasing:
                ChasingMovementScript();
                break;
            case Mode.Attacking:
                AttackingMovementScript();
                break;
            default:
                WanderMovementScript();
                Debug.LogWarning("Unknown movement mode, defaulting to wandering.");
                break;
        }
    }




    // Method called upon creation of the npc
    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.acceleration = 2f * speed;
        agent.angularSpeed = 135f * speed;
        for (int i = 0; i < attacks.Length; i++)
        {
            attackRanges = new float[attacks.Length];
            attackRanges[i] = attacks[i].attackRange;
        }
        if (attackRanges.Length > 0)
        {
            maxAttackRange = attackRanges.Max();
        }
        else
        {
            maxAttackRange = 3f;
        }
        currentAttack = attacks[0];
    }


    // Method called once per frame (for testing) - in future versions may use a centralised method to update all npcs with less overhead
    private void Update()
    {
        CheckMovementMode();
    }


    // Method for when the npc is wandering around
    protected virtual void WanderMovementScript()
    {
        bool inRange = Physics.CheckSphere(transform.position, startChaseRange, playerLayer);
        if (inRange)
        {
            currentMode = Mode.Chasing;
            return;
        }

        if (!desPointSet)
        {
            if (Time.time - lastWalkTime < walkInterval) return;
            SearchDesPoint();
        }

        if (desPointSet)
        {
            agent.SetDestination(desPoint);

            Vector3 distanceToDesPoint = transform.position - desPoint;
            distanceToDesPoint.y = 0;

            if (distanceToDesPoint.magnitude < 3f)
            {
                desPointSet = false;
                lastWalkTime = Time.time;
            }
        }
    }


    // Search for a valid random destination point on the ground within specified range
    private void SearchDesPoint()
    {
        float zPos = RandRange();
        float xPos = RandRange();
        desPoint = new Vector3(transform.position.x + xPos, transform.position.y, transform.position.z + zPos);

        if (Physics.Raycast(desPoint, -transform.up, 2f, groundLayer))
        {
            desPointSet = true;
        }
    }


    // function to get a random float within the specified min and max range, randomly positive or negative
    private float RandRange()
    {
        float pos = Random.Range(desPointMin, desPointMax);
        return Random.value > 0.5f ? pos : -pos;
    }


    // Method for when the npc is chasing after the player
    protected virtual void ChasingMovementScript()
    {
        bool inRange = Physics.CheckSphere(transform.position, stopChaseRange, playerLayer);
        if (!inRange)
        {
            currentMode = Mode.Wandering;
            agent.speed = speed;
            return;
        }

        agent.speed = speed * chaseSpeedModifier;
        agent.SetDestination(player.transform.position);

        inRange = Physics.CheckSphere(transform.position, maxAttackRange, playerLayer);
        if (inRange)
        {
            currentMode = Mode.Attacking;
            agent.speed = speed;
            return;
        }
    }


    // Method to run the attacking logic of the npcs - does not attack but handles choosing attacks and switching back to chase mode
    protected virtual void AttackingMovementScript()
    {
        agent.SetDestination(transform.position);
        bool inRange = Physics.CheckSphere(transform.position, maxAttackRange, playerLayer);
        if (!inRange)
        {
            currentMode = Mode.Chasing;
            if (currentAttack != null)
            {
                currentAttack.StopAttack();
            }
            return;
        }
        if (attackRanges.Length == 0) return;
        if (!currentAttack.attackActive)
        {
            SelectAttack();
        }
        inRange = Physics.CheckSphere(transform.position, currentAttackRange, playerLayer);
        if (inRange && !currentAttack.attackActive)
        {
            if (Time.time - currentAttack.lastAttackTime > currentAttack.attackCooldown)
            {
                currentAttack.lastAttackTime = Time.time;
                currentAttack.TriggerAttack(agent, player);
            }
        }
    }


    // Select attack
    private void SelectAttack()
    {
        int attackIndex = Random.Range(0, attackRanges.Length);

        currentAttack = attacks[attackIndex];
        currentAttackRange = attackRanges[attackIndex];

    }
}
