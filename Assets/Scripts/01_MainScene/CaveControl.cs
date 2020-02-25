using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CaveControl : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //플레이어의 공격에 맞았다면.
        if (other.gameObject.CompareTag("PlayerAttack") == true)
        {
            SceneManager.LoadScene("Cave");
        }
    }
}
