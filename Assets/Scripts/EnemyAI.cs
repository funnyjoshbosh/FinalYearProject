using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour, IDamagable
{
    public static int count;

    [SerializeField] private GameObject player;
    [SerializeField] private Transform defencePoint;

    public List<GameObject> checkpoints;
    private int currentCheckpoint;
    public float checkpointThreshold;

    Vector3[] path;
    private int currentWaypoint;
    public float waypointThreshold;
    private float pathRequestCooldown;

    private Rigidbody rb;
    public float thrust;
    public float maxSpeed;
    public float rotSpeed;
    public float maxRotStep;

    public int health;
    private float shootCooldown;
    public float fireRate;
    public Transform bulletOrigin;
    public GameObject bulletPrefab;
    public float bulletSpeed;
    public float maxFireRange;
    public float fireAngle;
    public float maintainDist;
    public float maintainDistDelta;

    public float engageRadius;
    public float disengageRadius;

    public Vector2 response;
    public List<GameObject> obstacles;

    public AudioSource shootingAudio;
    public AudioSource explosionAudio;

    [SerializeField] private AIState state;

    private enum AIState
    {
        Patrolling,
        CloseDistance,
        MaintainDistance,
        AvoidCollision,
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        defencePoint = GameObject.Find("Defence Point").transform;
        state = AIState.Patrolling;
        obstacles = new List<GameObject>();
        shootingAudio = bulletOrigin.GetComponent<AudioSource>();
        explosionAudio = GetComponent<AudioSource>();
        count++;
    }

    void Update()
    {
        CheckDeath();
        HandleCooldowns();
        CheckShoot();
    }

    void FixedUpdate()
    {
        CheckObstables();
        StateTransition();
        StateBehaviour();
    }

    private void Movement(float lateralThrust, float forwardThrust)
    {
        rb.AddRelativeForce(new Vector3(lateralThrust, 0f, forwardThrust), ForceMode.Force);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }

    private void LookToward(Vector3 _target)
    {
        Vector2 shipPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 target = new Vector2(_target.x, _target.z);
        Vector2 shipDirection = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 vecToWaypoint = new Vector2(target.x - shipPos.x, target.y - shipPos.y);

        float angle = Vector2.SignedAngle(shipDirection, vecToWaypoint);
        float step = Mathf.Clamp(angle * rotSpeed, -maxRotStep, maxRotStep) * Time.fixedDeltaTime;
        transform.RotateAround(transform.position, Vector3.down, step);
    }

    private void Shoot()
    {
        shootingAudio.Play();
        GameObject bullet = Instantiate(bulletPrefab, bulletOrigin.position, bulletOrigin.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.lifetime = 3f;
        bulletScript.speed = bulletSpeed;
        shootCooldown = 1f / fireRate;
    }

    private void CheckShoot()
    {
        Vector2 shipPos = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector2 vecToPlayer = shipPos - new Vector2(bulletOrigin.position.x,  bulletOrigin.position.z);
        if (vecToPlayer.magnitude > maxFireRange)
            return;

        float angleToPlayer = Vector2.Angle(new Vector2(bulletOrigin.up.x, bulletOrigin.up.z), vecToPlayer.normalized);
        if (angleToPlayer > fireAngle/2f)
            return;

        if (shootCooldown <= 0f)
            Shoot();
    }

    private void HandleCooldowns()
    {
        shootCooldown -= Time.deltaTime;
        pathRequestCooldown -= Time.deltaTime;
    }

    public void Damage()
    {
        health -= 10;
    }

    private void CheckDeath()
    {
        if (health <= 0)
        {
            count--;
            AudioSource.PlayClipAtPoint(explosionAudio.clip, transform.position);
            gameObject.SetActive(false);
        }
    }

    private void StateBehaviour()
    {
        switch (state)
        {
            case AIState.Patrolling:
                Vector3 checkpoint = GetCheckpoint();
                LookToward(checkpoint);
                Movement(0f, thrust);
                break;

            case AIState.CloseDistance:
                if (pathRequestCooldown <= 0f)
                    PathRequestManager.RequestPath(transform.position, player.transform.position, OnPathFound);
                Vector3 waypoint = GetWaypoint();
                LookToward(waypoint);
                Movement(0f, thrust);
                break;

            case AIState.MaintainDistance:
                LookToward(player.transform.position);
                Movement(0f, -thrust);
                break;

            case AIState.AvoidCollision:
                LookToward(player.transform.position);
                Movement(thrust * response.x, thrust * response.y);
                break;

            default:
                break;
        }
    }

    private void StateTransition()
    {
        Vector2 playerShipPos = new Vector2(player.transform.position.x, player.transform.position.z);

        // distance between the player and the station
        float playerStationDist = new Vector2(playerShipPos.x - defencePoint.position.x, playerShipPos.y - defencePoint.position.z).magnitude;

        // distance between the player and the Enemy
        float playerEnemyDist = new Vector2(playerShipPos.x - transform.position.x, playerShipPos.y - transform.position.z).magnitude;

        if (state != AIState.Patrolling && playerStationDist >= disengageRadius)
        {
            path = null;
            state = AIState.Patrolling;
        }
        else if (state == AIState.Patrolling && playerStationDist <= engageRadius)
        {
            path = null;
            state = AIState.CloseDistance;
        }
        else if (state == AIState.CloseDistance && playerEnemyDist <= maintainDist - maintainDistDelta)
        {
            path = null;
            state = AIState.MaintainDistance;
        }
        else if (state == AIState.MaintainDistance && playerEnemyDist > maintainDist + maintainDistDelta)
        {
            path = null;
            state = AIState.CloseDistance;
        }
    }

    private Vector3 GetCheckpoint()
    {
        Vector3 checkpoint = checkpoints[currentCheckpoint].transform.position;
        Vector2 shipPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 checkpointPos = new Vector2(checkpoint.x, checkpoint.z);

        float checkpointDist = (checkpointPos - shipPos).magnitude;
        if (checkpointDist <= checkpointThreshold)
        {
            currentCheckpoint++;
            if (currentCheckpoint >= checkpoints.Count)
                currentCheckpoint = 0;
            checkpoint = checkpoints[currentCheckpoint].transform.position;
        }

        return checkpoint;
    }

    private Vector3 GetWaypoint()
    {
        if (path == null || path.Length <= 0)
            return GetCheckpoint();

        Vector3 waypoint = path[currentWaypoint];
        Vector2 shipPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 waypointPos = new Vector2(waypoint.x, waypoint.z);

        float waypointDist = (waypointPos - shipPos).magnitude;
        if (waypointDist <= waypointThreshold)
        {
            currentWaypoint++;
            if (currentWaypoint >= checkpoints.Count)
                currentWaypoint = 0;
            waypoint = path[currentWaypoint];
        }

        return waypoint;
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        currentWaypoint = 0;

        if (pathSuccessful)
            path = newPath;
        else
            path = null;
    }

    
    private void CheckObstables()
    {
        response = new Vector2(0f, 0f);
        foreach (GameObject obstacle in obstacles)
        {
            Vector3 desire = (transform.position - obstacle.transform.position).normalized;
            response += new Vector2(desire.x, desire.z);
        }

        if (response == Vector2.zero)
            return;

        response.Normalize();
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;
        obstacles.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
            return;
        obstacles.Remove(other.gameObject);
    }
}
