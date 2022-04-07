using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityRigidbody : MonoBehaviour {
    [Header("Rigidbody Settings")]
    [SerializeField] private bool floatToSleep;
    
    [Header("Submergence Settings")]
    [SerializeField] float submergenceOffset = 0.5f;
    [SerializeField, Min(0.1f)] float submergenceRange = 1f;
    [SerializeField, Min(0f)] float buoyancy = 1f;
    [SerializeField, Range(0f, 10f)] float waterDrag = 1f;
    [SerializeField] LayerMask waterMask = 0;
    [SerializeField] private Vector3 buoyancyOffset = Vector3.zero;
    
    private float _floatDelay, _submergence;
    private Vector3 _gravity;
    
    private Rigidbody _body;

    private void Awake() {
        _body = GetComponent<Rigidbody>();
        _body.useGravity = false;
    }

    private void FixedUpdate() {
        if ( floatToSleep ) {
            if ( _body.IsSleeping() ) {
                _floatDelay = 0f;
                return;
            }

            if ( _body.velocity.sqrMagnitude < 0.0001f ) {
                _floatDelay += Time.deltaTime;
                if ( _floatDelay >= 1f ) {
                    return;
                }
            } else {
                _floatDelay = 0f;
            }
        }

        _gravity = Gravity.GetGravity(_body.position);
        if ( _submergence > 0f ) {
            float drag = Mathf.Max(0f, 1f - waterDrag * _submergence * Time.deltaTime);
            _body.velocity *= drag;
            _body.angularVelocity *= drag;
            _body.AddForceAtPosition(_gravity * -(buoyancy * submergenceRange), transform.TransformPoint(buoyancyOffset), ForceMode.Acceleration);
            _submergence = 0f;
        }

        _body.AddForce(_gravity, ForceMode.Acceleration);
    }

    private void OnTriggerEnter(Collider other) {
        if ((waterMask & (1 << other.gameObject.layer)) != 0) {
            EvaluateSubmergence();
        }
    }

    private void OnTriggerStay(Collider other) {
        if (!_body.IsSleeping() && (waterMask & (1 << other.gameObject.layer)) != 0) {
            EvaluateSubmergence();
        }
    }

    private void EvaluateSubmergence() {
        Vector3 upAxis = -_gravity.normalized;
        if ( Physics.Raycast(_body.position + upAxis * submergenceOffset, -upAxis, out RaycastHit hit, submergenceRange + 1f, waterMask, QueryTriggerInteraction.Collide) ) {
            _submergence = 1f - hit.distance / submergenceRange;
        } else {
            _submergence = 1f;
        }
    }
}
