using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHit : MonoBehaviour
{
    public int normalAttackDamage; //��ͨ�����˺�
    public int specialAttackDamage; //���⹥���˺�
    public int normalAttackEnergyCost; // ��ͨ������������

    private ParticleSystem specialParticle;

    private void OnTriggerEnter(Collider other)
    {
        // �����ж�ֻ�Ե�����Ч
        if (other.gameObject.layer.Equals(7))
        {
            other.GetComponent<EnemyCharacter>().GetHit(normalAttackDamage);
            Debug.Log("NormalAttack hit: " + other.gameObject.name);
        }
    }
}