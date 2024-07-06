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
    private PhotonView PV;  // PhotonView ���� �߰�
    public GameObject lobbyObject;
    public GameObject gameobj;
    public GameObject freeLook;
    private DemoInputControls playerInputControls;
    public float pushForce = 30.0f; // �浹 �� ���� ���� ũ��
    [SerializeField] float crt_CenterY = 0.02f;
    [SerializeField] float crt_Height = 1.22f;


    private Vector2 moveInput;

    private Vector3 velocity; //for gravity and jump

    private Vector3 moveDirection;

    private bool sprintInput;

    private bool crouchInput;

    private bool jumpInput;

    private float turnSmoothTime = 0.1f;
    private Rigidbody rb;
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

    private const int MaxJumpCount = 2;
    [SerializeField]
    private int currentJump = 0;
    #endregion

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        PV = GetComponent<PhotonView>();  // PhotonView �ʱ�ȭ
        rb = GetComponent<Rigidbody>(); // Rigidbody �ʱ�ȭ

        if (rb != null)
        {
            rb.isKinematic = true; // Rigidbody�� ĳ���� ��Ʈ�ѷ��� �Բ� ����ϱ� ���� isKinematic ����
        }
    }
    private void Start()
    {

        if (PhotonNetwork.InLobby)
        {
            Cursor.lockState = CursorLockMode.None;
            lobbyObject.SetActive(true);
            gameobj.SetActive(false);

        }
        else
        {
            if (PV.IsMine) // �̰� ī�޶� �ΰ� �� �⺻ ������ OFF�� �Ǿ��־ ���ֱ� �ؾ���
            {
                lobbyObject.SetActive(true);
                gameobj.SetActive(false);
            }
            else
            {
                if (!PV.IsMine)
                {
                    lobbyObject.SetActive(false);
                    freeLook.SetActive(false);
                }
            }


            // TO DO ��� �����ϰ� ���� ���� �� �̰ɷ� �ؾ��ҵ�
            /*     if (PV.IsMine)
                 {
                     lobbyObject.SetActive(false);
                     GamesObject.SetActive(true);
                 } else
                 {
                     lobbyObject.SetActive(false);
                     GamesObject.SetActive(false);
                 }*/

        }
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Obstacle"))
        {
            Vector3 pushDirection = transform.position - hit.transform.position;
            pushDirection.y = 0; // ���� �������θ� ���� ����
            pushDirection.Normalize();

            rb.isKinematic = false; // �浹 �� isKinematic ����
            rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);

            controller.enabled = false; // CharacterController ��Ȱ��ȭ

            // ���� �ð� �Ŀ� �ٽ� kinematic���� �����ϰ� CharacterController Ȱ��ȭ
            StartCoroutine(ResetController());
        }
    }

    private IEnumerator ResetController()
    {
        yield return new WaitForSeconds(0.5f); // 0.5�� �Ŀ� �ٽ� kinematic���� ����
        rb.isKinematic = true;
        controller.enabled = true; // CharacterController Ȱ��ȭ
    }

   
    private void OnEnable()
    {
        //Take Input
        if (playerInputControls == null)
        {
            playerInputControls = new DemoInputControls();

            playerInputControls.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            playerInputControls.Player.Movement.canceled += ctx => moveInput = Vector2.zero; //to not store input when we let go of the WASD / joystick

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
        if (PV.IsMine || PhotonNetwork.InLobby)  // �ڽ��� ������ ��ü�� ���� ������Ʈ
        {
            SimpleMovement();
            JumpingAndGravity();
            AnimationHandler();
            CrouchHandler();
        }
    }
    private void SimpleMovement()
    {
        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + PlayerCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            moveDirection.Normalize();

            if (sprintInput && moveAmount > 0.5f && isGrounded && !isCrouching)
            {
                moveDirection *= sprintSpeed;
                isSprinting = true;
            }
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
            else
            {
                isSprinting = false;
                if (moveAmount >= 0.5f)
                {
                    moveDirection *= moveSpeed;
                }
                else
                {
                    moveDirection *= walkSpeed;
                    moveAmount = .4f;
                }
            }
        }
        else
        {
            moveDirection = Vector3.zero;
            isSprinting = false;
        }

        controller.Move(moveDirection * Time.deltaTime);
    }

    private void JumpingAndGravity()
    {
        // Check if the player is on ground / in air
        isGrounded = Physics.CheckSphere(characterBase.position, 0.5f, groundMask);

        // Reset velocity.y in order to prevent the velocity from building up even when we are on ground, to prevent increasing gravity
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            currentJump = 0;  // Reset the jump counter when the player is grounded
            characterAnimator.SetInteger("JumpCount", currentJump); // Reset JumpCount in animator
            //characterAnimator.SetBool("Jumping", false);
        }

        // Apply Gravity to velocity.y
        velocity.y += gravity * Time.deltaTime;

        controller.Move((moveDirection + new Vector3(0, velocity.y, 0)) * Time.deltaTime); // ���� �̵��� ���� �ӵ��� ���ÿ� ����

        // Handle Jumping
        if (jumpInput && !isCrouching)
        {
            if (isGrounded || currentJump < MaxJumpCount)
            {
                currentJump++;
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                characterAnimator.SetInteger("JumpCount", currentJump); // Set JumpCount in animator
                characterAnimator.SetBool("Jumping", true);
                if (currentJump == 1)
                {
                    characterAnimator.CrossFade("Jumping", 0.2f);
                }
                else if (currentJump == 2)
                {
                    characterAnimator.SetTrigger("DoubleJumping");
                    characterAnimator.CrossFade("DoubleJumping", 1.2f);
                }
            }
            jumpInput = false; // Immediately reset jump input after processing
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
            controller.height = crt_Height;
            controller.center = new Vector3(0, crt_CenterY, 0);
        }
    }
}