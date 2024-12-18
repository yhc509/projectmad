using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        public GameObject _playerTop;
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private float _verticalVelocityForce; // 외부 기믹에 의한 수직속도

        private bool _tryLedgeGrab = false;
        private bool _onLedgeGrab = false;

        public GameObject _rayHitMark;
        public Transform _rayStartTransform;
        Vector3 _ledgeMarker;
        Vector3 _rayStart;
        Vector3 _rayLedgePosition;
        public Vector3 _playerOffset = new Vector3(0.0f, -2.4f, 0.0f);
        public Quaternion _playerRotOffset = Quaternion.identity;
        public LayerMask _ledgeDetectionMask;
        RaycastHit _rayHitWall;
        RaycastHit _rayFindLedge;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDLedgeGrab;

        private PlayerInput _playerInput;
        private Animator _animator;
        public CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private CinemachineVirtualCamera _playerVirtualCamera;

        public bool _autoLedgeGrab;

        public Rigidbody _rigidBody;
        public LayerMask pushLayers;
        public bool canPush;
        [Range(0.5f, 5f)] public float strength = 1.1f;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        // backup
        private float backup_MoveSpeed;
        private float backup_SprintSpeed;
        private float backup_Gravity;
        
        private bool IsCurrentDeviceMouse
        {
            get
            {
                return _playerInput.currentControlScheme == "KeyboardMouse";
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            _playerVirtualCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();

        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _playerInput = GetComponent<PlayerInput>();

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
            
            backup_MoveSpeed = MoveSpeed;
            backup_SprintSpeed = SprintSpeed;
            backup_Gravity = Gravity;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
        }

        private void FixedUpdate()
        {
            Move();
            CameraRotation();
        }


        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDLedgeGrab = Animator.StringToHash("LedgeGrab");
        }

        // 바닥이랑 상관없
        private void OnCollisionEnter(Collision hit)
        {
            if (canPush)
            {
                // https://docs.unity3d.com/ScriptReference/CharacterController.OnControllerColliderHit.html

                // make sure we hit a non kinematic rigidbody
                Rigidbody body = hit.collider.attachedRigidbody;
                if (body == null || body.isKinematic) return;

                // make sure we only push desired layer(s)
                var bodyLayerMask = 1 << body.gameObject.layer;
                if ((bodyLayerMask & pushLayers.value) == 0) return;

                // We dont want to push objects below us
                if (hit.relativeVelocity.y < -0.3f) return;

                // Calculate push direction from move direction, horizontal motion only
                Vector3 pushDir = new Vector3(hit.relativeVelocity.x, 0.0f, hit.relativeVelocity.z);

                // Apply the push and take strength into account
                body.AddForce(pushDir * strength, ForceMode.Impulse);
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);

            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        void SetRotationToLedgeGrabTarget()
        {
            if (_onLedgeGrab)
            {

            }
        }

        void LedgeRayCast()
        {
            if (_rayStartTransform == null) return;

            if (Physics.Raycast(_rayStartTransform.position, transform.forward, out _rayHitWall, 1f, _ledgeDetectionMask))
            {
                _rayStart = _rayHitWall.point + transform.forward * 0.03f;
                _rayStart.y += 3.0f;


                if (Physics.Raycast(_rayStart, Vector3.down, out _rayFindLedge, 3.0f))
                {
                    _ledgeMarker = new Vector3(_rayHitWall.transform.position.x, _rayFindLedge.transform.position.y, _rayHitWall.transform.position.z);

                    GameObject tempMark;
                    tempMark = Instantiate(_rayHitMark, _rayFindLedge.point, Quaternion.LookRotation(_rayFindLedge.normal)) as GameObject;
                    _rayLedgePosition = tempMark.transform.position;
                    Destroy(tempMark, 0.03f);
                }

                if (_tryLedgeGrab)
                {
                    if (Vector3.Distance(_rayLedgePosition, _rayStartTransform.position) < 1.5f)
                        _onLedgeGrab = true;
                }
            }
        }

        void OnBeginLerpToLedgeGrabClimb()
        {
            _onLedgeGrab = true;

            // ground values
            MoveSpeed = 0.0f;
            SprintSpeed = 0.0f;
            Gravity = 0.0f;
            _rotationVelocity = 0.0f;
            _verticalVelocity = 0.0f;

            float backwardOffset = 0.4f;

            Vector3 hitPosition = _rayLedgePosition;//_rayFindLedge.point;
            transform.position = hitPosition + _playerOffset - transform.forward * backwardOffset;
            transform.SetPositionAndRotation(transform.position, transform.rotation * _playerRotOffset);

        }

        void OnEndLerpToLedgeGrabClimb()
        {
            _animator.SetBool(_animIDLedgeGrab, false);
            _onLedgeGrab = false;

            MoveSpeed = backup_MoveSpeed;
            SprintSpeed = backup_SprintSpeed;
            Gravity = backup_Gravity;
            Vector3 upOffset = new Vector3(0.0f, 0.05f, 0.0f);
            transform.position = _rayLedgePosition + upOffset;
        }

        private void OnDrawGizmos()
        {
            if (_rayStartTransform == null) return;

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Debug.DrawRay(_rayStartTransform.position, forward, Color.green);
            //Vector3 down = transform.TransformDirection(Vector3.down);
            //Debug.DrawRay(_rayStart, down, Color.green);

        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            if (_onLedgeGrab)
                return;

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            if (_input.move != Vector2.zero || !Grounded || _input.jump)
            { 
                // move the player
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            _tryLedgeGrab = _autoLedgeGrab || (Input.GetKeyDown(KeyCode.Space) && _animator.GetBool(_animIDJump) && Grounded == false);

            if (_verticalVelocityForce > 0f)
            {
                _verticalVelocity = _verticalVelocityForce;
                _verticalVelocityForce = 0f;
            }

            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < -10.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if ((_input.jump && _jumpTimeoutDelta <= 0.0f))
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                LedgeRayCast();

                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;

                //      ׷  ϴ     ߿         ־    
                if (_onLedgeGrab)
                {
                    _speed = 0.0f;
                    _rotationVelocity = 0.0f;
                    _verticalVelocity = 0.0f;
                    Gravity = 0.0f;
                    _animator.SetBool(_animIDLedgeGrab, true);
                }
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        void OffLedgeGrab()
        {

        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f && LandingAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public void SetVerticalVelocity(float addValue)
        {
            _verticalVelocityForce += addValue;
        }
    }
}
