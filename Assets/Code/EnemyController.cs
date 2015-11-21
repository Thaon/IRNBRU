using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour {

    CharacterController m_CC;
    GameObject m_player;
    public float m_followDistance;
    public float m_minSpeed = 50f;
    public float m_maxSpeed = 80f;
    public List<Vector3> m_points;

    public bool m_IsFollowing = false;
    private int m_current = 0;

    public enum state {wayPointing, followingPlayer}

    state enemyState = state.wayPointing;

	// Use this for initialization
	void Start ()
    {
        m_CC = GetComponent<CharacterController>();
        m_player = null;
        foreach(Transform waypoint in GetComponentsInChildren<Transform>())
        {
            if (waypoint.gameObject != this.gameObject)
            {
                m_points.Add(waypoint.transform.position);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {

        switch (enemyState)
        {
            case state.wayPointing:
                //look for player and follow waypoints
                FollowWayPoints();
                break;

            case state.followingPlayer:
                //follow the player until is not in vision anymore, then return to the last patrolling position
                LookForPlayer();
                break;
        }

	}

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Collider>().tag == "Player")
        {
            m_player = other.GetComponent<Collider>().gameObject;
            enemyState = state.followingPlayer;
            //Debug.Log("Following!");
        }
    }

    void FollowWayPoints()
    {
        if (Vector3.Distance(m_points[m_current], transform.position) < 1)
        {
            m_current++;
            //Debug.Log(m_current);
            if (m_current == m_points.Count)
                m_current = 0;
        }

        Vector3 direction = m_points[m_current] - transform.position;
        direction = direction.normalized * m_minSpeed * Time.deltaTime;
        m_points[m_current] = new Vector3(m_points[m_current].x, transform.position.y, m_points[m_current].z);
        transform.LookAt(m_points[m_current], Vector3.up);
        m_CC.Move(direction);
    }

    void LookForPlayer()
    {
        if (m_player != null)
        {
            RaycastHit hit;
            Vector3 rayDirection = (m_player.transform.position - new Vector3(0, .5f, 0)) - transform.position;
            if (Physics.Raycast(transform.position, rayDirection, out hit))
            {
                if (hit.collider.tag == "Player" && hit.distance < m_followDistance)//check if player can be seen and is within distance
                {
                    FollowPlayer();
                }

                else
                {
                    enemyState = state.wayPointing;
                    m_player = null;
                }
            }
        }
    }

    void FollowPlayer()
    {
        Debug.Log("following");
        Vector3 direction = m_player.transform.position - transform.position;
        transform.LookAt(m_player.transform.position);
        m_CC.SimpleMove(direction.normalized * m_minSpeed);
    }
}
