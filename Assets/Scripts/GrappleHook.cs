using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour
{


    public GameObject Target;
    private Transform aimTarget;

    public Transform cameraHolder;
    public GameObject hookPrefab; 

    public PigeonController pigeonController;

    public float grappleForce = 10;

    public float grappleMaxDistance = 100;

    private bool isHookInstantiated = false;

    private bool isHookFired = false;

    private GameObject hook;

    // Start is called before the first frame update
    void Start()
    {
        aimTarget = Target.transform;
    }

    // Update is called once per frame
    void Update()
    {
        Fire();
        DrawRope();
        DetectConstraints();
    }

    void FixedUpdate()
    {
        Grapple();
        
        if (isHookFired)
        {
            // Calculate the tension of the rope to make the pigeon swing :)
            Vector3 direction = Vector3.Normalize(hook.transform.position - GetGunPosition());
            float length = Vector3.Magnitude(GetGunPosition() - hook.transform.position);
            float angle = Vector3.Angle(direction, Vector3.up);
            Vector3 velocity = pigeonController.GetVelocity(); 
            
            // Calculate tension caused by gravity
            float tension = 9.81f * Mathf.Cos(angle * Mathf.Deg2Rad);

            // Calculate the centripetal force
            float centripetal = velocity.magnitude * velocity.magnitude / length;
            Vector3 force = direction * (tension + centripetal);

            if (force.magnitude > 20) {
                // Precausion to prevent weird situations / bugs
                force = force.normalized * 20;
            }
            
            pigeonController.ApplyForce(force);
        }
    }

    private void DetectConstraints()
    {
        if (isHookFired)
        {
            // Find out if something is cutting the rope
            RaycastHit hitInfo;

            Vector3 direction = (hook.transform.position - GetGunPosition()).normalized;
            float length = (GetGunPosition() - hook.transform.position).magnitude;

            if (Physics.Raycast(GetGunPosition(), direction, out hitInfo, length - 0.5f))
            {
                isHookFired = false;
                hook.SetActive(false);
            }
        }
    }

    private void DrawRope()
    {
        if (isHookFired) {
            // Draw line
            LineRenderer line = hook.GetComponent<LineRenderer>();
            Vector3[] positions = new Vector3[] { GetGunPosition(), hook.transform.position };
            
            line.SetPositions(positions);
        }
    }

    private void Grapple()
    {
        if (isHookFired && Input.GetMouseButton(1))
        {
            Vector3 direction = Vector3.Normalize(hook.transform.position - GetGunPosition());
            
            pigeonController.Move(direction * grappleForce);
        }
    }

    private void Fire()
    {
        if (Input.GetMouseButtonDown(0)) {
            isHookFired = !isHookFired;
            if (isHookFired) {
                ShootHook();
            }
            else
            {
                if (isHookInstantiated)
                {
                    hook.SetActive(false);
                }
            }
        }
    }

    private Vector3 GetGunPosition()
    {
        return new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void ShootHook()
    {
        // Find out where the hook will grab
        RaycastHit hitInfo;

        Vector3 direction = (aimTarget.position - cameraHolder.position).normalized;

        if (Physics.Raycast(cameraHolder.position + direction * 4, direction, out hitInfo, 50))
        {
            Vector3 hookPosition = hitInfo.point;

            // Instantiate hook
            if (!isHookInstantiated)
            {
                hook = Instantiate(hookPrefab, hookPosition, Quaternion.identity);
                isHookInstantiated = true;
            }
            else
            {
                hook.SetActive(true);
                hook.transform.position = hookPosition;
            }
        }
    }
}
