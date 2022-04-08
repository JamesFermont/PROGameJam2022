using UnityEngine;

public class Movement : MonoBehaviour {
    [Header("Aim Settings")]
    [SerializeField] private Transform pivotTransform;
    [SerializeField, Range(1f, 360f)] private float rotationSpeed = 90f;
    [SerializeField, Range(-89f, 89f)] private float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    [SerializeField, Range(0f, 2f)] private float sensitivityX = 1f, sensitivityY = 1f;
    [SerializeField] private bool invertX, invertY;
    [SerializeField, Min(0f)] private float upAlignmentSpeed = 360f;
    
    [Header("Speed Settings")]
    [SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
    
    [Header("Jump Settings")]
    [SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 10f;
    [SerializeField, Range(0f, 5f)] private float jumpHeight = 3.1f;
    [SerializeField, Range(0, 5)] private int maxAirJumps;

    private Vector2 Sensitivity => new Vector2(sensitivityY, sensitivityX);
    private Vector2 Invert => new Vector2(invertY ? -1 : 1, invertX ? -1 : 1);
    
    private Rigidbody _body, _connectedBody, _previousConnectedBody;
    private InputAxisConverter _inputToAxis;
    private Input.PlayerActions _input;

    private Vector2 _cameraAngles;
    private Vector3 _playerInput;
    private Vector3 _contactNormal;
    private Vector3 _velocity, _connectionVelocity, _connectionWorldPosition, _connectionLocalPosition;
    private Vector3 _upAxis, _rightAxis, _forwardAxis;
    private Quaternion _gravityAlignment = Quaternion.identity;
    private bool _jumpRequested;
    private int _stepsSinceLastJump, _jumpPhase, _groundContactCount;

    private bool Grounded => _groundContactCount > 0;

    private void Awake() {
        _body = GetComponent<Rigidbody>();
        _body.useGravity = false;
        _inputToAxis = new InputAxisConverter();
        _input = new Input().Player;
        _input.Enable();
        transform.localRotation = pivotTransform.localRotation = Quaternion.Euler(_cameraAngles);
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        Vector2 inputXZ = _inputToAxis.InputToAxis(_input.Move.ReadValue<Vector2>());
        _playerInput = new Vector3(inputXZ.x, 0f, inputXZ.y);
        _playerInput = Vector3.ClampMagnitude(_playerInput, 1f);
        
        _rightAxis = ProjectDirectionOnPlane(pivotTransform.right, _upAxis);
        _forwardAxis = ProjectDirectionOnPlane(pivotTransform.forward, _upAxis);

        _jumpRequested |= _input.Jump.WasPerformedThisFrame();
    }

    private void FixedUpdate() {
        Vector3 gravity = Gravity.GetGravity(_body.position, out _upAxis);
        UpdateState();
        AdjustVelocity();
        if ( _jumpRequested ) {
            _jumpRequested = false;
            Jump(gravity);
        }
        
        if ( Grounded && _velocity.sqrMagnitude < 0.01f ) {
            _velocity += _contactNormal * (Vector3.Dot(gravity, _contactNormal) * Time.deltaTime);
        } else {
            _velocity += gravity * Time.deltaTime;
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
        } else {
            _contactNormal = _upAxis;
        }

        if ( _connectedBody ) {
            UpdateConnectionState();
        }
    }
    
    private void UpdateConnectionState() {
        if ( _connectedBody == _previousConnectedBody ) {
            Vector3 connectionMovement = _connectedBody.transform.TransformPoint(_connectionLocalPosition) - _connectionWorldPosition;
            _connectionVelocity = connectionMovement / Time.deltaTime;
        }
        _connectionWorldPosition = _body.position;
        _connectionLocalPosition = _connectedBody.transform.InverseTransformPoint(_connectionWorldPosition);
    }

    private void AdjustVelocity() {
        Vector3 projectedX = _rightAxis;
        Vector3 projectedZ = _forwardAxis;
        float acceleration = Grounded ? maxAcceleration : maxAirAcceleration;
        
        projectedX = ProjectDirectionOnPlane(projectedX, _contactNormal);
        projectedZ = ProjectDirectionOnPlane(projectedZ, _contactNormal);
        
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

    private void Jump(Vector3 gravity) {
        Vector3 jumpDirection;
        _stepsSinceLastJump = 0;
        _jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);

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

        jumpDirection = (jumpDirection + _upAxis).normalized;
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

    private void LateUpdate() {
        UpdateGravityAlignment();
        Vector2 cameraInput = _input.Aim.ReadValue<Vector2>();
        const float e = 0.001f;
        if ( cameraInput.sqrMagnitude > 1000f ) {
            return;
        }
        if ( cameraInput.x < -e || cameraInput.x > e || cameraInput.y < -e || cameraInput.y > e ) {
            _cameraAngles += rotationSpeed * Time.unscaledDeltaTime * cameraInput * Sensitivity * Invert;
        }
        ConstrainAngles();
        Quaternion bodyRotation = Quaternion.Euler(_cameraAngles);
        pivotTransform.localRotation = bodyRotation;
        transform.rotation = _gravityAlignment;
    }
    
    private void UpdateGravityAlignment() {
        Vector3 fromUp = _gravityAlignment * Vector3.up;
        Vector3 toUp = Gravity.GetUpAxis(transform.position);

        float dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1, 1f);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        float maxAngle = upAlignmentSpeed * Time.deltaTime;

        Quaternion newAlignment = Quaternion.FromToRotation(fromUp, toUp) * _gravityAlignment;
        if ( angle <= maxAngle ) {
            _gravityAlignment = newAlignment;
        } else {
            _gravityAlignment = Quaternion.SlerpUnclamped(_gravityAlignment, newAlignment, maxAngle / angle);
        }
    }
    
    private void ConstrainAngles() {
        _cameraAngles.x = Mathf.Clamp(_cameraAngles.x, minVerticalAngle, maxVerticalAngle);

        if ( _cameraAngles.y < 0f ) {
            _cameraAngles.y += 360f;
        } else if ( _cameraAngles.y >= 360f ) {
            _cameraAngles.y -= 360f;
        }
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

    private void OnDrawGizmos() {
        if ( pivotTransform ) {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(pivotTransform.position, pivotTransform.forward);
        }
    }
}
