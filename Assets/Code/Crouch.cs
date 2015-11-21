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

        GetComponent<CharacterController>().height = m_newHeight;

	}

    void CrouchMovement()
    {
        bool crouchButton = Input.GetKey(KeyCode.LeftControl);

        Vector3 pos = Vector3.zero;

        if (crouchButton)
        {
            iTween.MoveBy(gameObject, iTween.Hash("from",m_height, "to", m_newHeight,"speed",m_speed));
        }
        else
        {
            iTween.MoveBy(gameObject, iTween.Hash("to", m_newHeight, "from", m_height, "speed", m_speed));
        }
          

    }

    
}
