using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script controls the camera to the gameobject (which is the player)

public class CameraController : MonoBehaviour
{
	public GameObject MarleytheCat;
	private Vector3 offset;



    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - MarleytheCat.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = MarleytheCat.transform.position + offset;
    }
}
