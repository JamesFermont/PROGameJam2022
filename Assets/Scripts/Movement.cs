using UnityEngine;

public class Movement : MonoBehaviour {
    [Header("Speed Settings")]
    [SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
    
    [Header("Jump Settings")]
    [SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 10f;
    [SerializeField, Range(0f, 5f)] private float jumpHeight = 3.1f;
    [SerializeField, Range(0, 5)] private int maxAirJumps;

    private Rigidbody _body, _connectedBody, _previousConnectedBody;
    private InputAxisConverter _inputToAxis;
    private Input.PlayerActions _input;

    private Vector3 _playerInput;
    private Vector3 _contactNormal;
    private Vector3 _velocity, _connectionVelocity;
    private bool _jumpRequested;
    private int _stepsSinceLastJump, _jumpPhase, _groundContactCount;

    private bool Grounded => _groundContactCount > 0;

    private void Awake() {
        _body = GetComponent<Rigidbody>();
        _inputToAxis = new InputAxisConverter();
        _input = new Input().Player;
        _input.Enable();
    }

    private void Update() {
        Vector2 inputXZ = _inputToAxis.InputToAxis(_input.Move.ReadValue<Vector2>());
        _playerInput = new Vector3(inputXZ.x, 0f, inputXZ.y);
        _playerInput = Vector3.ClampMagnitude(_playerInput, 1f);

        _jumpRequested |= _input.Jump.WasPerformedThisFrame();
    }

    private void FixedUpdate() {
        UpdateState();
        AdjustVelocity();
        if ( _jumpRequested ) {
            _jumpRequested = false;
            Jump();
        }
        _body.velocity = _velocity;
        ClearState();
    }

    private void UpdateState() {
        _stepsSinceLastJump += 1;
        _velocity = _body.velocity;

        if ( Grounded ) {
            if ( _stepsSinceLastJump > 1 ) {
                _jumpPhase = 0;
            }
            if ( _groundContactCount > 1 ) {
                _contactNormal.Normalize();
            }
        }
    }

    private void AdjustVelocity() {
        float acceleration = Grounded ? maxAcceleration : maxAirAcceleration;
        Vector3 projectedX = ProjectDirectionOnPlane(Vector3.right, _contactNormal);
        Vector3 projectedZ = ProjectDirectionOnPlane(Vector3.forward, _contactNormal);

        Vector3 relativeVelocity = _velocity - _connectionVelocity;
        Vector3 adjustment;

        adjustment.x = _playerInput.x * maxSpeed - Vector3.Dot(relativeVelocity, projectedX);
        adjustment.y = 0f;
        adjustment.z = _playerInput.z * maxSpeed - Vector3.Dot(relativeVelocity, projectedZ);

        adjustment = Vector3.ClampMagnitude(adjustment, acceleration * Time.deltaTime);

        _velocity += projectedX * adjustment.x + projectedZ * adjustment.z;
    }
    
    private Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    private void Jump() {
        Vector3 jumpDirection;
        _stepsSinceLastJump = 0;
        _jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * jumpHeight);

        if ( Grounded ) {
            jumpDirection = _contactNormal;
        } else if (maxAirJumps > 0 && _jumpPhase <= maxAirJumps) {
            if ( _jumpPhase == 0 ) {
                _jumpPhase = 1;
            }
            jumpDirection = _contactNormal;
        } else {
            return;
        }

        jumpDirection = (jumpDirection + Vector3.up).normalized;
        float alignedSpeed = Vector3.Dot(_velocity, jumpDirection);
        if ( alignedSpeed > 0f ) {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        
        _velocity += jumpDirection * jumpSpeed;
    }

    private void ClearState() {
        _groundContactCount = 0;
        _contactNormal = _connectionVelocity = Vector3.zero;
        _previousConnectedBody = _connectedBody;
        _connectedBody = null;
    }

    private void OnCollisionStay(Collision other) {
        EvaluateCollision(other);
    }

    private void OnCollisionExit(Collision other) {
        EvaluateCollision(other);
    }

    private void EvaluateCollision(Collision collision) {
        for (int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;
            _groundContactCount += 1;
            _contactNormal += normal;
            _connectedBody = collision.rigidbody;
        }
    }
}
