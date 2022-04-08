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
    [Space]
    [SerializeField] private string targetTag = "Interactable";

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * projectileSpeed, ForceMode.Impulse);
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag(targetTag))
        {
            InteractionObject interactionObject = collider.GetComponent<InteractionObject>();
            interactionObject.DoInteract(mode);
        }

        hitCount--;
        if (hitCount == 0)
            Destroy(gameObject);
    }
}
