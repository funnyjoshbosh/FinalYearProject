using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime;
    public float speed;

    void FixedUpdate()
    {
        lifetime -= Time.fixedDeltaTime;
        if (lifetime <= 0f)
            Destroy(gameObject);

        transform.position += transform.up * speed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamagable damagable = other.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
            damagable.Damage();
        Debug.Log("I have been triggered");
        Destroy(gameObject);
    }
}