using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // ����
    public static WeaponManager Instance { get; private set; }
    // ���õ������б�
    public List<GameObject> weaponList;
    // ��ǰʹ���������±�
    int current = 0;

    void Awake()
    {
        Instance = this;
    }
    
    // ��ȡ��ǰ����
    public GameObject CurrentWeapon { get { return weaponList[current]; } }

    // ��������
    public GameObject ChangeNext()
    {
        current = (current + 1) % weaponList.Count;
        return weaponList[current];
    }
}
