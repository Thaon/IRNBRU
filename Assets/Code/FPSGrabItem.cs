using UnityEngine;
using System.Collections;

public class FPSGrabItem : MonoBehaviour {

    bool m_grabItem;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
        m_grabItem = Input.GetButton("Interact");
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 2.0f))
        {

        }
	}
}
