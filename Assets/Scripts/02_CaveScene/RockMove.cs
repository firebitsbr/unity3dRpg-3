using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockMove : MonoBehaviour
{ 

    // addforce
    Rigidbody rb;
    public float x = 1f;
    public float z = 0f;
    bool isFighter = false;

    //공격 사정거리
    private Transform playerTransform;

    public float traceDist = 1.0f;
    public float dist = 30f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        playerTransform = GameObject.FindWithTag("Player").GetComponent<Transform>();

        StartCoroutine(this.CheckState());
    }

    IEnumerator CheckState() {

        while (!isFighter)
        {
            yield return new WaitForSeconds(0.2f);

            dist = Vector3.Distance(playerTransform.position, transform.position);

            Debug.Log(dist);

            if (dist <= traceDist)
            {
                isFighter = true;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(isFighter) rb.AddForce(x, 0f, z);  
    }

/*
    private void OnTriggerEnter(Collider other)
    {
        //플레이어의 공격에 맞았다면.
        if (other.gameObject.CompareTag("Player") == true)
        {
            Debug.Log(other.gameObject.name);
            isFighter = true;

        }
    }
*/

}
