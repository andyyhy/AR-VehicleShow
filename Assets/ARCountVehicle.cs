using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ARCountVehicle : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        GameObject[] vehicles;
        vehicles = GameObject.FindGameObjectsWithTag("Vehicle");
        int num_vehicles;
        num_vehicles = vehicles.Length;
        GetComponent<TextMeshProUGUI>().text = "Vehicle Count: " + num_vehicles.ToString();
    }
}
