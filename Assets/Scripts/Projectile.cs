using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public Gun.ProjectileMode mode;
    [Space]
    public int hitCount;

    [SerializeField] private float projectileSpeed;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.velocity = transform.forward * projectileSpeed;
    }

    public void OnCollisionEnter(Collision col) {
        hitCount--;
        if ( hitCount == 0 ) {
            if ( col.gameObject.TryGetComponent(out InteractionObject interactionObject) ) {
                interactionObject.DoInteract(mode);
            }
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawRay(transform.position, rb.velocity);
    }
}
