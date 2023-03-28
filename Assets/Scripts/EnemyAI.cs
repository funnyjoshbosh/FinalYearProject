using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour, IDamagable
{
    [SerializeField] private GameObject player;
    [SerializeField] private Transform defencePoint;

    private Rigidbody rb;
    public List<GameObject> waypoints;
    public int currentWaypoint;
    public float waypointThreshold;

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
    public float fireRangePercent; // This is the percentage of the max fire range before maintaining distance

    public float engageRadius;
    public float disengageRadius;

    [SerializeField] private AIState state;

    private enum AIState
    {
        Patrolling,
        CloseDistance,
        MaintainDistance,
    }

    void Start()
    {
        currentWaypoint = 0;
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        defencePoint = GameObject.Find("Defence Point").transform;
        state = AIState.Patrolling;
    }

    void Update()
    {
        CheckDeath();
        CheckShoot();
    }

    void FixedUpdate()
    {
        StateTransition();
        StateBehaviour();
    }

    private void Movement(GameObject waypoint, float thrust)
    {
        rb.AddRelativeForce(new Vector3(0f, 0f, thrust), ForceMode.Force);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }

    private void LookToward(GameObject _target)
    {
        Vector2 shipPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 target = new Vector2(_target.transform.position.x, _target.transform.position.z);
        Vector2 shipDirection = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 vecToWaypoint = new Vector2(target.x - shipPos.x, target.y - shipPos.y);

        float angle = Vector2.SignedAngle(shipDirection, vecToWaypoint);
        float step = Mathf.Clamp(angle * rotSpeed, -maxRotStep, maxRotStep) * Time.fixedDeltaTime;
        transform.RotateAround(transform.position, Vector3.down, step);
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletOrigin.position, bulletOrigin.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.lifetime = 3f;
        bulletScript.speed = bulletSpeed;
        shootCooldown = 1f / fireRate;
    }

    private void CheckShoot()
    {
        shootCooldown -= Time.fixedDeltaTime;

        Vector2 shipPos = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector2 vecToPlayer = new Vector2(shipPos.x - bulletOrigin.position.x, shipPos.y - bulletOrigin.position.z);
        if (vecToPlayer.magnitude > maxFireRange)
            return;

        float angleToPlayer = Vector2.Angle(bulletOrigin.up, vecToPlayer);
        if (angleToPlayer > fireAngle)
            return;

        if (shootCooldown <= 0f)
            Shoot();
    }

    public void Damage()
    {
        health -= 10;
    }

    private void CheckDeath()
    {
        if (health <= 0)
            Destroy(gameObject);
    }

    private void StateBehaviour()
    {
        if (state == AIState.CloseDistance)
        {
            LookToward(player);
            Movement(player, thrust);
        }

        else if (state == AIState.MaintainDistance)
        {
            LookToward(player);
            Movement(player, -thrust);
        }

        else if (state == AIState.Patrolling)
        {
            GameObject waypoint = GetWaypoint();
            LookToward(waypoint);
            Movement(waypoint, thrust);
        }
    }

    private void StateTransition()
    {
        Vector2 playerShipPos = new Vector2(player.transform.position.x, player.transform.position.z);
        // distance between the player and the station
        float playerStationDist = new Vector2(playerShipPos.x - defencePoint.position.x, playerShipPos.y - defencePoint.position.z).magnitude;
        // distance between the player and the Enemy
        float playerEnemyDist = new Vector2(playerShipPos.x - transform.position.x, playerShipPos.y - transform.position.z).magnitude;

        if ((state == AIState.CloseDistance || state == AIState.MaintainDistance) && playerStationDist >= disengageRadius)
            state = AIState.Patrolling;

        else if (state == AIState.Patrolling && playerStationDist <= engageRadius)
            state = AIState.CloseDistance;

        else if (state == AIState.CloseDistance && playerEnemyDist <= fireRangePercent/100f * maxFireRange)
            state = AIState.MaintainDistance;

        else if (state == AIState.MaintainDistance && playerEnemyDist > maxFireRange)
            state = AIState.CloseDistance;
    }

    private GameObject GetWaypoint()
    {
        GameObject waypoint = waypoints[currentWaypoint];

        Vector2 shipPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 waypointPos = new Vector2(waypoint.transform.position.x, waypoint.transform.position.z);
        float waypointDist = (waypointPos - shipPos).magnitude;
        if (waypointDist <= waypointThreshold)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Count)
                currentWaypoint = 0;
            waypoint = waypoints[currentWaypoint];
        }

        return waypoint;
    }
}
