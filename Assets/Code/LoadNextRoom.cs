using UnityEngine;
using System.Collections;

public class LoadNextRoom : MonoBehaviour {

    public string name;

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
            Application.LoadLevel(name);
        }
           
    }
}
