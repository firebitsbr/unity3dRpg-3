using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockMove : MonoBehaviour
{ 
    Rigidbody rb;
    public float x = 1f;
    public float z = 0f;

    bool isFighter = false;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();  
    }

    // Update is called once per frame
    void Update()
    {
        if(isFighter) rb.AddForce(x, 0f, z);  
    }

    private void OnTriggerEnter(Collider other)
    {
        //플레이어의 공격에 맞았다면.
        if (other.gameObject.CompareTag("Player") == true)
        {
            Debug.Log(other.gameObject.name);
            isFighter = true;

        }
    }


}
