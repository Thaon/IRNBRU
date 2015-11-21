using UnityEngine;
using System.Collections;

public class FPSGrabItem : MonoBehaviour {

    bool m_grabItemButton;
    bool m_isCarryingItem = false;
    GameObject m_carriedItem;

    public GameObject m_grabTarget;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
        m_grabItemButton = Input.GetButtonDown("Interact");
        RaycastHit hit;
        if (m_grabItemButton)
        {
            if (!m_isCarryingItem)
            {
                //get the fucker
                if (Physics.Raycast(transform.position, transform.forward, out hit, 5.0f))
                {
                    if (hit.collider.tag == "Grabable")
                    {
                        //Debug.Log("grabable got");
                        m_carriedItem = hit.collider.gameObject;
                        m_carriedItem.GetComponent<Rigidbody>().isKinematic = true;
                        iTween.MoveTo(hit.collider.gameObject, m_grabTarget.transform.position, 1.0f);
                        m_isCarryingItem = true;
                    }
                }
            }
            else
            {
                //throw the fucker
                m_carriedItem.GetComponent<Rigidbody>().isKinematic = false;
                m_carriedItem.GetComponent<Rigidbody>().AddForce(transform.forward * 5, ForceMode.Impulse);
                m_isCarryingItem = false;
                m_carriedItem = null;
            }
        }
        //carry the fucker
        if (m_carriedItem != null)
        {
            m_carriedItem.transform.position = m_grabTarget.transform.position;
            m_carriedItem.transform.rotation = m_grabTarget.transform.rotation;
        }
	}
}
