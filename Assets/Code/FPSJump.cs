using UnityEngine;
using System.Collections;

public class FPSJump : MonoBehaviour {

    Crouch m_crouchScript;
    CharacterController m_CC;
    public float m_jumpSpeed = 1f;
    public float m_gravity = 5f;
    Vector3 pos;

	// Use this for initialization
	void Start ()
    {
        m_crouchScript = GetComponent<Crouch>();
        m_CC = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (Input.GetButton("Jump") && m_CC.isGrounded && !m_crouchScript.IsCrouched())
        {
            pos.y = m_jumpSpeed;
        }

        if (!m_CC.isGrounded)
        {
            pos.y -= m_gravity * Time.deltaTime;
        }
        m_CC.Move(pos);
    }
}
