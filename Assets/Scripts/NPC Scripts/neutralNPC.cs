using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NeutralNPC : GeneralNPC
{
    // NPC type shouldn't be directly accessed outside of the npc script
    private NpcType type = NpcType.Neutral;
    public NpcType Type { get { return type; } }

    // Fields used for chasing/attacking behavior
    [SerializeField] private GameObject player;
    [SerializeField] private float stopChaseRange;
    [SerializeField] private NpcAttack[] attacks;
    private float[] attackRanges;
    private float maxAttackRange;

    // Navmesh fields
    private NavMeshAgent agent;
    public LayerMask groundLayer, playerLayer;
    private Vector3 desPoint;
    private bool desPointSet = false;
    public float desPointMin;
    public float desPointMax;
    [SerializeField] private float walkInterval;
    private float lastWalkTime = -Mathf.Infinity;
    private Vector3 escapeStart;
    [SerializeField] private float escapeTimeout;
    private bool isEscaping = false;
    private bool escapeInitiated = false;
    private float startEscapeTime = -Mathf.Infinity;
    [SerializeField] private float escapeMoveModifier = 2f;

    // Fields used in the state machine
    private enum Mode
    {
        Wandering,
        Escaping,
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

                GetComponent<Renderer>().material.color = Color.cyan;
                WanderMovementScript();
                break;
            case Mode.Escaping:
                GetComponent<Renderer>().material.color = Color.white;
                EscapeMovementScript();
                break;
            case Mode.Chasing:
                GetComponent<Renderer>().material.color = Color.magenta;
                ChasingMovementScript();
                break;
            case Mode.Attacking:
                GetComponent<Renderer>().material.color = Color.yellow;
                AttackingMovementScript();
                break;
            default:
                WanderMovementScript();
                GetComponent<Renderer>().material.color = Color.blue;
                Debug.LogWarning("Unknown movement mode, defaulting to wandering.");
                break;
        }
    }




    // Method called upon creation of the npc
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.acceleration = 2f * speed;
        agent.angularSpeed = 135f * speed;
        for (int i = 0; i < attacks.Length; i++)
        {
            attackRanges = new float[attacks.Length];
            attackRanges[i] = attacks[i].AttackRange;
        }
        maxAttackRange = attackRanges.Max();
    }


    // Method called once per frame (for testing) - in future versions may use a centralised method to update all npcs with less overhead
    private void Update()
    {
        CheckMovementMode();
    }


    // Method for when the npc is wandering around
    private void WanderMovementScript()
    {
        if (!desPointSet)
        {
            if (Time.time - lastWalkTime < walkInterval) return;
            SearchDesPoint(false);
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
    private void SearchDesPoint(bool escape)
    {
        float zPos = RandRange(escape);
        float xPos = RandRange(escape);

        // When escaping, bias the destination point away from the escape start position
        if (escape)
        {
            Vector3 offset = new Vector3(xPos, 0f, zPos);
            Vector3 awayDirection = transform.position - escapeStart;
            awayDirection.y = 0;
            if (awayDirection.sqrMagnitude < 1e-6f) awayDirection = transform.forward;
            awayDirection.Normalize();

            float d = Vector3.Dot(awayDirection, offset);
            if (d < -0.25f) offset -= 2f * d * awayDirection;

            desPoint = transform.position + offset;
        }
        else
        {
            desPoint = new Vector3(transform.position.x + xPos, transform.position.y, transform.position.z + zPos);
        }

        if (Physics.Raycast(desPoint, -transform.up, 2f, groundLayer))
        {
            desPointSet = true;
        }
    }


    // function to get a random float within the specified min and max range, randomly positive or negative
    private float RandRange(bool escape)
    {
        float pos = Random.Range(desPointMin, desPointMax);
        if (escape)
        {
            pos /= 2f;
        }
        return Random.value > 0.5f ? pos : -pos;
    }

    // Method for when the npc is wandering around
    private void EscapeMovementScript()
    {
        EscapeTimeout();
        if (!desPointSet)
        {
            SearchDesPoint(true);
        }
        if (desPointSet)
        {
            agent.SetDestination(desPoint);

            Vector3 distanceToDesPoint = transform.position - desPoint;
            distanceToDesPoint.y = 0;

            if (distanceToDesPoint.magnitude < 3f)
            {
                desPointSet = false;
            }
        }
    }


    // Method checking for escape movement timeout
    private void EscapeTimeout()
    {
        if (isEscaping)
        {
            if (Time.time - startEscapeTime > escapeTimeout)
            {
                isEscaping = false;
                desPointSet = false;
                currentMode = Mode.Wandering;
                agent.speed = speed;
                agent.acceleration = 2f * speed;
                agent.angularSpeed = 135f * speed;
                return;
            }
        }
        else
        {
            escapeStart = transform.position;
            startEscapeTime = Time.time;
            isEscaping = true;
            escapeInitiated = true;
            agent.speed = escapeMoveModifier * speed;
            agent.acceleration = escapeMoveModifier * 6f * speed;
            agent.angularSpeed = escapeMoveModifier * 270f * speed;
        }
    }


    // Method for when the npc is chasing after the player
    private void ChasingMovementScript()
    {
        bool inRange = Physics.CheckSphere(transform.position, stopChaseRange, playerLayer);
        if (!inRange)
        {
            currentMode = Mode.Wandering;
        }

        agent.SetDestination(player.transform.position);

        if (attackRanges.Length == 0) return;
        float attackRange = attackRanges[Random.Range(0, attackRanges.Length)];
        inRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);
        if (inRange)
        {
            currentMode = Mode.Attacking;
        }
    }


    // Method to run the attacking logic of the npcs - does not attack but handles choosing attacks and switching back to chase mode
    private void AttackingMovementScript()
    {
        bool inRange = Physics.CheckSphere(transform.position, maxAttackRange, playerLayer);
        if (!inRange)
        {
            currentMode = Mode.Chasing;
            return;
        }
        float attackRange = attackRanges[Random.Range(0, attackRanges.Length)];
        inRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);
        if (inRange)
        {
            NpcAttack chosenAttack = attacks[Random.Range(0, attacks.Length)];
            chosenAttack.TriggerAttack();
        }

        if (!escapeInitiated)
        {
            CheckEscapeCondition();
        }
    }


    // Override takeDamage method to switch to chase mode upon taking damage
    public override void TakeDamage(int damage, DamageCause cause)
    {
        base.TakeDamage(damage, cause);
        if (cause == DamageCause.PlayerAttack)
        {
            if (!isEscaping)
            {
                currentMode = Mode.Chasing;
            }
        }
    }


    // Method to check if escape condition is met (health below 40% by default)
    protected virtual void CheckEscapeCondition()
    {
        if (Health / StartHealth < 0.4)
        {
            currentMode = Mode.Escaping;
        }
    }
}
