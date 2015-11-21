using UnityEngine;
using System.Collections;

public class MGEnemy : MonoBehaviour {

    public GameObject m_spawner;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        //if (transform.position.y > -15f)
            transform.position -= new Vector3(0, .15f, 0);
	}

    public void Reset()
    {
        transform.position = m_spawner.transform.position;
    }
}
