using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Sleeping,           // 待机或伪装状态
    Alert,              // 警戒
    Move,               // 移动
    Attack,             // 攻击
    GetHit,             // 受击
    Die                 // 死亡
}

public class EnemyCharacter : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rigid;

    public EnemyState state = EnemyState.Sleeping;

    private PlayerCharacter player;

    public int hp = 100;                        // 生命值
    public int souls = 100;                     // 掉落魂量
    public int moveSpeed = 2;                   // 移动速度
    public float alertDistance = 10f;           // 发现玩家的最大距离
    public float trackingDistance = 20f;        // 发现玩家后的最大追踪距离
    public float stopDistance = 2f;             // 移动到据玩家该距离时停止移动

    private float disappearTime = 6f;           // 到尸体消失的时间

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

    // 敌人转向目标
    private void Rotate(Vector3 dir)
    {
        Quaternion faceToQuat = Quaternion.LookRotation(dir); //角色面朝目标方向的四元数
        Quaternion slerp = Quaternion.Slerp(transform.rotation, faceToQuat, 0.2f);
        transform.rotation = slerp;
    }

    // 攻击
    private void Attack()
    {
        // 随机选取一种攻击
        int ran = Random.Range(1, 4);
        anim.SetInteger("Attack", ran);
    }

    // 受击
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

    // 死亡相关处理
    public void Die()
    {
        anim.SetTrigger("Die");
        GetComponent<Collider>().isTrigger = true;
        rigid.isKinematic = true;
    }

    // 动画状态机驱动脚本状态机

    #region 状态机

    private delegate void FuncStateEnter();

    private delegate void FuncStateUpdate(int n);

    private delegate void FuncStateExit(int n);

    private Dictionary<EnemyState, FuncStateEnter> dictStateEnter;
    private Dictionary<EnemyState, FuncStateUpdate> dictStateUpdate;
    private Dictionary<EnemyState, FuncStateExit> dictStateExit;

    public void OnAnimStateEnter(EnemyState s)
    {
        Debug.Log("进入状态" + s);
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
        // 获取玩家位置相关信息
        Vector3 playerPos = player.transform.position;
        Vector3 to = playerPos - transform.position;
        float dist = to.magnitude; //敌人距玩家的距离

        if (dist < alertDistance)
        {
            // 进入警戒距离后锁定玩家
            Rotate(to);
            move = transform.forward;
            anim.SetBool("FindPlayer", true);
        }
    }

    public void MoveUpdate(int n)
    {
        if (player.state == PlayerState.Die)
        {
            // 玩家死亡后不再向玩家移动
            move = Vector3.zero;
            anim.SetBool("FindPlayer", false);
            return;
        }

        // 获取玩家位置相关信息
        Vector3 playerPos = player.transform.position;
        Vector3 to = playerPos - transform.position;
        float dist = to.magnitude; //敌人距玩家的距离
        if (dist > trackingDistance)
        {
            // 超过最远追踪距离后停止移动，进入警戒状态
            move = Vector3.zero;
            anim.SetBool("FindPlayer", false);
            return;
        }

        // 索敌时朝玩家移动
        Rotate(to);
        move = transform.forward;
        anim.SetBool("Move", true);

        if (dist <= stopDistance)
        {
            // 进入攻击范围后停止移动，攻击
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
        float dist = to.magnitude; //敌人距玩家的距离

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
        // 退出攻击动作，防止一直循环
        anim.SetInteger("Attack", 0);
        // 清空受击触发，怪物有霸体招式
        anim.ResetTrigger("GetHit");
    }

    public void GetHitExit(int n)
    {
        // 防止玩家连击时怪物一直硬直
        anim.ResetTrigger("GetHit");
    }

    #endregion 状态机
}