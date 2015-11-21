using UnityEngine;
using System.Collections;

public class WeightTrigger : MonoBehaviour {

    public GameObject start;
    public GameObject end;
    float speed = 3.0f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider target)
    {
        if (target.gameObject.tag == "Grabable")
            transform.position = Vector3.MoveTowards(transform.position, end.transform.position, speed*Time.deltaTime);

        Debug.Log("On Trap");
    }

    void OnTriggerExit(Collider target)
    {
        if (target.gameObject.tag == "Grabable")
            transform.position = Vector3.MoveTowards(transform.position, start.transform.position, speed * Time.deltaTime);

        Debug.Log("Off Trap");

    }
}
