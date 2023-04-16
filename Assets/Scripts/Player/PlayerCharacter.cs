using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Move,           // 静止和移动状态
    Roll,           // 翻滚状态
    Attack,         // 攻击状态
    SwitchWeapon,   // 切换武器
    GetHit,         // 受击
    Die,            // 死亡
}

public class PlayerCharacter : MonoBehaviour
{
    private PlayerController controller;
    private Animator anim;
    private Rigidbody rigid;

    public PlayerState state;
    public float runSpeed = 4;
    public int maxHp = 100;
    public int maxEnergy = 100;
    public int rollEnergyCost = 20;

    // 体力恢复速度
    public float recoverSpeed = 30f;

    // 体力空后经过多久才可以开始恢复体力
    public float recoverCD = 1f;

    private Vector3 move;
    public int hp;
    public float energy;

    // 体力槽空后进入Move状态后开始计时
    private float recoverTime;

    private WeaponHit curWeapon;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        state = PlayerState.Move;
        hp = maxHp;
        energy = maxEnergy;
        curWeapon = WeaponManager.Instance.CurrentWeapon.GetComponent<WeaponHit>();
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case PlayerState.Move:
                Move();
                break;

            case PlayerState.Roll:
                Roll();
                break;

            case PlayerState.Attack:
                Attack();
                break;

            case PlayerState.SwitchWeapon:
                break;
        }
    }

    private void Move()
    {
        if (move.magnitude > 0.1f)
        {
            // 移动的方向
            Vector3 dir = move.normalized;
            // 面朝目标方向的四元数
            Quaternion faceToQuat = Quaternion.LookRotation(dir);
            // 在目标方向和当前朝向之间做一个插值（介于目标朝向和当前朝向之间）
            Quaternion q = Quaternion.Slerp(transform.rotation, faceToQuat, 0.5f);
            // 转向插值方向
            rigid.MoveRotation(q);
            // 朝自身前方移动
            // rigid.MovePosition(transform.position + transform.forward * runSpeed * Time.fixedDeltaTime); // 这种强制位移的方式在角色速度较快时可能穿过较薄墙体的接缝
            rigid.velocity = transform.forward * runSpeed;
            anim.SetBool("Run", true);
        }
        else
        {
            rigid.velocity = Vector3.zero;
            anim.SetBool("Run", false);
        }
    }

    private void Roll()
    {
        // 朝输入方向翻滚，若没有输入方向，朝人物面朝方向翻滚
        Vector3 dir = move.normalized;
        if (move == Vector3.zero)
        {
            dir = transform.forward;
        }
        rigid.velocity = transform.forward * runSpeed * 2;
    }

    private void Attack()
    {
        // 攻击时无法移动
        rigid.velocity = Vector3.zero;
    }

    // 切换武器时的动画帧事件
    public void SwitchWeaponEvent()
    {
        WeaponManager.Instance.CurrentWeapon.SetActive(false);
        WeaponManager.Instance.ChangeNext().SetActive(true);
        curWeapon = WeaponManager.Instance.CurrentWeapon.GetComponent<WeaponHit>();
    }

    // 受击
    public void GetHit(int damage)
    {
        if (state == PlayerState.Die)
        {
            return;
        }
        hp -= damage;

        anim.SetTrigger("GetHit");

        if (hp <= 0)
        {
            hp = 0;
            Die();
        }
    }

    // 死亡逻辑
    private void Die()
    {
        anim.SetTrigger("Die");
        GameMode.Instance.PlayerDie();
    }

    // 消耗体力
    public void CostEnergy(int cost)
    {
        energy -= cost;
        if (energy <= 0)
        {
            // 如果体力耗光，需要等待时间再恢复
            energy = 0;
            recoverTime = Time.time + recoverCD;
        }
    }

    // Move状态恢复体力
    private void RecoverEnergy()
    {
        if (recoverTime <= Time.time)
        {
            energy += recoverSpeed * Time.deltaTime;
        }
        if (energy > maxEnergy)
        {
            energy = maxEnergy;
        }
    }

    // Move状态的相关更新
    private void StateMoveUpdate()
    {
        move = new Vector3(controller.h, 0, controller.v);
        if (energy > 0)
        {
            // 有体力时才能翻滚和攻击
            if (controller.roll)
            {
                // 消耗体力
                CostEnergy(rollEnergyCost);
                anim.SetTrigger("Roll");
            }
            else if (controller.attack)
            {
                anim.SetTrigger("Attack");
            }
            else if (controller.switchWeapon)
            {
                anim.SetTrigger("SwitchWeapon");
            }
        }
    }

    // 动画状态机驱动脚本状态机

    #region 状态机

    private delegate void FuncStateEnter();

    private delegate void FuncStateUpdate(int n);

    private delegate void FuncStateExit(int n);

    private Dictionary<PlayerState, FuncStateEnter> dictStateEnter;
    private Dictionary<PlayerState, FuncStateUpdate> dictStateUpdate;
    private Dictionary<PlayerState, FuncStateExit> dictStateExit;

    public void OnAnimStateEnter(PlayerState s)
    {
        if (dictStateEnter == null)
        {
            //第一次使用时初始化字典
            dictStateEnter = new Dictionary<PlayerState, FuncStateEnter>
            {
                {PlayerState.Move, MoveEnter},
                {PlayerState.Roll, RollEnter},
                {PlayerState.Attack, AttackEnter},
                {PlayerState.SwitchWeapon, SwitchWeaponEnter},
                {PlayerState.GetHit, GetHitEnter},
                {PlayerState.Die, DieEnter},
            };
        }

        if (dictStateEnter.ContainsKey(s) && dictStateEnter[s] != null)
        {
            dictStateEnter[s]();
        }
    }

    public void OnAnimStateUpdate(PlayerState s, int n = 0)
    {
        if (dictStateUpdate == null)
        {
            // 第一次使用时初始化字典
            dictStateUpdate = new Dictionary<PlayerState, FuncStateUpdate>
            {
                {PlayerState.Move, MoveUpdate},
                {PlayerState.Roll, RollUpdate},
                {PlayerState.Attack, AttackUpdate},
            };
        }

        if (dictStateUpdate.ContainsKey(s) && dictStateUpdate[s] != null)
        {
            dictStateUpdate[s](n);
        }
    }

    public void OnAnimStateExit(PlayerState s, int n)
    {
        if (dictStateExit == null)
        {
            // 第一次使用时初始化字典
            dictStateExit = new Dictionary<PlayerState, FuncStateExit>
            {
                {PlayerState.Move, MoveExit},
                {PlayerState.Roll, RollExit},
                {PlayerState.Attack, AttackExit},
                {PlayerState.GetHit, GetHitExit},
            };
        }

        if (dictStateExit.ContainsKey(s) && dictStateExit[s] != null)
        {
            dictStateExit[s](n);
        }
    }

    // ---------- Enter ----------

    public void MoveEnter()
    {
        state = PlayerState.Move;
    }

    public void RollEnter()
    {
        state = PlayerState.Roll;
    }

    public void AttackEnter()
    {
        state = PlayerState.Attack;
        CostEnergy(curWeapon.normalAttackEnergyCost);
    }

    public void SwitchWeaponEnter()
    {
        state = PlayerState.SwitchWeapon;
    }

    public void GetHitEnter()
    {
        state = PlayerState.GetHit;
    }

    public void DieEnter()
    {
        state = PlayerState.Die;
    }

    // ---------- Update ----------

    public void MoveUpdate(int n)
    {
        RecoverEnergy();
        StateMoveUpdate();
    }

    public void RollUpdate(int n)
    {
    }

    public void AttackUpdate(int n)
    {
        if (controller.attack && energy > 0)
        {
            anim.SetTrigger("Attack");
        }
    }

    // ---------- Exit ----------

    public void MoveExit(int n)
    {
    }

    public void RollExit(int n)
    {
    }

    public void AttackExit(int n)
    {
        if (n == 3)
        {
            // 第三段攻击结束时取消攻击预输入
            anim.ResetTrigger("Attack");
        }
    }

    public void GetHitExit(int n)
    {
        // 防止一直硬直
        anim.ResetTrigger("GetHit");
    }

    #endregion 状态机
}