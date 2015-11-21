using UnityEngine;
using System.Collections;

public class Crouch : MonoBehaviour {

    float m_height=2.0f;
    float m_newHeight = 1.0f;
    float m_speed = 3.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        transform.position += CrouchMovement();
        Mathf.Lerp(m_height, m_newHeight, m_speed*Time.deltaTime);
        GetComponent<CharacterController>().height = m_newHeight;

	}

    Vector3 CrouchMovement()
    {
        bool crouchButton = Input.GetKey(KeyCode.LeftControl);

        Vector3 pos = Vector3.zero;

        if (crouchButton)
        {
            Debug.Log("£crouched");
            m_height = 2.0f;
            m_newHeight = 1.0f;
        }
        else
        {
            m_height = 1.0f;
            m_newHeight = 2.0f;
        }
          

        return pos;
    }

    
}
