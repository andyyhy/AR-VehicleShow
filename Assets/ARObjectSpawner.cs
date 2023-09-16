using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARObjectSpawner : MonoBehaviour
{

    public Transform cursor;
    public GameObject purple_car;
    public GameObject blue_car;
    public GameObject plane;
    public AudioClip spawn_sound_effect;



    public void SpawnPurpleCarAtCursor()
    {
        GameObject new_object = Instantiate(purple_car);
        new_object.transform.position = cursor.position;
        new_object.transform.forward = cursor.forward;

        AudioSource.PlayClipAtPoint(spawn_sound_effect, Camera.main.transform.position);
    }
    public void SpawnBlueCarAtCursor()
    {
        GameObject new_object = Instantiate(blue_car);
        new_object.transform.SetPositionAndRotation(cursor.position, cursor.rotation);

        AudioSource.PlayClipAtPoint(spawn_sound_effect, Camera.main.transform.position);
    }
    public void SpawnPlaneAtCursor()
    {
        GameObject new_object = Instantiate(plane);
        new_object.transform.SetPositionAndRotation(cursor.position, cursor.rotation);

        AudioSource.PlayClipAtPoint(spawn_sound_effect, Camera.main.transform.position);
    }
}
