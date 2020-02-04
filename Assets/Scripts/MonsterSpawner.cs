using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour {

    //생성할 몬스터 소스.
    public GameObject SpawnMonster = null;

    public List<GameObject> MonsterList = new List<GameObject>();

    public int SpawnMaxCount = 50;

	// Use this for initialization
	void Start () {
        InvokeRepeating("Spawn", 3.5f, 4f);
	}

    void Spawn()
    {
        if(MonsterList.Count > SpawnMaxCount)
        {
            return;
        }

        Vector3 spawnPos = new Vector3(Random.Range(-100.0f, 100.0f), 1000.0f, Random.Range(-100.0f, 100.0f));

        Ray ray = new Ray(spawnPos, Vector3.down);
        RaycastHit raycastHit = new RaycastHit();
        if(Physics.Raycast(ray, out raycastHit, Mathf.Infinity) == true)
        {
            spawnPos.y = raycastHit.point.y;
        }
        GameObject newMonster = Instantiate(SpawnMonster, spawnPos, Quaternion.identity);
        MonsterList.Add(newMonster);

    }
	


}
