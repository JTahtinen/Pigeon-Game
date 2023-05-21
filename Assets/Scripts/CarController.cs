using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float speed = 0;
    public float maxSpeed = 3;
    public float acceleration = 1;
    private bool moving = false;

    private List<GameObject> wheels;

    // Start is called before the first frame update
    void Start()
    {
        wheels = new List<GameObject>();
        foreach (Transform child in transform)
        {
            WheelCollider wheelCollider = child.GetComponent<WheelCollider>();
            if (wheelCollider != null)
            {
                wheels.Add(child.gameObject);
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
        if (moving)
        {
            speed += acceleration * Time.deltaTime;
            if (speed > maxSpeed)
            {
                speed = maxSpeed;
            }
        }

        ApplyTorque();

        TurnWheels();
    }

    private void ApplyTorque()
    {
        foreach (GameObject wheel in wheels)
        {
            WheelCollider wheelCollider = wheel.GetComponent<WheelCollider>();
            wheelCollider.motorTorque = speed * 100;
        }
    }

    private void TurnWheels()
    {
        foreach (GameObject wheel in wheels)
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
}
