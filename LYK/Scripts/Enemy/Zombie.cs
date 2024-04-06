using Glory.InventorySystem;
using Glory.ObjectPool;
using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using static UnityEngine.EventSystems.EventTrigger;
//using UnityEngine.UIElements;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum ZombieState { State_Idle, State_Move, State_Attack, State_Die }

public class Zombie : LivingEntity
{
    //스탯
    protected int enemyIndex;
    public float MaxHealth { get; protected set; }
    public float Defense { get; protected set; }
    public float Damage { get; protected set; }
    public float Speed { get; protected set; }
    public float Exp { get; protected set; }
    public int DropItemIndex { get; protected set; }

    [Range(1, 20)]
    public float attackRadius = 5;
    [Range(1, 30)]
    public float attackDistance = 2;
    [Range(0, 360)]
    public float attackAngle;
    public float AttackSpeed { get; protected set; }
    [field: SerializeField] public float Currenthealth { get; protected set; }
    protected float lastAttackTime;

    //공격대상(레이어,객체)
    public LayerMask targetLayer;
    protected Player targetEntity;

    //컴포넌트
    protected Animator anim;
    protected NavMeshAgent pathFinder;
    protected EnemyHpBar hpbar;

    //상태
    protected ZombieState zombieState;//현재 상태
    protected bool isChange = true;
    protected ZombieState nextState;//다음 행동 상태

    protected List<Transform> visibleTargets = new List<Transform>();
    //오브젝트 거리계산 관련 변수
    float shortDisEnemy = 1000;//가장 가까운 플레이어 거리
    Transform shortEnemy = null;//가장 가까운 플레이어
    Vector3 distanceOffset;//플레이어와 거리편차
    public LayerMask obstacleMask;//장애물 레이어마스크


    public bool hasTarget
    {
        get
        {
            if (targetEntity != null && targetEntity.TryGetComponent(out Player Player))// && targetEntity.CurrentPlayerState != PlayerStates.Die
            {
                if (Player.Dead)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }

    protected void Awake()
    {
        Init();
    }

    void Init()
    {
        pathFinder = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        ChangeState(ZombieState.State_Idle);//시작 상태
    }

    /// <summary> 좀비_스텟처리(구글시트) </summary>
    public void SetupStat(EnemyData enemyBaseData)
    {
        //생성자로 만들어주면 좋을 듯
        enemyIndex = enemyBaseData.enemyIndex;
        MaxHealth = enemyBaseData.maxHealth;
        Currenthealth = MaxHealth;
        Defense = enemyBaseData.defense;
        Damage = enemyBaseData.damage;
        Speed = enemyBaseData.speed;
        pathFinder.speed = Speed;
        Exp = enemyBaseData.exp;
        DropItemIndex = enemyBaseData.dropItemIndex;
        attackRadius = enemyBaseData.attackRadius;
        attackDistance = enemyBaseData.attackDistance;
        attackAngle = enemyBaseData.attackAngle;
        AttackSpeed = enemyBaseData.attackSpeed;
    }

    /// <summary> 좀비_상태변경 </summary>
    protected void ChangeState(ZombieState newState)
    {
        if (Dead) return;

        isChange = false;
        //새로운 상태로 변경
        zombieState = newState;
        //현재 상태의 코루틴 실행
        StartCoroutine(zombieState.ToString());
    }

    /// <summary> 피격 처리 메서드 </summary>
    public override void OnDamage(float damage, Transform attackObject = null)
    {
        base.OnDamage(damage);

        //HpBar
        if (hpbar == null)
        {
            hpbar = ObjectPoolManager.Instance.GetHPBarPool();
        }

        float damageValue = damage - Defense;
        if (damageValue <= 0)//피해량 < 방어력
        {
            damageValue = 1;
        }

        //피격처리
        if (Currenthealth - damageValue > 0)
        {
            Currenthealth -= damageValue;
            //DamagePopUp
            ObjectPoolManager.Instance.DamagePopUpPool(transform.position + new Vector3(0, 2, 0), (int)damage);

            if (zombieState == ZombieState.State_Idle)
            {
                if (attackObject != null)
                {
                    //추격
                    if (attackObject.TryGetComponent<Player>(out Player _player))
                    {
                        targetEntity = _player;
                        isChange = false;
                        nextState = ZombieState.State_Move;
                    }
                }
            }
            hpbar.HpBar(transform.position + new Vector3(0, 2f, 0), Currenthealth, MaxHealth);
        }
        else
        {
            //죽음처리
            Currenthealth = 0;
            if (attackObject != null)
            {
                //추격
                if (attackObject.TryGetComponent<Player>(out Player _player))
                {
                    targetEntity = _player;
                }
            }
            hpbar.HpBar(transform.position + new Vector3(0, 2f, 0), 0, MaxHealth);
            Die();
        }


    }

    /// <summary> 죽음상태처리 </summary>
    public override void Die()
    {
        ObjectPoolManager.Instance.Despawn(hpbar);
        hpbar = null;
        StopAllCoroutines();

        PlayerStatLYK.Instance.CurrentExp += Exp;//경험치 증가
        DropItem();
        ChangeState(ZombieState.State_Die);
        base.Die();
    }

    /// <summary> 공격처리 </summary>
    public void ZombieAttack()
    {
        if (hasTarget)
        {
            if (Time.time >= lastAttackTime + AttackSpeed)
            {
                if (Dead) return;
                pathFinder.isStopped = true;
                lastAttackTime = Time.time;
                this.transform.LookAt(targetEntity.transform);
                anim.SetTrigger("AttackTrigger");
            }
        }
    }

    /// <summary> 공격 애니메이션 이벤트 </summary>
    public void ZombieAttackAnimEvent()
    {
        if (hasTarget)
        {
            targetEntity.OnDamage(Damage);
        }
    }

    protected void SearchTargets()
    {
        visibleTargets.Clear();
        targetEntity = null;

        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, attackRadius, targetLayer);

        if (targetsInRadius.Length < 1) return;

        for (int i = 0; i < targetsInRadius.Length; i++)
        {
            Transform target = targetsInRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            //Vector3.Angle => 0~180 도 사이값만 반환
            if (Vector3.Angle(transform.forward, dirToTarget) < attackAngle * 0.5)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }

        if (visibleTargets.Count > 0)
        {
            shortDisEnemy = 1000;
            shortEnemy = null;

            for (int i = 0; i < visibleTargets.Count; i++)
            {
                distanceOffset = visibleTargets[i].transform.position - transform.position;
                float distance = distanceOffset.sqrMagnitude;

                if (distance < shortDisEnemy)
                {
                    shortDisEnemy = distance;
                    shortEnemy = visibleTargets[i];
                }

            }
            if (shortEnemy.TryGetComponent(out Player player))
            {
                targetEntity = player;

                if (zombieState != ZombieState.State_Move)
                {
                    nextState = ZombieState.State_Move;
                    isChange = false;
                }
            }
        }
    }

    public void DropItem()
    {
        ItemData dropItem = null;
        DropItemIndex = 17; //=> 테스트
        if (DropItemIndex >= 0) dropItem = ItemDataMG.Instance.GetItemData(DropItemIndex);

        if (dropItem != null)
        {
            DropItemInfo dropItemInfo = new(dropItem, DropItemIndex);
            ObjectPoolManager.Instance.GetDropItemPool(transform, dropItemInfo);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.5f, 0), attackRadius);
    }
}