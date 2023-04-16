using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // 单例
    public static WeaponManager Instance { get; private set; }
    // 可用的武器列表
    public List<GameObject> weaponList;
    // 当前使用武器的下标
    int current = 0;

    void Awake()
    {
        Instance = this;
    }
    
    // 获取当前武器
    public GameObject CurrentWeapon { get { return weaponList[current]; } }

    // 更换武器
    public GameObject ChangeNext()
    {
        current = (current + 1) % weaponList.Count;
        return weaponList[current];
    }
}
