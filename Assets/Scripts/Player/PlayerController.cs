using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    PlayerCharacter player;

    [HideInInspector] public float h;
    [HideInInspector] public float v;

    [HideInInspector] public bool roll;
    [HideInInspector] public bool attack;
    [HideInInspector] public bool specialAttack;
    [HideInInspector] public bool switchWeapon;
    [HideInInspector] public bool use;
    [HideInInspector] public bool react;

    void Start()
    {
        player = GetComponent<PlayerCharacter>();
    }

    void Update()
    {
        roll = Input.GetButtonDown("Roll");
        attack = Input.GetButtonDown("Attack");
        specialAttack = Input.GetButtonDown("SpecialAttack");
        switchWeapon = Input.GetButtonDown("SwitchWeapon");
        use = Input.GetButtonDown("Use");
        react = Input.GetButtonDown("React");

        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
    }
}
