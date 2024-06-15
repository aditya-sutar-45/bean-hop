using TMPro.EditorUtilities;
using UnityEngine;

public class WallRunning : MonoBehaviour {
    [Header("Wallrunning Stuff")]
    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float _wallRunTimer;

    [Header("Input Stuff")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode upwardsWallRunKey = KeyCode.Q;
    public KeyCode downwardsWallRunKey = KeyCode.E;
    private bool _upwardsRunning;
    private bool _downwardsRunning;
    private float _horizontalInput;
    private float _verticalInput;

    [Header("Wall Detection Stuff")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit _leftWallHit;
    private RaycastHit _rightWallHit;
    private bool _wallLeft;
    private bool _wallRight;

    private bool _exitingWall;
    public float exitWallTime;
    private float _exitWallTimer;

    public Transform orientation;
    public PlayerCamera cam;
    private PlayerMovement _pm;
    private Rigidbody _rb;

    private void Start() {
        _rb = GetComponent<Rigidbody>();
        _pm = GetComponent<PlayerMovement>();
    }

    private void Update() {
        DetectWalls();
        HandleStates();
    }

    private void FixedUpdate() {
        if (_pm.wallRunning) {
            WallRunMovement();
        }
    }

    public void DetectWalls() {
        _wallRight = Physics.Raycast(transform.position, orientation.right, out _rightWallHit, wallCheckDistance, wallLayer);
        _wallLeft = Physics.Raycast(transform.position, -orientation.right, out _leftWallHit, wallCheckDistance, wallLayer);
    }

    // checks if the player is high enough in the air to start wallrunning
    private bool AboveGround() {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundLayer);
    }

    private void HandleStates() {
        // inputs
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        _upwardsRunning = Input.GetKey(upwardsWallRunKey);
        _downwardsRunning = Input.GetKey(downwardsWallRunKey);

        // wallrunning state
        if ((_wallLeft || _wallRight) && _verticalInput > 0 && AboveGround() && !_exitingWall) {
            // start wallrun
            if (!_pm.wallRunning)
                StartWallRun();

            if (_wallRunTimer > 0)
                _wallRunTimer -= Time.deltaTime;

            if (_wallRunTimer <= 0 && _pm.wallRunning) {
                _exitingWall = true;
                _exitWallTimer = exitWallTime;
            }

            // wall jump
            if (Input.GetKeyDown(jumpKey))
                WallJump();

        } else if (_exitingWall) {

            if (_pm.wallRunning)
                StopWallRun();

            if (_exitWallTimer > 0)
                _exitWallTimer -= Time.deltaTime;

            if (_exitWallTimer <= 0)
                _exitingWall = false;

        } else {
            if (_pm.wallRunning)
                StopWallRun();
        }
    }

    private void StartWallRun() {
        _pm.wallRunning = true;

        _wallRunTimer = maxWallRunTime;

        cam.ManageFOV(90f);
        if (_wallLeft) cam.ManageTilting(-5f);
        if (_wallRight) cam.ManageTilting(5f);
    }
    private void WallRunMovement() {
        _rb.useGravity = false;
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        Vector3 wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        _rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (_upwardsRunning)
            _rb.velocity = new Vector3(_rb.velocity.x, wallClimbSpeed, _rb.velocity.z);
        if (_downwardsRunning)
            _rb.velocity = new Vector3(_rb.velocity.x, -wallClimbSpeed, _rb.velocity.z);

        // push player towards the wall
        if (!(_wallLeft && _horizontalInput > 0) && !(_wallRight && _horizontalInput < 0))
            _rb.AddForce(-wallNormal * 100f, ForceMode.Force);
    }

    private void StopWallRun() {
        _pm.wallRunning = false;

        cam.ManageFOV(80f);
        cam.ManageTilting(0f);
    }

    private void WallJump() {
        _exitingWall = true;
        _exitWallTimer = exitWallTime;
        Vector3 wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;

        Vector3 force = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _rb.AddForce(force, ForceMode.Impulse);
    }
}
