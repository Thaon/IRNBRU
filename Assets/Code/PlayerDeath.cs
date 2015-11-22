using UnityEngine;
using System.Collections;

public class PlayerDeath : MonoBehaviour {

    public GameObject m_deathFlowChart;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider target)
    {
        if (target.gameObject.tag == "Player")
        {
            m_deathFlowChart.SetActive(true);
        
        }
    }
}
