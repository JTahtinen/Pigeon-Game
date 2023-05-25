using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSensor : MonoBehaviour
{
    private int _overlaps;

    public bool isOccupied {
        get {
            return _overlaps > 0;
        }
    }

    // Count how many colliders are overlapping this trigger.
    // If desired, you can filter here by tag, attached components, etc.
    // so that only certain collisions count. Physics layers help too.
    void OnTriggerEnter(Collider other) {
        if (other.name.Contains("Car"))
            _overlaps++;
    }

    void OnTriggerExit(Collider other) {
        if (other.name.Contains("Car"))
            _overlaps--;
    }
}
