using UnityEngine;
using System.Collections;

public class TurretScript : MonoBehaviour {

    public GameObject m_deathFC;

	void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            m_deathFC.SetActive(true);
        }
    }
}
