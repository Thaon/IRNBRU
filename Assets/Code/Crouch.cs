using UnityEngine;
using System.Collections;

public class Crouch : MonoBehaviour {

    float m_standingHeight=2.0f;
    float m_crouchedHeight = 1.0f;
    float m_height;
    float m_speed = 3.0f;

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

        Vector3 pos = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            iTween.ValueTo(gameObject, iTween.Hash("to", m_crouchedHeight, "from", m_standingHeight, "speed", m_speed, "easetype", iTween.EaseType.linear, "onupdate", "UpdateHeight"));
           
        }
        
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            iTween.ValueTo(gameObject, iTween.Hash("to", m_standingHeight, "from", m_crouchedHeight, "speed", m_speed, "easetype", iTween.EaseType.linear, "onupdate", "UpdateHeight"));
           
        }
          

    }

    void UpdateHeight(float value)
    {
        m_height = value;
    }

    
}
