using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamagable
{
    private Rigidbody rb;
    private Camera cam;
    public InputMaster inputMaster;
    public float thrust;
    public float maxSpeed;
    public float rotSpeed;
    public float maxRotStep;
    private Vector2 mousePos;
    private Vector2 movement;

    public int health;
    private bool isShoot;
    private float shootCooldown;
    public float fireRate;
    public Transform bulletOrigin;
    public GameObject bulletPrefab;
    public float bulletSpeed;

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        isShoot = ctx.ReadValueAsButton();
    }

    public void OnAim(InputAction.CallbackContext ctx)
    {
        mousePos = ctx.ReadValue<Vector2>();
    }

    public void OnThrust(InputAction.CallbackContext ctx)
    {
        movement = ctx.ReadValue<Vector2>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        mousePos = Vector2.zero;
        movement = Vector2.zero;
    }

    void Update()
    {
        if (health <= 0)
        {
            Debug.Log("You have died");
        }
    }

    void FixedUpdate()
    {
        shootCooldown -= Time.fixedDeltaTime;
        if (isShoot && shootCooldown <= 0f)
            Shoot();

        rb.AddRelativeForce(new Vector3(movement.x, 0.0f, movement.y) * thrust, ForceMode.Force);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        Vector2 shipPos = cam.WorldToScreenPoint(transform.position);
        Vector2 shipDirection = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 targetDirection = mousePos - shipPos;

        float angle = Vector2.SignedAngle(shipDirection, targetDirection);
        float step = Mathf.Clamp(angle * rotSpeed, -maxRotStep, maxRotStep) * Time.fixedDeltaTime;
        transform.RotateAround(transform.position, Vector3.down, step);
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletOrigin.position, bulletOrigin.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.lifetime = 3f;
        bulletScript.speed = bulletSpeed;
        shootCooldown = 1f/fireRate;
    }

    public void Damage()
    {
        health -= 10;
    }
}