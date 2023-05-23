using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float speed = 0;
    public float maxSpeed = 3;
    public float acceleration = 1;
    private bool moving = false;

    private float steering = 0;

    private int currentWayPoint = 0;

    public GameObject[] Waypoints;

    private List<GameObject> frontWheels;
    private List<GameObject> rearWheels;
    private List<GameObject> allWheels;

    // Start is called before the first frame update
    void Start()
    {
        frontWheels = new List<GameObject>();
        rearWheels = new List<GameObject>();
        allWheels = new List<GameObject>();
        foreach (Transform child in transform)
        {

            WheelCollider wheelCollider = child.GetComponent<WheelCollider>();
            if (wheelCollider != null)
            {
                Vector3 relativePos = child.transform.position - transform.position;
                bool isInFront = Vector3.Dot(transform.forward, relativePos) > 0.0f;
                if (isInFront)
                {
                    frontWheels.Add(child.gameObject);
                }
                else
                {
                    rearWheels.Add(child.gameObject);
                }
                allWheels.Add(child.gameObject);
            }
        }
    }

    public void StartMoving()
    {
        moving = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            StartMoving();
        }

        float carVelocity = this.GetComponent<Rigidbody>().velocity.magnitude;
        if (moving)
        {
            speed += acceleration * Time.deltaTime;
            if (speed > maxSpeed)
            {
                speed = maxSpeed;
            }
            
            if (Waypoints.Length > 0)
            {
                TurnTowardsWaypoint();

                // Follow waypoints
                float distance = Vector3.Distance(Waypoints[currentWayPoint].transform.position, this.transform.position);
                if (distance > 8)
                {
                    
                    // Move towards the waypoint
                    if (carVelocity < maxSpeed) { 
                        //Debug.Log("Distance: " + distance + " Velocity: " + carVelocity + " - Accelerating");
                        ApplyTorque();
                    }
                }
                else if (distance > 2)
                {
                    // Slow down
                    //ApplyTorque();
                    if (carVelocity > 3)
                    {
                        ApplyBrakes();
                        //Debug.Log("Distance: " + distance + " Velocity: " + carVelocity + " - Slowing down");
                    }
                        
                    else ApplyTorque();
                }
                else
                {
                    // Start going towards next waypoint
                    currentWayPoint++;
                    if (Waypoints.Length == currentWayPoint)
                    {
                        currentWayPoint = 0;
                    }
                }
            }
            else
            {
                // Just go forward
                ApplyTorque();
            }
        }

        
        TurnWheels();

        //DetectIntersection();
    }

    /*private void DetectIntersection()
    {
        foreach (GameObject wheel in wheels)
        {
            WheelCollider wheelCollider = wheel.GetComponent<WheelCollider>();
            if (wheelCollider.GetGroundHit(out WheelHit hit)) {
                if (hit.collider.gameObject.CompareTag("Intersection")) {
                    hit.collider.
                    TurnCar();
                };
            }
        }
    }*/

    private void TurnTowardsWaypoint()
    {
        Vector3 relativePos = Waypoints[currentWayPoint].transform.position - transform.position;
        float angle = Vector3.SignedAngle(relativePos, transform.forward, transform.up);

        if (Mathf.Abs(-steering - angle) < 2)
        {
            steering = -angle;
        }
        else
        {
            if (angle < 0)
                steering += 30 * Time.deltaTime;
            else
                steering -= 30 * Time.deltaTime;
        }
            
        if (steering > 30) steering = 30;
        if (steering < -30) steering = -30;
        foreach (GameObject wheel in frontWheels)
        {
            WheelCollider wheelCollider = wheel.GetComponent<WheelCollider>();
            wheelCollider.steerAngle = steering;
        }
    }

    private void ApplyTorque()
    {
        foreach (GameObject wheel in frontWheels)
        {
            WheelCollider wheelCollider = wheel.GetComponent<WheelCollider>();
            wheelCollider.motorTorque = speed * 200;
            wheelCollider.brakeTorque = 0;
        }
    }

    private void ApplyBrakes()
    {
        foreach (GameObject wheel in frontWheels)
        {
            WheelCollider wheelCollider = wheel.GetComponent<WheelCollider>();
            wheelCollider.brakeTorque = 2;
            wheelCollider.motorTorque = 0;
        }
    }

    private void TurnWheels()
    {
        foreach (GameObject wheel in allWheels)
        {
            WheelCollider wheelCollider = wheel.GetComponent<WheelCollider>();
            
            Transform visualWheel = wheel.transform.GetChild(0);
     
            Vector3 position;
            Quaternion rotation;
            
            wheelCollider.GetWorldPose(out position, out rotation);
        
            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation;
        }
    }

    // Upon collision with another GameObject, this GameObject will reverse direction
    /*void OnCollisionEnter(Collision collision)
    {
        transform.Rotate(0, 180, 0);
    }*/
}
