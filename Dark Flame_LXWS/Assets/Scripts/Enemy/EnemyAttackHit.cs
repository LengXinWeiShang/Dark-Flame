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
            // 怪物攻击只对玩家生效
            other.GetComponent<PlayerCharacter>().GetHit(attackDamage);
            Debug.Log("Attack hit: " + other.gameObject.name);
        }
    }
}