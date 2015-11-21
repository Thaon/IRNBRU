using UnityEngine;
using System.Collections;

public class Crouch : MonoBehaviour {

    float m_standingHeight=2.0f;
    float m_crouchedHeight = 1.0f;
    float m_height;
    float m_speed = 3.0f;

    bool m_isCrouching = false;

	// Use this for initialization
	void Start () {
        m_height = m_standingHeight;
	}
	
	// Update is called once per frame
	void Update () {

        GetComponent<CharacterController>().height = m_height;
        CrouchMovement();

	}

    void CrouchMovement()
    {

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            m_isCrouching = true;
            iTween.ValueTo(gameObject, iTween.Hash("to", m_crouchedHeight, "from", m_standingHeight, "speed", m_speed, "easetype", iTween.EaseType.linear, "onupdate", "UpdateHeight"));
           
        }
        
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            m_isCrouching = false;
            iTween.ValueTo(gameObject, iTween.Hash("to", m_standingHeight, "from", m_crouchedHeight, "speed", m_speed, "easetype", iTween.EaseType.linear, "onupdate", "UpdateHeight"));
           
        }
          

    }

    public bool IsCrouched()
    {
        return m_isCrouching;
    }

    void UpdateHeight(float value)
    {
        m_height = value;
    }

    
}
