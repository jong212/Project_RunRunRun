using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    #region Public Fields
    [Header("Mouse Settings")]
    public float SensetivityX;

    public float SensetivityY;

    public bool InvertX;

    public bool InvertY = true;
    #endregion

    #region Serialized Fields
    [Header("Depending Variables")]
    [SerializeField]
    private Transform targetTransform;

    [SerializeField] 
    private Transform cameraPivot;

    [SerializeField]
    private Transform cameraTransform;

    [SerializeField] 
    private LayerMask collisionLayers;

    [SerializeField] 
    private ThirdPersonController playerMovement;
    #endregion

    #region Private Fields
    private float followTime = 0f;
    private float mouseInputX;
    private float mouseInputY;
    private float lookAngle; //up and down angle
    private float pivotAngle; // left and right angle
    private float minPivotAngle = -35f, maxPivotAngle = 35f;
    private float defaultFOV;
    private float zoomedOutFOV;
    private float defaultPosition;
    private DemoInputControls playerControls;
    private Vector3 followVelocity = Vector3.zero;
    private Vector3 cameraVectorPosition;
    private Vector2 mouseInput;
    #endregion

    private void Awake()
    {
        //Invisible Cursor at center
        Cursor.lockState = CursorLockMode.Locked;

        //Assign variables
        defaultFOV = cameraTransform.GetComponent<Camera>().fieldOfView;

        defaultPosition = cameraTransform.localPosition.z;

        //Assign Zoomed Out FOV
        zoomedOutFOV = defaultFOV + 5f;
    }
    private void OnEnable()
    {
        //Get Input
        if (playerControls == null)
        {
            playerControls = new DemoInputControls();

            playerControls.Player.Look.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
            playerControls.Player.Look.canceled += ctx => mouseInput = ctx.ReadValue<Vector2>(); // to stop moving camera after mouse movement stops
        }

        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
    private void Update()
    {
        FollowTarget();
        HandleRotation();
        HandleZoom();
        HandleCollision();
    }
    void FollowTarget()
    {
        //Get the target position
        Vector3 targetPosition = Vector3.SmoothDamp(transform.position, targetTransform.position, ref followVelocity, followTime);

        //Set the position to target position
        transform.position = targetPosition;

        //Handle Follow Times for falling and moving (Important)
        if (!playerMovement.isGrounded)
        {
            if (followTime >= 0.01)
            {
                followTime -=  0.002f;
            }
        }
        else
        {
            followTime = 0.2f;
        }
    }
    void HandleRotation()
    {
        //Handle Regular / Inverted Input Assigning
        if (InvertX)
        {
            mouseInputX = mouseInput.x *-1;
        }
        else
        {
            mouseInputX = mouseInput.x;
        }
        if (InvertY)
        {

            mouseInputY = mouseInput.y *-1;
        }
        else
        {
            mouseInputY = mouseInput.y;
        }

        Vector3 rotation;

        //Assign rotation angles according to input
        lookAngle = lookAngle + (mouseInputX * SensetivityX);
        pivotAngle = pivotAngle + (mouseInputY * SensetivityY);
        pivotAngle = Mathf.Clamp(pivotAngle, minPivotAngle, maxPivotAngle);

        //Set rotation of camera
        rotation = Vector3.zero;
        rotation.y = lookAngle;
        transform.rotation = Quaternion.Euler(rotation);

        rotation = Vector3.zero;
        rotation.x = pivotAngle;
        cameraPivot.localRotation = Quaternion.Euler(rotation);
    }
    void HandleZoom()
    {
        //Handle zooming while sprinting
        if (playerMovement.isSprinting)
        {
            cameraTransform.GetComponent<Camera>().fieldOfView = Mathf.Lerp(cameraTransform.GetComponent<Camera>().fieldOfView, zoomedOutFOV, 0.3f);
        }
        else
        {
            cameraTransform.GetComponent<Camera>().fieldOfView = Mathf.Lerp(cameraTransform.GetComponent<Camera>().fieldOfView, defaultFOV, 0.3f);
        }
    }
    void HandleCollision()
    {
        //Set target position to default position at start
        float targetPosition = defaultPosition;

        //Set direction
        Vector3 direction = cameraTransform.position - cameraPivot.position;
        direction.Normalize();

        //Check collision
        RaycastHit hit;
        if (Physics.SphereCast(cameraPivot.transform.position, 0.2f, direction, out hit, Mathf.Abs(targetPosition), collisionLayers))
        {
            //Set targetPosition to proper position in front of object of collision
            float distance = Vector3.Distance(cameraPivot.position, hit.point);
            targetPosition = -(distance - 0.2f);
        }

        //Math to get final target position of camera
        if (Mathf.Abs(targetPosition) < 0.2f)
        {
            targetPosition -= 0.2f;
        }

        //Set camera position to targetPosition (smoothened)
        cameraVectorPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, 0.1f);
        cameraTransform.localPosition = cameraVectorPosition;
    }
}
