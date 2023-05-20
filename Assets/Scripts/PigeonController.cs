using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PigeonController : MonoBehaviour
{

    public CharacterController characterController;
    public float speed = 3;
    

    public Animator animator;
    
    // camera and rotation
    public Transform cameraHolder;
    public float mouseSensitivity = 2f;
    public float upLimit = -50;
    public float downLimit = 50;
    
    public float lyft = 0.9f;
    // gravity
    private float gravity = 9.81f;
    private float verticalSpeed = 0;


    private bool isWalking = false;
    private bool isFlying = false;
    private bool isJumping = false;

    public GameObject Target;
    private Transform aimTarget;

    private float _aimTargetYaw;
    private float _aimTargetPitch;
    
    // Start is called before the first frame update
    void Start()
    {
        aimTarget = Target.transform;
    }

    void Update()
    {
        Move();
        Rotate();
        MoveCamera();
        SetAnimate();
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Rotate()
    {
        float horizontalRotation = Input.GetAxis("Mouse X");
        float verticalRotation = Input.GetAxis("Mouse Y");

        _aimTargetPitch = _aimTargetPitch - verticalRotation * mouseSensitivity;
        _aimTargetYaw = _aimTargetYaw + horizontalRotation * mouseSensitivity;

        _aimTargetYaw = ClampAngle(_aimTargetYaw, float.MinValue, float.MaxValue);
        _aimTargetPitch = ClampAngle(_aimTargetPitch, upLimit, downLimit);

        aimTarget.rotation = Quaternion.Euler(_aimTargetPitch, _aimTargetYaw, 0.0f);
        aimTarget.position = transform.position + aimTarget.forward * 4;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    
    private void MoveCamera() 
    {
        
        cameraHolder.transform.SetLocalPositionAndRotation(aimTarget.localPosition - aimTarget.forward * 8 + transform.up * 2, aimTarget.localRotation);
        
        cameraHolder.transform.LookAt(aimTarget, Vector3.up);
    }

    private void SetAnimate()
    {
        if (isFlying)
        {
            animator.speed = 1f;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.Play("Fly");
        }
        else
        {
            if (isJumping)
            {
                animator.speed = 1f;
            }
            else {
                if (isWalking)
                {
                    animator.speed = speed / 1.2f;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animator.Play("Walk");
                } else {
                    animator.speed = 0.5f;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animator.Play("Idle_A");
                }
            }
        }
        
    }

    private void Move()
    {
        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");

        bool space = Input.GetKey(KeyCode.Space);

        isFlying = false;
        if (characterController.isGrounded)
        {
            isJumping = false;
            verticalSpeed = 0;
            if (space)
            {
                verticalSpeed += 5;
                isJumping = true;
                animator.cullingMode = AnimatorCullingMode.CullCompletely;
                animator.Play("Jump");
            }
        }
        else 
        {
            float downward = gravity * Time.deltaTime;
            if (space) {
                if (verticalSpeed < 0)
                {
                    downward -= gravity * lyft * Time.deltaTime;
                    isFlying = true;
                }

            }
            verticalSpeed -= downward;
            
        }
        Vector3 gravityMove = new Vector3(0, verticalSpeed, 0);
        
        Vector3 move = aimTarget.forward * verticalMove + aimTarget.right * horizontalMove;
        
        characterController.Move(speed * Time.deltaTime * move + gravityMove * Time.deltaTime);
        
        isWalking = verticalMove != 0 || horizontalMove != 0;
        
        if (isWalking) {
            // The step size is equal to speed times frame time.
            float singleStep = 10 * Time.deltaTime;

            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, move, singleStep, 0.0f);

            // Draw a ray pointing at our target in
            Debug.DrawRay(transform.position, newDirection, Color.red);

            // Calculate a rotation a step closer to the target and applies rotation to this object
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }
}
