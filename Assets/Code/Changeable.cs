using UnityEngine;
using System.Collections;

public class Changeable : MonoBehaviour {

    public bool m_isVisible = true;
    public GameObject m_alternateObject;
    float m_burnAmount = -0.5f;

    void Start()
    {
        if (!m_isVisible)
        {
            m_burnAmount = 1.2f;
        }
    }

    public void Activate()
    {
        m_isVisible = !m_isVisible;
    }

    void Update()
    {
        if (!m_isVisible)
        {
            if (m_burnAmount < 1.2)
            {
                m_burnAmount += .01f;
            }
        }
        else
        {
            if (m_burnAmount > - 0.5f)
            {
                m_burnAmount -= .01f;
            }
        }
        GetComponent<MeshRenderer>().material.SetFloat("_Cutoff", m_burnAmount);
    }

    public void Dismantle()
    {
        if (m_alternateObject != null)
        {
            m_alternateObject.GetComponent<Changeable>().Activate();
        }
        else
        {
            Activate();
        }
    }
}
