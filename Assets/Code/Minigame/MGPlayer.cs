using UnityEngine;
using System.Collections;

public class MGPlayer : MonoBehaviour {

    public GameObject m_leftLane;
    public GameObject m_centerLane;
    public GameObject m_rightLane;

    int m_lane = 1; //0 for left, 2 for right

    bool m_canSwapLane = true;
	
	// Update is called once per frame
	void Update ()
    {
	    if (Input.GetAxis("Horizontal") > 0 && m_lane < 2 && m_canSwapLane)
        {
            StartCoroutine(GoRight());
        }

        if (Input.GetAxis("Horizontal") < 0 && m_lane > 0 && m_canSwapLane)
        {
            StartCoroutine(GoLeft());
        }
	}

    IEnumerator GoLeft()
    {
        m_canSwapLane = false;

        if (m_lane == 1)
        {
            transform.position = m_leftLane.transform.position;
            m_lane = 0;
        }

        if (m_lane == 2)
        {
            transform.position = m_centerLane.transform.position;
            m_lane = 1;
        }

        yield return new WaitForSeconds(0.5f);
        m_canSwapLane = true;
    }

    IEnumerator GoRight()
    {
        m_canSwapLane = false;

        if (m_lane == 1)
        {
            transform.position = m_rightLane.transform.position;
            m_lane = 2;
        }
        if (m_lane == 0)
        {
            transform.position = m_centerLane.transform.position;
            m_lane = 1;
        }

        yield return new WaitForSeconds(0.5f);
        m_canSwapLane = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "MGEnemy")
            Debug.Log("you lost!");
    }
}
