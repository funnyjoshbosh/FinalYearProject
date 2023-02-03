using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Camera cam;
    public InputMaster inputMaster;
    public float camOffset;
    public float thrust;
    public float maxSpeed;
    public float rotSpeed;
    public float maxRotStep;
    private Vector2 mousePos;
    private Vector2 movement;

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

    void FixedUpdate()
    {
        rb.AddRelativeForce(new Vector3(movement.x, 0.0f, movement.y) * thrust, ForceMode.Force);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        Vector2 shipPos = cam.WorldToScreenPoint(transform.position);
        Vector2 shipDirection = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 targetDirection = mousePos - shipPos;

        float angle = Vector2.SignedAngle(shipDirection, targetDirection);
        float step = Mathf.Clamp(angle * rotSpeed, -maxRotStep, maxRotStep) * Time.fixedDeltaTime;
        transform.RotateAround(transform.position, Vector3.down, step);
    }
}