using UnityEngine;
using System.Collections;

public class MGSpawner : MonoBehaviour {

    public GameObject[] m_enemies;
    public float m_timeBetweenSpawns = 2;

    // Use this for initialization
    void Start ()
    {
        StartCoroutine(SpawnEnemies(m_timeBetweenSpawns));
    }
	
    void Spawn(int lane)
    {
        m_enemies[lane].GetComponent<MGEnemy>().Reset();
    }

	IEnumerator SpawnEnemies(float timeBetweenSpawns)
    {
        yield return new WaitForSeconds(timeBetweenSpawns);
        int ran = Random.Range(0, 3);
        Spawn(ran);
        ran++;
        if (ran > 2)
        {
            ran = 0;
        }
        Spawn(ran);
        StartCoroutine(SpawnEnemies(m_timeBetweenSpawns));
    }

    public void Reset()
    {
        StopAllCoroutines();
        StartCoroutine(SpawnEnemies(m_timeBetweenSpawns));
        m_enemies[0].GetComponent<MGEnemy>().Restart();
        m_enemies[1].GetComponent<MGEnemy>().Restart();
        m_enemies[2].GetComponent<MGEnemy>().Restart();
    }
}
