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
    private Vector3 absoluteMovement;
    

    private bool rKeyPressed = false;
    private bool isWalking = false;
    private bool isFlying = false;
    private bool isJumping = false;

    public GameObject Target;
    private Transform aimTarget;

    private float _aimTargetYaw;
    private float _aimTargetPitch;

    private bool isGrounded;

    private CarController controlledCar;
    private bool controllingCar = false;

    
    // Start is called before the first frame update
    void Start()
    {
        aimTarget = Target.transform;
        CleanUp();
    }

    void Update()
    {
        Rotate();
        MoveCamera();
        SetAnimate();
    }

    void FixedUpdate()
    {
        if (controllingCar)
        {
            FollowCar();

            if (!rKeyPressed && Input.GetKey(KeyCode.R))
            {
                rKeyPressed = true;
                controlledCar.ReleaseControl();
                controllingCar = false;
                characterController.detectCollisions = true;
            }
        }
        else
        {
            HandleFlying();
            HandleMovement();
            ApplyGravity();
            HandleJumping();

            velocity += acceleration;
            
            characterController.Move((velocity + absoluteMovement) * Time.deltaTime);

            // Use a more reliable ground check
            isGrounded = false;
            if (Physics.Raycast(transform.position + characterController.center, Vector3.down, out RaycastHit hitInfo, characterController.height / 2 + 0.1f))
            {
                isGrounded = true;
                velocity *= 0;
                isJumping = false;
    
                CheckCarCollision(hitInfo);
            }
        }

        if (!Input.GetKey(KeyCode.R))
        {
            rKeyPressed = false;
        }
        
        CleanUp();
    }

    private void FollowCar()
    {
        Vector3 carVelocity = controlledCar.GetComponentInParent<Rigidbody>().velocity;
        velocity = carVelocity;

        //characterController.SimpleMove(velocity * Time.deltaTime);
        gameObject.transform.position += velocity * Time.deltaTime;
    }

    private void CheckCarCollision(RaycastHit hitInfo)
    {
        if (hitInfo.collider.gameObject.name.Contains("Car"))
        {
            Vector3 carVelocity = hitInfo.collider.gameObject.GetComponentInParent<Rigidbody>().velocity;
            velocity = carVelocity;

            if (!rKeyPressed && Input.GetKey(KeyCode.R))
            {
                Debug.Log("Taking control of car");
                controlledCar = hitInfo.collider.gameObject.GetComponentInParent<CarController>();
                if (controlledCar != null)
                {
                    rKeyPressed = true;
                    controlledCar.TakeControl();
                    characterController.detectCollisions = false;
                    transform.position = controlledCar.gameObject.transform.position;
                    controllingCar = true;
                    isJumping = false;
                    isWalking = false;
                    isFlying = false;
                }
                else
                {
                    Debug.Log("CarController not found");
                }
            }
        }
    }

    public void Move(Vector3 movement)
    {
        absoluteMovement += movement;
    }

    private void CleanUp()
    {
        acceleration *= 0;
        absoluteMovement *= 0;
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
                animator.Play("Swim");
            }
            else 
            {
                if (isWalking)
                {
                    animator.speed = 1;
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

    public Vector3 GetVelocity()
    {
        return velocity;
    }
    
    private void HandleJumping()
    {
        if (isGrounded)
        {
            if (Input.GetKey(KeyCode.Space) && !isJumping)
            {
                ApplyForce(0, jumpForce, 0, false);
                isJumping = true;
                isGrounded = false;
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
        if (!isGrounded) 
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
        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");
        isWalking = verticalMove != 0 || horizontalMove != 0;
        Vector3 groundMove = (XZPlane(aimTarget.forward) * verticalMove + aimTarget.right * horizontalMove) * walkSpeed;
        
        Move(groundMove);

        if (isWalking) {
            // The step size is equal to speed times frame time.
            float singleStep = 10 * Time.deltaTime;

            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, absoluteMovement, singleStep, 0.0f);

            // Draw a ray pointing at our target in
            // Debug.DrawRay(transform.position, newDirection, Color.red);

            // Calculate a rotation a step closer to the target and applies rotation to this object
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }

    private void Move()
    {
        
    }

    public Vector3 XZPlane(Vector3 vec)
    {
        return new Vector3(vec.x,0,vec.z);
    }
}
