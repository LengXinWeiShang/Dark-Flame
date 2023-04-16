using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Move,           // ��ֹ���ƶ�״̬
    Roll,           // ����״̬
    Attack,         // ����״̬
    SwitchWeapon,   // �л�����
    GetHit,         // �ܻ�
    Die,            // ����
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

    // �����ָ��ٶ�
    public float recoverSpeed = 30f;

    // �����պ󾭹���òſ��Կ�ʼ�ָ�����
    public float recoverCD = 1f;

    private Vector3 move;
    public int hp;
    public float energy;

    // �����ۿպ����Move״̬��ʼ��ʱ
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
            // �ƶ��ķ���
            Vector3 dir = move.normalized;
            // �泯Ŀ�귽�����Ԫ��
            Quaternion faceToQuat = Quaternion.LookRotation(dir);
            // ��Ŀ�귽��͵�ǰ����֮����һ����ֵ������Ŀ�곯��͵�ǰ����֮�䣩
            Quaternion q = Quaternion.Slerp(transform.rotation, faceToQuat, 0.5f);
            // ת���ֵ����
            rigid.MoveRotation(q);
            // ������ǰ���ƶ�
            // rigid.MovePosition(transform.position + transform.forward * runSpeed * Time.fixedDeltaTime); // ����ǿ��λ�Ƶķ�ʽ�ڽ�ɫ�ٶȽϿ�ʱ���ܴ����ϱ�ǽ��Ľӷ�
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
        // �����뷽�򷭹�����û�����뷽�򣬳������泯���򷭹�
        Vector3 dir = move.normalized;
        if (move == Vector3.zero)
        {
            dir = transform.forward;
        }
        rigid.velocity = transform.forward * runSpeed * 2;
    }

    private void Attack()
    {
        // ����ʱ�޷��ƶ�
        rigid.velocity = Vector3.zero;
    }

    // �л�����ʱ�Ķ���֡�¼�
    public void SwitchWeaponEvent()
    {
        WeaponManager.Instance.CurrentWeapon.SetActive(false);
        WeaponManager.Instance.ChangeNext().SetActive(true);
        curWeapon = WeaponManager.Instance.CurrentWeapon.GetComponent<WeaponHit>();
    }

    // �ܻ�
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

    // �����߼�
    private void Die()
    {
        anim.SetTrigger("Die");
        GameMode.Instance.PlayerDie();
    }

    // ��������
    public void CostEnergy(int cost)
    {
        energy -= cost;
        if (energy <= 0)
        {
            // ��������Ĺ⣬��Ҫ�ȴ�ʱ���ٻָ�
            energy = 0;
            recoverTime = Time.time + recoverCD;
        }
    }

    // Move״̬�ָ�����
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

    // Move״̬����ظ���
    private void StateMoveUpdate()
    {
        move = new Vector3(controller.h, 0, controller.v);
        if (energy > 0)
        {
            // ������ʱ���ܷ����͹���
            if (controller.roll)
            {
                // ��������
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

    // ����״̬�������ű�״̬��

    #region ״̬��

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
            //��һ��ʹ��ʱ��ʼ���ֵ�
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
            // ��һ��ʹ��ʱ��ʼ���ֵ�
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
            // ��һ��ʹ��ʱ��ʼ���ֵ�
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
            // �����ι�������ʱȡ������Ԥ����
            anim.ResetTrigger("Attack");
        }
    }

    public void GetHitExit(int n)
    {
        // ��ֹһֱӲֱ
        anim.ResetTrigger("GetHit");
    }

    #endregion ״̬��
}