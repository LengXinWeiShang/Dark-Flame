using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Sleeping,           // ������αװ״̬
    Alert,              // ����
    Move,               // �ƶ�
    Attack,             // ����
    GetHit,             // �ܻ�
    Die                 // ����
}

public class EnemyCharacter : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rigid;

    public EnemyState state = EnemyState.Sleeping;

    private PlayerCharacter player;

    public int hp = 100;                        // ����ֵ
    public int souls = 100;                     // �������
    public int moveSpeed = 2;                   // �ƶ��ٶ�
    public float alertDistance = 10f;           // ������ҵ�������
    public float trackingDistance = 20f;        // ������Һ�����׷�پ���
    public float stopDistance = 2f;             // �ƶ�������Ҹþ���ʱֹͣ�ƶ�

    private float disappearTime = 6f;           // ��ʬ����ʧ��ʱ��

    public ParticleSystem deathParticle;
    public ParticleSystem bloodParticle;
    private Transform soulsParticle;
    private Transform soulsClone;
    private Transform soulsParent;

    private Vector3 move;
    private float dieTime = -1;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacter>();
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (state == EnemyState.Move)
        {
            rigid.MovePosition(transform.position + move * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // ����ת��Ŀ��
    private void Rotate(Vector3 dir)
    {
        Quaternion faceToQuat = Quaternion.LookRotation(dir); //��ɫ�泯Ŀ�귽�����Ԫ��
        Quaternion slerp = Quaternion.Slerp(transform.rotation, faceToQuat, 0.2f);
        transform.rotation = slerp;
    }

    // ����
    private void Attack()
    {
        // ���ѡȡһ�ֹ���
        int ran = Random.Range(1, 4);
        anim.SetInteger("Attack", ran);
    }

    // �ܻ�
    public void GetHit(int damage)
    {
        if (state == EnemyState.Die)
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

    // ������ش���
    public void Die()
    {
        anim.SetTrigger("Die");
        GetComponent<Collider>().isTrigger = true;
        rigid.isKinematic = true;
    }

    // ����״̬�������ű�״̬��

    #region ״̬��

    private delegate void FuncStateEnter();

    private delegate void FuncStateUpdate(int n);

    private delegate void FuncStateExit(int n);

    private Dictionary<EnemyState, FuncStateEnter> dictStateEnter;
    private Dictionary<EnemyState, FuncStateUpdate> dictStateUpdate;
    private Dictionary<EnemyState, FuncStateExit> dictStateExit;

    public void OnAnimStateEnter(EnemyState s)
    {
        Debug.Log("����״̬" + s);
        dictStateEnter = new Dictionary<EnemyState, FuncStateEnter>
        {
            {EnemyState.Alert, AlertEnter},
            {EnemyState.Move, MoveEnter},
            {EnemyState.Attack, AttackEnter},
            {EnemyState.Sleeping, SleepingEnter},
            {EnemyState.GetHit, GetHitEnter},
            {EnemyState.Die, DieEnter},
        };

        if (dictStateEnter.ContainsKey(s) && dictStateEnter[s] != null)
        {
            dictStateEnter[s]();
        }
    }

    public void OnAnimStateUpdate(EnemyState s, int n)
    {
        dictStateUpdate = new Dictionary<EnemyState, FuncStateUpdate>
        {
            {EnemyState.Alert, AlertUpdate},
            {EnemyState.Move, MoveUpdate},
            {EnemyState.Attack, AttackUpdate},
            {EnemyState.Sleeping, SleepingUpdate},
            {EnemyState.GetHit, null},
            {EnemyState.Die, DieUpdate},
        };

        if (dictStateUpdate.ContainsKey(s) && dictStateUpdate[s] != null)
        {
            dictStateUpdate[s](n);
        }
    }

    public void OnAnimStateExit(EnemyState s, int n)
    {
        dictStateExit = new Dictionary<EnemyState, FuncStateExit>
        {
            {EnemyState.Move, null},
            {EnemyState.Attack, AttackExit},
            {EnemyState.Sleeping, null},
            {EnemyState.GetHit, GetHitExit},
            {EnemyState.Die, null},
        };

        if (dictStateExit.ContainsKey(s) && dictStateExit[s] != null)
        {
            dictStateExit[s](n);
        }
    }

    // ---------- Enter ----------

    public void AlertEnter()
    {
        state = EnemyState.Alert;
    }

    public void MoveEnter()
    {
        state = EnemyState.Move;
    }

    public void AttackEnter()
    {
        state = EnemyState.Attack;
    }

    public void GetHitEnter()
    {
        state = EnemyState.GetHit;
    }

    public void DieEnter()
    {
        state = EnemyState.Die;
        GameMode.Instance.enemyCount--;
        if (GameMode.Instance.enemyCount <= 0)
        {
            GameMode.Instance.Win();
        }
    }

    public void SleepingEnter()
    {
        state = EnemyState.Sleeping;
    }

    // ---------- Update ----------

    public void AlertUpdate(int n)
    {
        if (player.state == PlayerState.Die)
        {
            return;
        }
        // ��ȡ���λ�������Ϣ
        Vector3 playerPos = player.transform.position;
        Vector3 to = playerPos - transform.position;
        float dist = to.magnitude; //���˾���ҵľ���

        if (dist < alertDistance)
        {
            // ���뾯�������������
            Rotate(to);
            move = transform.forward;
            anim.SetBool("FindPlayer", true);
        }
    }

    public void MoveUpdate(int n)
    {
        if (player.state == PlayerState.Die)
        {
            // �����������������ƶ�
            move = Vector3.zero;
            anim.SetBool("FindPlayer", false);
            return;
        }

        // ��ȡ���λ�������Ϣ
        Vector3 playerPos = player.transform.position;
        Vector3 to = playerPos - transform.position;
        float dist = to.magnitude; //���˾���ҵľ���
        if (dist > trackingDistance)
        {
            // ������Զ׷�پ����ֹͣ�ƶ������뾯��״̬
            move = Vector3.zero;
            anim.SetBool("FindPlayer", false);
            return;
        }

        // ����ʱ������ƶ�
        Rotate(to);
        move = transform.forward;
        anim.SetBool("Move", true);

        if (dist <= stopDistance)
        {
            // ���빥����Χ��ֹͣ�ƶ�������
            anim.SetBool("Move", false);
            Attack();
        }
    }

    public void AttackUpdate(int n)
    {
    }

    public void SleepingUpdate(int n)
    {
        Vector3 playerPos = player.transform.position;
        Vector3 to = playerPos - transform.position;
        float dist = to.magnitude; //���˾���ҵľ���

        if (dist < alertDistance)
        {
            anim.SetTrigger("WakeUp");
        }
    }

    public void DieUpdate(int n)
    {
        if (dieTime < 0)
        {
            dieTime = Time.time;
        }
        if (Time.time > dieTime + disappearTime)
        {
            Destroy(gameObject);
        }
    }

    // ---------- Exit ----------

    public void AlertExit(int n)
    {
    }

    public void MoveExit(int n)
    {
    }

    public void RollExit(int n)
    {
    }

    public void AttackExit(int n)
    {
        // �˳�������������ֹһֱѭ��
        anim.SetInteger("Attack", 0);
        // ����ܻ������������а�����ʽ
        anim.ResetTrigger("GetHit");
    }

    public void GetHitExit(int n)
    {
        // ��ֹ�������ʱ����һֱӲֱ
        anim.ResetTrigger("GetHit");
    }

    #endregion ״̬��
}