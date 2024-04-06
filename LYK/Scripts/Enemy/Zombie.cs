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
    //����
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

    //���ݴ��(���̾�,��ü)
    public LayerMask targetLayer;
    protected Player targetEntity;

    //������Ʈ
    protected Animator anim;
    protected NavMeshAgent pathFinder;
    protected EnemyHpBar hpbar;

    //����
    protected ZombieState zombieState;//���� ����
    protected bool isChange = true;
    protected ZombieState nextState;//���� �ൿ ����

    protected List<Transform> visibleTargets = new List<Transform>();
    //������Ʈ �Ÿ���� ���� ����
    float shortDisEnemy = 1000;//���� ����� �÷��̾� �Ÿ�
    Transform shortEnemy = null;//���� ����� �÷��̾�
    Vector3 distanceOffset;//�÷��̾�� �Ÿ�����
    public LayerMask obstacleMask;//��ֹ� ���̾��ũ


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
        ChangeState(ZombieState.State_Idle);//���� ����
    }

    /// <summary> ����_����ó��(���۽�Ʈ) </summary>
    public void SetupStat(EnemyData enemyBaseData)
    {
        //�����ڷ� ������ָ� ���� ��
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

    /// <summary> ����_���º��� </summary>
    protected void ChangeState(ZombieState newState)
    {
        if (Dead) return;

        isChange = false;
        //���ο� ���·� ����
        zombieState = newState;
        //���� ������ �ڷ�ƾ ����
        StartCoroutine(zombieState.ToString());
    }

    /// <summary> �ǰ� ó�� �޼��� </summary>
    public override void OnDamage(float damage, Transform attackObject = null)
    {
        base.OnDamage(damage);

        //HpBar
        if (hpbar == null)
        {
            hpbar = ObjectPoolManager.Instance.GetHPBarPool();
        }

        float damageValue = damage - Defense;
        if (damageValue <= 0)//���ط� < ����
        {
            damageValue = 1;
        }

        //�ǰ�ó��
        if (Currenthealth - damageValue > 0)
        {
            Currenthealth -= damageValue;
            //DamagePopUp
            ObjectPoolManager.Instance.DamagePopUpPool(transform.position + new Vector3(0, 2, 0), (int)damage);

            if (zombieState == ZombieState.State_Idle)
            {
                if (attackObject != null)
                {
                    //�߰�
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
            //����ó��
            Currenthealth = 0;
            if (attackObject != null)
            {
                //�߰�
                if (attackObject.TryGetComponent<Player>(out Player _player))
                {
                    targetEntity = _player;
                }
            }
            hpbar.HpBar(transform.position + new Vector3(0, 2f, 0), 0, MaxHealth);
            Die();
        }


    }

    /// <summary> ��������ó�� </summary>
    public override void Die()
    {
        ObjectPoolManager.Instance.Despawn(hpbar);
        hpbar = null;
        StopAllCoroutines();

        PlayerStatLYK.Instance.CurrentExp += Exp;//����ġ ����
        DropItem();
        ChangeState(ZombieState.State_Die);
        base.Die();
    }

    /// <summary> ����ó�� </summary>
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

    /// <summary> ���� �ִϸ��̼� �̺�Ʈ </summary>
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

            //Vector3.Angle => 0~180 �� ���̰��� ��ȯ
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
        DropItemIndex = 17; //=> �׽�Ʈ
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