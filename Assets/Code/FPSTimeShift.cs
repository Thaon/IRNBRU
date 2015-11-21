using UnityEngine;
using System.Collections;
using Colorful;

public class FPSTimeShift : MonoBehaviour {

    public GameObject m_rootNode;
    public bool m_glitching;
    float m_tearingAmount = 0;
    bool m_canTimeShift = true;
	
	// Update is called once per frame
	void Update ()
    {
	    if (Input.GetButton("TimeShift") && m_canTimeShift)
        {
            m_canTimeShift = false;
            StartCoroutine(GlitchIn());
            foreach (Changeable changeable in m_rootNode.GetComponentsInChildren<Changeable>())
            {
                changeable.Dismantle();
            }
        }

        if (m_glitching)
        {
            if (m_tearingAmount < 1)
            {
                m_tearingAmount += .01f;
            }
        }
        else
        {
            if (m_tearingAmount > 0)
            {
                m_tearingAmount -= .01f;
            }
            else
            {
                m_tearingAmount = 0;
            }
        }

        GetComponent<Glitch>().SettingsTearing.Intensity = m_tearingAmount;
	}

    IEnumerator GlitchIn()
    {
        m_glitching = true;
        yield return new WaitForSeconds(1.8f);
        StartCoroutine(GlitchOut());
    }

    IEnumerator GlitchOut()
    {
        m_glitching = false;
        yield return new WaitForSeconds(1f);
        m_canTimeShift = true;
    }
}
