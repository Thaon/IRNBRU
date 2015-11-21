using UnityEngine;
using System.Collections;

public class FPSMinigameTrigger : MonoBehaviour {

    public GameObject m_minigame;
    public GameObject m_player;
    

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.Q))
        {
            m_minigame.SetActive(true);
            m_minigame.GetComponentInChildren<MGSpawner>().Reset(); ;
            m_player.SetActive(false);
        }
	}
}
