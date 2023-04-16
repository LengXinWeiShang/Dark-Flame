using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackHit : MonoBehaviour
{
    public int attackDamage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ���﹥��ֻ�������Ч
            other.GetComponent<PlayerCharacter>().GetHit(attackDamage);
            Debug.Log("Attack hit: " + other.gameObject.name);
        }
    }
}