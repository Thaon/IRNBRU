using UnityEngine;
using System.Collections;

public class LoadNextRoom : MonoBehaviour {

    public string name;
      public GameObject m_flowChart;
     

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnTriggerEnter(Collider target)
    {
        if (target.gameObject.tag == "Player")
        {
            m_flowChart.SetActive(true);
            //Application.LoadLevel(name);
        }
           
    }
}
