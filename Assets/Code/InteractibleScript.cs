using UnityEngine;
using System.Collections;

public class InteractibleScript : MonoBehaviour {

    public GameObject m_toRemove;
    public GameObject m_FC;

	public void Activate()
    {
        if (m_toRemove != null)
        {
            m_toRemove.SetActive(false);
            m_toRemove = null;
        }
        if (m_FC != null)
        {
            m_FC.SetActive(true);
        }
    }
}
