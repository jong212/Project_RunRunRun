using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;

public class ThirdPersonController : MonoBehaviourPun
{
    #region Private Fields
    private CharacterController controller;
    private PhotonView PV;  // PhotonView 변수 추가
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject freeLookCamera;
    private DemoInputControls playerInputControls;

    private Vector2 moveInput;

    private Vector3 velocity; //for gravity and jump

    private Vector3 moveDirection;

    private bool sprintInput;

    private bool crouchInput;

    private bool jumpInput;

    private float turnSmoothTime = 0.1f;

    private float turnSmoothVelocity; //this is just to hold the turning velocity when the player turns to face where its moving
    #endregion

    #region Public Fields
    [Header("Move Amount (Do Not Edit)")]
    public float moveAmount;
    
    [Header("Player State Booleans (Do Not Edit)")]
    public bool isGrounded;

    public bool isSprinting;

    public bool isJumping;

    public bool isCrouching;

    [Header("Crouch Options")]
    public bool holdToCrouch;

    public bool pressToCrouch;
    #endregion

    #region Serialized Fields
    [Header("Move Speeds")]
    [SerializeField] 
    private float moveSpeed;

    [SerializeField] 
    private float walkSpeed;

    [SerializeField] 
    private float sprintSpeed;

    [SerializeField] 
    private float crouchSpeed;

    [Header("Jump & Gravity Variables")]
    [SerializeField] 
    private float gravity = -9.81f;

    [SerializeField] 
    private float jumpHeight = 4f;

    [SerializeField] 
    private LayerMask groundMask;

    [Header("Other")]
    [SerializeField] 
    private Transform PlayerCamera;

    [SerializeField] 
    private Transform characterBase;


    [SerializeField] 
    private Animator characterAnimator;
    #endregion

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        PV = GetComponent<PhotonView>();  // PhotonView 초기화

        // 카메라 설정
        if (!PV.IsMine)
        {
            // 다른 플레이어의 카메라는 비활성화
            mainCamera.SetActive(false);
            freeLookCamera.SetActive(false);
        }
        else
        {
            // 자신의 카메라는 활성화
            mainCamera.SetActive(true);
            freeLookCamera.SetActive(true);
        }
    }
    private void OnEnable()
    {
        //Take Input
        if (playerInputControls == null)
        {
            playerInputControls = new DemoInputControls();

            playerInputControls.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            playerInputControls.Player.Movement.canceled += ctx => moveInput = ctx.ReadValue<Vector2>(); //to not store input when we let go of the WASD / joystick

            playerInputControls.Player.Jump.performed += ctx => jumpInput = true;

            playerInputControls.Player.Sprint.performed += ctx => sprintInput = true;
            playerInputControls.Player.Sprint.canceled += ctx => sprintInput = false;

            playerInputControls.Player.Crouch.started += ctx => crouchInput = true;
            playerInputControls.Player.Crouch.canceled += ctx => crouchInput = false;
        }
        playerInputControls.Enable();
    }
    private void OnDisable()
    {
        playerInputControls.Disable();
    }
    void Update()
    {
        if (PV.IsMine)  // 자신이 소유한 객체일 때만 업데이트
        {
            SimpleMovement();
            JumpingAndGravity();
            AnimationHandler();
            CrouchHandler();
        }
    }
    private void SimpleMovement()
    {
        //Assign Input Values
        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        //Get Move Direction
        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput).normalized;

        if (direction.magnitude >= 0.1) //if there is any input to move
        {
            //Get Rotation Angle
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + PlayerCamera.eulerAngles.y; //to get the angle where our player should be looking at while moving. Atan 2 gives the angle bet. current rotation and the the angle we want, from direction.x to direction.z
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);


            moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward; //this is to actually move in the direction where the player rotates, not just face that way
            moveDirection.Normalize();
            
            //Handle Sprinting
            if (sprintInput && moveAmount > 0.5f && isGrounded && !isCrouching)
            {
                moveDirection *= sprintSpeed;
                isSprinting = true;
            }
            //Handle Crouching
            else if (isCrouching)
            {
                if (moveAmount > 0.2f)
                {
                    moveDirection *= crouchSpeed;
                    moveAmount = 1f;
                    controller.height = 1.5f;
                    controller.center = new Vector3(0, -0.25f, 0);
                }
                else
                {
                    moveDirection *= 0;
                    moveAmount = 0f;
                }
                isSprinting = false;
            }
            //Handle Regular Movement
            else
            {
                isSprinting = false;
                //Normal Movement
                if (moveAmount >= 0.5f)
                {
                    moveDirection *= moveSpeed;
                }
                //Walking
                else
                {
                    moveDirection *= walkSpeed;
                    moveAmount = .4f;
                }
            }
            
            //Move the player
            controller.Move(moveDirection * Time.deltaTime);

        }
        else
        {
            //Toggle is sprinting when we stop pressing any key / afk
            isSprinting = false;
        }
    }
    private void JumpingAndGravity()
    {
        //Check if the player is on ground / in air
        isGrounded = Physics.CheckSphere(characterBase.position, 0.5f, groundMask);

        //Reset velocity.y in order to prevent the velocity from building up even when we are on ground, to prevent increasing gravity
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        //Apply Gravity to velocity.y
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime); //Apply gravity to player

        //Handle Jumping
        if (jumpInput && !isCrouching)
        {
            jumpInput = false;
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                characterAnimator.SetBool("Jumping", true);
                characterAnimator.CrossFade("Jumping", 0.2f);
            }   
        }
    }
    private void AnimationHandler()
    {
        //Set the animator.isGrounded bool
        characterAnimator.SetBool("isGrounded", isGrounded);

        //Assign animator bools to local variables
        bool Jumping = characterAnimator.GetBool("Jumping");
        bool Grounded = characterAnimator.GetBool("isGrounded");

        //Play falling animation if not jumping and not on ground
        if (!Jumping && !Grounded)
        {
            characterAnimator.CrossFade("Falling", 0.2f * Time.deltaTime);
        }

        //Play sprint animation, as the sprinting animation in the blend tree plays if "Vertical" reaches 2
        if (isSprinting)
        {
            characterAnimator.SetFloat("Vertical", 2f, 0.1f, Time.deltaTime);
        }
        //Play idle/walk/move animation as moveAmount is how much input we get
        else
        {
            characterAnimator.SetFloat("Vertical", moveAmount, 0.1f, Time.deltaTime);
        }

        //Set the animator.isCrouching bool
        characterAnimator.SetBool("isCrouching", isCrouching);
    }
    void CrouchHandler()
    {
        //if we are continuously pressing the crouch key
        if (holdToCrouch)
        {
            isCrouching = crouchInput;
        }

        //if we want to toggle crouch (default)
        if (pressToCrouch)
        {
            if (!isCrouching && crouchInput)
            {
                crouchInput = false;
                isCrouching = true;
            }
            if (isCrouching && crouchInput)
            {
                crouchInput = false;
                isCrouching = false;
            }
            if (isCrouching && jumpInput)
            {
                isCrouching = false;
            }
        }

        //Set the character controller height when crouching, so that we can go through places we can only go through if we crouch
        if (isCrouching && moveAmount < 0.2f)
        {
            controller.height = 1.3f;
            controller.center = new Vector3(0, -0.35f, 0);
        }
        if (!isCrouching)
        {
            controller.height = 1.8f;
            controller.center = new Vector3(0, -0.13f, 0);    
        }
    }
}