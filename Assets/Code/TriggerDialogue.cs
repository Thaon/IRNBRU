using UnityEngine;
using System.Collections;

public class TriggerDialogue : MonoBehaviour {

    public GameObject m_dialogueFlowchart;
    public bool m_isDialogueRepeatable;
    bool m_hasBeenActivated = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (!m_hasBeenActivated)
            {
                m_dialogueFlowchart.SetActive(true);
                if (!m_isDialogueRepeatable)
                {
                    m_hasBeenActivated = true;
                }
            }
        }
    }
}
