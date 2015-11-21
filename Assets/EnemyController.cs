using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

    public GameObject player;
    public float followDistance;
    public float minSpeed = 50f;
    public float maxSpeed = 80f;
    public Vector3[] points;
    public LayerMask layer;

    private bool IsFollowing = false;
    private int current = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        RayCasting();
	}

    void RayCasting()
    {
        Vector3 pos = Vector3.zero;

        if (Vector3.Distance(points[current], transform.position) < 1)
        {
            current++;
            if (current >= points.Length)
                current = 0;
        }

        if (player != null)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, (player.transform.position - transform.position).normalized);
            Debug.DrawRay(transform.position, (player.transform.position - transform.position).normalized, Color.green);
            Physics.Raycast(ray, out hit, followDistance, layer);

            if (hit.collider != null)
            {
                if (hit.collider.tag == "Player")
                {
                    IsFollowing = true;
                }

                else
                {
                    IsFollowing = false;
                }
            }
            else
                IsFollowing = false;


        }
    }

    private void FixedUpdate()
    {
        if (!IsFollowing)
        {
            Vector3 direction = points[current] - transform.position;
            direction = direction.normalized;
            GetComponent<Rigidbody>().velocity = direction * minSpeed * Time.deltaTime;
        }
        else if (IsFollowing)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            GetComponent<Rigidbody>().velocity = direction * Random.Range(minSpeed, maxSpeed) * Time.deltaTime;
        }
    }
}
