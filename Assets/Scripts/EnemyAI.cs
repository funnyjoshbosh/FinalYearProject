using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour, IDamagable
{
    public Transform player;

    private Rigidbody rb;
    public List<GameObject> waypoints;
    public int currentWaypoint;
    public float thrust;
    public float maxSpeed;
    public float rotSpeed;
    public float maxRotStep;
    private Vector2 movement;

    public int health;
    [SerializeField] private bool isShoot;
    private float shootCooldown;
    public float fireRate;
    public Transform bulletOrigin;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;

    public float maxFireRange;
    public float fireAngle;

    private enum State
    {
        Patroling,
        Chasing,
        Engaging,
    }

    // Start is called before the first frame update
    void Start()
    {
        currentWaypoint = 0;
        rb = GetComponent<Rigidbody>();
        movement = Vector2.zero;
        player = GameObject.Find("Player").transform;
        if (player == null)
            Debug.Log("Player not found!");
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
        CheckShoot();
        shootCooldown -= Time.fixedDeltaTime;
        if (isShoot && shootCooldown <= 0f)
            Shoot();
    }
    // Add a raycast below that checks if the player is within range to shoot
    void FixedUpdate()
    {
        if (currentWaypoint <= waypoints.Count)
        {
            LookToward(waypoints[currentWaypoint]);
            MoveToward(waypoints[currentWaypoint]);
        }
    }

    private void MoveToward(GameObject waypoint)
    {
        rb.AddRelativeForce(new Vector3(movement.x, 0.0f, movement.y) * thrust, ForceMode.Force);
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
        isShoot = false;
    }

    private void CheckShoot()
    {
        Vector2 vecToPlayer = new Vector2(player.position.x - bulletOrigin.position.x, player.position.z - bulletOrigin.position.z);
        if (vecToPlayer.magnitude > maxFireRange)
            return;

        float angleToPlayer = Vector2.Angle(bulletOrigin.up, vecToPlayer);
        if (angleToPlayer > fireAngle)
            return;

        isShoot = true;
    }

    public void Damage()
    {
        health -= 10;
    }
}
