using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PigeonController : MonoBehaviour
{

    public CharacterController characterController;
    public float walkSpeed = 6;
    public float runSpeed = 12;
    public float jumpForce = 200;

    // Haha
    public float lyft = 0.9f;

    public Animator animator;
    
    // camera and rotation
    public Transform cameraHolder;
    public float mouseSensitivity = 2f;
    public float upLimit = -50;
    public float downLimit = 50;

    public float cameraDistance = 3;

    // gravity
    private Vector3 gravity = new Vector3(0, -9.81f, 0);

    private Vector3 velocity;
    private Vector3 acceleration;
    

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
        CleanUp();
    }

    void Update()
    {
        HandleJumping();
        HandleFlying();
        HandleMovement();
        ApplyGravity();
        Move();
        Rotate();
        MoveCamera();
        SetAnimate();
        CleanUp();
    }

    private void CleanUp()
    {
        acceleration *= 0;
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
        aimTarget.position = transform.position + aimTarget.forward * cameraDistance;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    
    private void MoveCamera() 
    {
        cameraHolder.transform.SetLocalPositionAndRotation(aimTarget.localPosition - aimTarget.forward * cameraDistance * 2 + transform.up * 2, aimTarget.localRotation);
        
        cameraHolder.transform.LookAt(aimTarget, Vector3.up);
        
        if (cameraHolder.transform.position.y < transform.position.y + 0.1f)
        {
            
            cameraHolder.transform.position = new Vector3(cameraHolder.transform.position.x, transform.position.y + 0.1f, cameraHolder.transform.position.z);
        }
    }

    private void SetAnimate()
    {
        if (isFlying)
        {
            animator.speed = 1f;
            animator.Play("Fly");
        }
        else
        {
            if (isJumping)
            {
                animator.speed = 1f;
                animator.Play("Jump");
            }
            else 
            {
                if (isWalking)
                {
                    float walkSpeed = velocity.magnitude;
                    animator.speed = walkSpeed / 10;
                    animator.Play("Walk");
                } else {
                    animator.speed = 0.5f;
                    animator.Play("Idle_A");
                }
            }
        }
    }

    public void ApplyForce(Vector3 force, bool applyDeltaTime = true)
    {
        acceleration += applyDeltaTime ? force * Time.deltaTime : force;
    }

    public void ApplyForce(float x, float y, float z, bool applyDeltaTime = true)
    {
        ApplyForce(new Vector3(x, y, z), applyDeltaTime);
    }

    private void HandleJumping()
    {
        if (characterController.isGrounded)
        {
            isJumping = false;
            if (Input.GetKey(KeyCode.Space))
            {
                ApplyForce(0, jumpForce, 0, false);
                isJumping = true;
            }
        }
    }

    private void ApplyGravity()
    {
        ApplyForce(gravity);
    }

    private void HandleFlying()
    {
        isFlying = false;
        if (!characterController.isGrounded) 
        {
            if (Input.GetKey(KeyCode.Space)) {
                if (velocity.y < 0)
                {
                    ApplyForce(0, lyft * -gravity.y, 0);
                    isFlying = true;
                }
            }
        }
    }

    private void HandleMovement()
    {
        if (characterController.isGrounded)
        {
            float horizontalMove = Input.GetAxis("Horizontal");
            float verticalMove = Input.GetAxis("Vertical");
            Vector3 groundMove = XZPlane(aimTarget.forward) * verticalMove + aimTarget.right * horizontalMove;
            isWalking = verticalMove != 0 || horizontalMove != 0;

            ApplyForce(groundMove * walkSpeed);

            if (isWalking) {
                // The step size is equal to speed times frame time.
                float singleStep = 10 * Time.deltaTime;

                // Rotate the forward vector towards the target direction by one step
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, groundMove, singleStep, 0.0f);

                // Draw a ray pointing at our target in
                Debug.DrawRay(transform.position, newDirection, Color.red);

                // Calculate a rotation a step closer to the target and applies rotation to this object
                transform.rotation = Quaternion.LookRotation(newDirection);
            }
        }
    }

    private void Move()
    {
    
        if (characterController.isGrounded && velocity.y < 0)
        {
            ApplyForce(0, -(velocity.y + acceleration.y), 0);
        }

        velocity += acceleration;
        
        characterController.Move(velocity * Time.deltaTime);
    }

    public Vector3 XZPlane(Vector3 vec)
    {
        return new Vector3(vec.x,0,vec.z);
    }
}
