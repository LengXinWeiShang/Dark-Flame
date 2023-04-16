using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHit : MonoBehaviour
{
    public int normalAttackDamage; //普通攻击伤害
    public int specialAttackDamage; //特殊攻击伤害
    public int normalAttackEnergyCost; // 普通攻击体力消耗

    private ParticleSystem specialParticle;

    private void OnTriggerEnter(Collider other)
    {
        // 攻击判定只对敌人生效
        if (other.gameObject.layer.Equals(7))
        {
            other.GetComponent<EnemyCharacter>().GetHit(normalAttackDamage);
            Debug.Log("NormalAttack hit: " + other.gameObject.name);
        }
    }
}