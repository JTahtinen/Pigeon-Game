using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public float maxSpeed = 3;
    private bool moving = false;
    private bool playerControlled = false;

    private float steering = 0;

    private int currentWayPoint = 0;

    public GameObject[] Waypoints;

    private List<GameObject> frontWheels;
    private List<GameObject> rearWheels;
    private List<GameObject> allWheels;

    private GameObject[] spawnPoints;

    private bool waitingForSpawn = false;

    private Rigidbody rigidbody;


    // Start is called before the first frame update
    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();

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

        spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
    }

    public void TakeControl()
    {
        playerControlled = true;
        moving = false;
    }

    public void ReleaseControl()
    {
        playerControlled = false;
        moving = true;
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

        if (moving)
        {
            if (Waypoints.Length > 0)
            {
                FollowWaypoints();
            }
            else
            {
                // No waypoints. Just go forward
                float carVelocity = rigidbody.velocity.magnitude;
                if (carVelocity < maxSpeed * 2) { 
                    ApplyTorque();
                }
                else
                {
                    ReleaseTorque();
                }
            }
            Steer();
        }

        if (playerControlled)
        {
            float turning = Input.GetAxis("Horizontal");
            float acceleration = Input.GetAxis("Vertical");

            if (acceleration >= 0)
            {
                ApplyTorque(acceleration);
            }
            else
            {
                Vector3 carVelocity = GetComponent<Rigidbody>().velocity;
                if (carVelocity.magnitude > 0.5f)
                {
                    ApplyBrakes(acceleration);
                }
                else
                {
                    ApplyTorque(acceleration);
                }
                
            }

            if (turning == 0)
            {
                if (steering > 1)
                {
                    turning = -1;
                }
                else if (steering < 1)
                {
                    turning = 1;
                }
                else
                {
                    steering = 0;
                    turning = 0;
                }
            }
            
            steering += turning * 100 * Time.deltaTime;

            Steer();
        }

        TurnWheels();

        if (waitingForSpawn)
        {
            TryToSpawnCar();
        }
    }

    private void TryToSpawnCar()
    {
        GameObject spawnPoint = null;
        int retries = 0;
        while (spawnPoint == null)
        {
            int spawnIndex = Random.Range (0, spawnPoints.Length);
            spawnPoint = spawnPoints[spawnIndex];
            if (spawnPoint.GetComponent<CarSensor>().isOccupied)
            {
                spawnPoint = null;
            }
            retries++;
            if (retries > 30)
            {
                Debug.Log("No free spawn point found!!!");
                break;
            }
        }
        if (spawnPoint != null)
        {
            float carVelocity = rigidbody.velocity.magnitude;
            Quaternion rotation = spawnPoint.transform.parent.rotation * Quaternion.Euler(0,180,0);
            transform.position = spawnPoint.transform.position;
            transform.rotation = rotation;
            rigidbody.velocity = transform.forward * carVelocity;
            waitingForSpawn = false;
        }
    }

    private void FollowWaypoints()
    {
        float carVelocity = rigidbody.velocity.magnitude;
        TurnTowardsWaypoint();

        // Follow waypoints
        float distance = Vector3.Distance(Waypoints[currentWayPoint].transform.position, this.transform.position);
        if (distance > 8)
        {
            
            // Move towards the waypoint
            if (carVelocity < maxSpeed) { 
                ApplyTorque();
            }
            else
            {
                ReleaseTorque();
            }
        }
        else if (distance > 2)
        {
            // Slow down
            if (carVelocity > 3)
            {
                ApplyBrakes();
            }
                
            else if (carVelocity < 2)
            {
                ApplyTorque();
            }
            else
            {
                ReleaseTorque();
            }
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

    private void TurnTowardsWaypoint()
    {
        Vector3 relativePos = Waypoints[currentWayPoint].transform.position - transform.position;
        float angle = Vector3.SignedAngle(relativePos, transform.forward, transform.up);

        if (Mathf.Abs(-steering - angle) < 10)
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
    }

    private void Steer()
    {
        float carVelocity = rigidbody.velocity.magnitude;
        float turnMultiplier = 1 + carVelocity / 3;
        if (steering > 50 / turnMultiplier) steering = 50 / turnMultiplier;
        if (steering < -50 / turnMultiplier) steering = -50 / turnMultiplier;
        foreach (GameObject wheel in frontWheels)
        {
            WheelCollider wheelCollider = wheel.GetComponent<WheelCollider>();
            wheelCollider.steerAngle = steering;
        }
    }

    private void ApplyTorque(float torque = 1)
    {
        foreach (GameObject wheel in frontWheels)
        {
            WheelCollider wheelCollider = wheel.GetComponent<WheelCollider>();
            wheelCollider.motorTorque = 1000 * torque;
            wheelCollider.brakeTorque = 0;
        }
    }

    private void ReleaseTorque()
    {
        foreach (GameObject wheel in frontWheels)
        {
            WheelCollider wheelCollider = wheel.GetComponent<WheelCollider>();
            wheelCollider.motorTorque = 0;
            wheelCollider.brakeTorque = 0;
        }
    }

    private void ApplyBrakes(float torque = -0.5f )
    {
        foreach (GameObject wheel in frontWheels)
        {
            WheelCollider wheelCollider = wheel.GetComponent<WheelCollider>();
            wheelCollider.brakeTorque = -torque;
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
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Finish"))
        {
            waitingForSpawn = true;
        }
    }
}
