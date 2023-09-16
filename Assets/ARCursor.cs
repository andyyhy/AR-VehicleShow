using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.ARFoundation;

public class ARCursor : MonoBehaviour
{
    ARRaycastManager ar_raycast_manager;

    public Transform cursor;

    // Start is called before the first frame update
    void Start()
    {
        ar_raycast_manager = GetComponent<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 screen_center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        List<ARRaycastHit> result_hits = new List<ARRaycastHit>();


        ar_raycast_manager.Raycast(screen_center, result_hits);

        if (result_hits.Count > 0)
        {
            Vector3 new_position_for_cursor = result_hits[0].pose.position;

            Vector3 new_direction_for_cursor = new Vector3(Camera.main.transform.forward.x, 0.0f, Camera.main.transform.forward.z);


            cursor.transform.position = new_position_for_cursor;
            cursor.transform.forward = new_direction_for_cursor;
        }
    }
}
