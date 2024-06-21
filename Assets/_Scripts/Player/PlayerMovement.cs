using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public bool canSprint = true;
    public bool canCrouch = true;

    [Header("Movement Stuff")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float maxSlideSpeed;
    public float wallRunSpeed;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;
    public float groundDrag;
    private float _desiredMoveSpeed;
    private float _lastDesiredMoveSpeed;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool _readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float _startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer;
    private bool _isGrounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit _slopeHit;
    private bool _exitingSlope;

    [Header("Audio Stuff")]
    public AudioClip jumpClip;

    [Header("Camera Options")]
    public float normalFov;
    public float sprintFov;
    public float slideFov;
    public float wallRunFov;
    public float wallRunTilt;

    [Header("UI")]
    [SerializeField] private TMP_Text speedText;

    [Header("Misc")]
    public Transform orientation;

    private float _horizontalInput;
    private float _verticalInput;

    private Vector3 _moveDirection;
    private PlayerCamera _cam;
    private Rigidbody _rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
        air
    }

    public bool sliding;
    public bool wallRunning;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _cam = Camera.main.GetComponent<PlayerCamera>();
        _rb.freezeRotation = true;

        _readyToJump = true;

        _startYScale = transform.localScale.y;
        _cam.fov(normalFov);
    }

    private void Update()
    {
        // ground check
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

        HandleInputs();
        SpeedControl();
        StateHandler();

        // speed text
        speedText.text = "Speed: " + _rb.velocity.magnitude.ToString("00.00") + " b/s";

        // handle drag
        if (_isGrounded)
            _rb.drag = groundDrag;
        else
            _rb.drag = 0;

        // reload the scene
        if (Input.GetKeyDown(KeyCode.R)) {
            ReloadScene();
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInputs()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if(Input.GetKey(jumpKey) && _readyToJump && _isGrounded)
        {
            _readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (canCrouch) {
            if (Input.GetKeyDown(crouchKey)) {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }

            // stop crouch
            if (Input.GetKeyUp(crouchKey)) {
                transform.localScale = new Vector3(transform.localScale.x, _startYScale, transform.localScale.z);
            }
        }
    }

    private void StateHandler()
    {
        if (wallRunning) {
            state = MovementState.wallrunning;
            _desiredMoveSpeed = wallRunSpeed;
        }

        // Mode - Sliding
        if (sliding)
        {
            state = MovementState.sliding;
            _cam.fov(slideFov);

            if (OnSlope() && _rb.velocity.y < 0.1f)
                _desiredMoveSpeed = maxSlideSpeed;

            else
                _desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Crouching
        else if (Input.GetKey(crouchKey) && canCrouch)
        {
            state = MovementState.crouching;
            _desiredMoveSpeed = crouchSpeed;
            _cam.fov(normalFov);
        }

        // Mode - Sprinting
        else if(_isGrounded && Input.GetKey(sprintKey) && canSprint)
        {
            state = MovementState.sprinting;
            _desiredMoveSpeed = sprintSpeed;
            _cam.fov(sprintFov);
        }

        // Mode - Walking
        else if (_isGrounded)
        {
            state = MovementState.walking;
            _desiredMoveSpeed = walkSpeed;
            _cam.fov(normalFov);
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
            // _cam.fov(normalFov);
        }

        // check if desiredMoveSpeed has changed drastically
        if(Mathf.Abs(_desiredMoveSpeed - _lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = _desiredMoveSpeed;
        }

        _lastDesiredMoveSpeed = _desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(_desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, _desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = _desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        // calculate movement direction
        _moveDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;

        // on slope
        if (OnSlope() && !_exitingSlope)
        {
            _rb.AddForce(GetSlopeMoveDirection(_moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (_rb.velocity.y > 0)
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if(_isGrounded)
            _rb.AddForce(_moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if(!_isGrounded)
            _rb.AddForce(_moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        _rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !_exitingSlope)
        {
            if (_rb.velocity.magnitude > moveSpeed)
                _rb.velocity = _rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        _exitingSlope = true;

        // reset y velocity
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        SoundFXManager.instnace.PlaySoundAtPosition(jumpClip, transform.position, 1f);
    }
    private void ResetJump()
    {
        _readyToJump = true;

        _exitingSlope = false;
    }

    private void ReloadScene() {
        int currentSceneIndex =  SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out _slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;
    }
}