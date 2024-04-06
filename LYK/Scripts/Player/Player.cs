using Glory.InventorySystem;
using PlayerOwnedStates;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public enum PlayerStates { Idle, Move, EnemyAttack, Interaction, Die, Dodge, Crouch }


public class Player : LivingEntity
{
    //�÷��̾� ���°���
    private State<Player>[] states;
    private State<Player> currentState;

    //���̽�ƽ
    public Joystick virtualJoystick;//���̽�ƽ

    //Ÿ�� ������Ʈ
    private Collider target;//Ÿ�� ��
    private Collider targetInteraction;//Ÿ�� ���ͷ��� ������Ʈ
    public Collider Target
    {
        get
        {
            if (target == null) return target;

            if (target.TryGetComponent(out LivingEntity targetOb))
            {
                if (!targetOb.Dead) return target;
                else
                {
                    target = null;
                    return target;
                }
            }
            else
            {
                target = null;
                return target;
            }
        }
        set
        {
            target = value;
        }
    }
    public Collider TargetInteraction
    {
        get
        {
            if (targetInteraction == null) return targetInteraction;

            if (targetInteraction.TryGetComponent(out InteractionObject targetOb))
            {
                if (!targetOb.Dead) return targetInteraction;
                else
                {
                    targetInteraction = null;
                    return targetInteraction;
                }
            }
            else
            {
                targetInteraction = null;
                return targetInteraction;
            }
        }
        set
        {
            targetInteraction = value;
        }
    }

    /// <summary> ���� ��� �θ� ��ġ / �ڽĵ� => 0=�ֹ���,1=����,2=ä��,3=���� </summary>
    public Transform weaponParentPosition;

    //��ƼŬ
    public ParticleSystem woodChopping;//������ƼŬ ������
    public ParticleSystem StoneMining;//ä����ƼŬ ������

    //���̾�
    public LayerMask intractionTargetLayer;
    public LayerMask enemyTargetLayer;
    public LayerMask buildingRoofLayer;

    
    [HideInInspector] public float lastFireTime;//���� ���������� �߻��� ����

    [SerializeField] private Transform interactionCircle;//���ͷ��� Ÿ�� ǥ�� ������(����)
    [SerializeField] private Transform attackCircle;//���� Ÿ�� ǥ�� ������(����)


    float shortDisEnemy = 1000;
    Collider shortEnemy = null;

    float shortDisInteraction = 1000;
    Collider shortInteraction = null;

    public Animator playerAnim { get; private set; }

    /// <summary> �ڽ� �̹��� => 0= Fish, 1 = Axe, 2= PickAxe, 3=Search </summary>
    [SerializeField] Button interactionBt;

    [HideInInspector] public bool isAttackBtPointerUp = false;

    public Transform respawnUI;


    private void Awake()
    {
        PlayerStatLYK.Instance.playerDeadEvent += Die;
        Init();
    }

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        if (Dead) return;
        if (currentState == null) return;

        currentState.Execute(this);
    }

    public void Init()
    {
        if (transform.TryGetComponent(out Animator animator)) playerAnim = animator;

        //Player�� ���� �� �ִ� ���� ������ŭ �޸� �Ҵ�, �� ������ Ŭ���� �޸� �Ҵ�
        states = new State<Player>[System.Enum.GetValues(typeof(PlayerStates)).Length];
        states[(int)PlayerStates.Idle] = new PlayerOwnedStates.Idle();
        states[(int)PlayerStates.Move] = new PlayerOwnedStates.Move();
        states[(int)PlayerStates.EnemyAttack] = new PlayerOwnedStates.EnemyAttack();
        states[(int)PlayerStates.Interaction] = new PlayerOwnedStates.Interaction();
        states[(int)PlayerStates.Die] = new PlayerOwnedStates.Die();

        states[(int)PlayerStates.Dodge] = new PlayerOwnedStates.Dodge();
        states[(int)PlayerStates.Crouch] = new PlayerOwnedStates.Crouch();

        currentState = states[(int)PlayerStates.Idle];//�÷��̾� ó�� ����
    }

    /// <summary> �÷��̾� �ʱ� ���� �� </summary>
    public void Setup()
    {

        //Ÿ�� ǥ�� ������ ����
        interactionCircle = Instantiate(interactionCircle, new Vector3(0, -100, 0), Quaternion.Euler(new Vector3(90, 0, 0))).transform;
        attackCircle = Instantiate(attackCircle, new Vector3(0, -100, 0), Quaternion.Euler(new Vector3(90, 0, 0))).transform;
        //��ƼŬ ����
        woodChopping = Instantiate(woodChopping, new Vector3(0, -100, 0), Quaternion.Euler(new Vector3(90, 0, 0))).GetComponent<ParticleSystem>();
        StoneMining = Instantiate(StoneMining, new Vector3(0, -100, 0), Quaternion.Euler(new Vector3(90, 0, 0))).GetComponent<ParticleSystem>();

        interactionBt.onClick.AddListener(TryInteractionState);

        //�÷��̾� ���� �ҷ�����
        PlayerStatLYK.Instance.CurrentLevel = DatabaseManager.Instance.databaseData.userLevel;
        PlayerStatLYK.Instance.CurrentHealth = DatabaseManager.Instance.databaseData.lastHP;
        PlayerStatLYK.Instance.Hungry = DatabaseManager.Instance.databaseData.lastHungryValue;
        PlayerStatLYK.Instance.Thirsty = DatabaseManager.Instance.databaseData.lastThirstyValue;
        PlayerStatLYK.Instance.CurrentExp = DatabaseManager.Instance.databaseData.playerExp;

        StartCoroutine(BuildingRoof());
        StartCoroutine(HungryCo());
        StartCoroutine(ThirstyCo());
    }

    /// <summary> �÷��̾� ���� �ൿ���� ��ȯ </summary>
    public PlayerStates CurrentPlayerState
    {
        get
        {
            int i = 0;

            foreach (var state in states)
            {
                if (state == currentState)
                {
                    //Debug.Log("PlayerState => " + (PlayerStates)i);
                    return (PlayerStates)i;
                }
                i += 1;
            }
            Debug.LogError("Error");
            return PlayerStates.Idle;
        }
    }

    /// <summary> �÷��̾� �ൿ���� �ٲٱ� </summary>
    public void ChangeState(PlayerStates newState)
    {
        if (Dead) return;
        //���� �ٲٷ��� ���°� ����ְų� �������¸� ����
        if (states[(int)newState] == null || currentState == states[(int)newState]) return;

        //���� ������� ���°� ������ Exit() �޼ҵ� ȣ��
        if (currentState != null)
        {
            currentState.Exit(this);
        }

        currentState = states[(int)newState];
        currentState.Enter(this);
    }


    /// <summary>���ͷ��� �ִϸ��̼� �̺�Ʈ/summary>
    public void InteractObjectEvent()
    {
        if (TargetInteraction == null) return;
        if (TargetInteraction.TryGetComponent(out InteractionObject interactionOb))
        {
            if (interactionOb is Tree)
            {
                //Tree Ÿ��
                if (woodChopping != null)
                {
                    woodChopping.transform.position = interactionOb.transform.position + new Vector3(0, 1, -0.5f);
                    woodChopping.Play();
                }

                ECCameraShake.Instance.CameraShaking(1f, 0.2f);
                float woodDamage = PlayerStatLYK.Instance.LumberingDamage.GetValue;
                interactionOb.InteractionEvent();
            }
            else if (interactionOb is Mineral)
            {
                //Mineral Ÿ��
                if (StoneMining != null)
                {
                    StoneMining.transform.position = interactionOb.transform.position + new Vector3(0, 1.5f, 0);
                    StoneMining.Play();
                }

                ECCameraShake.Instance.CameraShaking(1f, 0.2f);
                //float miningDamage = PlayerStatLYK.Instance.MiningDamage.GetValue;
                interactionOb.InteractionEvent();
            }
            else if (interactionOb is PickupInteraction)
            {
                interactionOb.InteractionEvent();
            }
            else if (interactionOb is Fishing)
            {
                interactionOb.InteractionEvent();
            }
            ChangeState(PlayerStates.Idle);
        }
    }

    /// <summary> LivingEntity.cs ��� -> �ǰ�ó�� �޼ҵ� </summary>
    public override void OnDamage(float damage, Transform attackObject = null)
    {
        if (Dead) return;

        //���ط� - ����
        float damageValue = damage - PlayerStatLYK.Instance.Defense.GetValue;
        if (damageValue <= 0)//���ط� < ����
        {
            damageValue = 1;
        }

        Debug.Log(PlayerStatLYK.Instance.CurrentHealth + "/" + damageValue);

        if (PlayerStatLYK.Instance.CurrentHealth - damageValue > 0)
        {
            PlayerStatLYK.Instance.CurrentHealth -= damageValue;
        }
        else
        {
            //����
            PlayerStatLYK.Instance.CurrentHealth = 0;
        }
    }

    /// <summary> ����ó�� �޼ҵ� </summary>
    public override void Die()
    {
        Debug.Log("Die!");
        ChangeState(PlayerStates.Die);
        base.Die();
    }

    /// <summary> ���� �ִϸ��̼� �̺�Ʈ </summary>
    public void AttackAnimEvent()
    {
        if (Target == null) return;
        if (Dead) return;

        if (Target.TryGetComponent(out LivingEntity attackTarget))
        {
            //���� ���� �̺�Ʈ������ �ҷ�����
            if (Inventory.Instance.EquipItemRetrun(4) != null)
            {
                if (Inventory.Instance.EquipItemRetrun(4).Equipmentdata.equipPrefab.TryGetComponent<WeaponInfo>(out WeaponInfo weaponInfo))
                {
                    weaponInfo.Shot(Target.transform);
                }
            }

            if ((WeaponAttackType)PlayerStatLYK.Instance.WeaponAttackType.GetValue == WeaponAttackType.SingleAttack)
            {
                //���ϰ���
                Debug.Log("SingleAttack");
                attackTarget.OnDamage(PlayerStatLYK.Instance.Damage.GetValue, this.transform);

            }
            else if ((WeaponAttackType)PlayerStatLYK.Instance.WeaponAttackType.GetValue == WeaponAttackType.RangeAttack)
            {
                //��������
                Debug.Log("RangeAttack");
                Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, PlayerStatLYK.Instance.AttackRange.GetValue, enemyTargetLayer);
                if (targetsInRadius.Length > 0)
                {
                    for (int i = 0; i < targetsInRadius.Length; i++)
                    {
                        Transform target = targetsInRadius[i].transform;
                        Vector3 dirToTarget = (target.position - transform.position).normalized;

                        //�÷��̾�� forward�� target�� �̷�� ���� ������ ���� �����
                        //Vector3.Angle => 0~180 �� ���̰��� ��ȯ => ���� ������ ���� ���� ������ ��ȯ�ϱ� ����
                        float viewAngle = 100;//=>�ӽ� ����
                        if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle * 0.5)
                        {
                            target.GetComponent<LivingEntity>().OnDamage(PlayerStatLYK.Instance.Damage.GetValue, this.transform);
                        }
                    }
                }

            }

            ECCameraShake.Instance.CameraShaking(1f, 0.2f);
            lastFireTime = Time.time;
        }

    }

    /// <summary> �÷��̾ �ǹ��� ���� �� �ǹ� ���� ���� </summary>
    IEnumerator BuildingRoof()
    {
        RaycastHit hitInfo;
        Transform bulidingRoofObject = null;
        WaitForSeconds waitTime = new WaitForSeconds(0.1f);

        while (true)
        {
            if (Physics.Raycast(this.transform.position, this.transform.up, out hitInfo, 5, buildingRoofLayer))
            {
                if (hitInfo.transform.TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
                {
                    Debug.Log("BuildingRoof=>RaycastHit");
                    if (bulidingRoofObject != null && bulidingRoofObject != hitInfo.transform)
                    {
                        Debug.Log("�ٸ�����");
                        //bulidingRoofObject.gameObject.SetActive(true);
                        bulidingRoofObject.transform.GetComponent<MeshRenderer>().enabled = true;
                        for (int i = 0; i < bulidingRoofObject.transform.childCount; i++)
                        {
                            bulidingRoofObject.transform.GetChild(i).gameObject.SetActive(true);
                        }
                        bulidingRoofObject.transform.parent.GetChild(1).gameObject.SetActive(false);//�ǹ� ���� ������Ʈ��
                    }
                    bulidingRoofObject = hitInfo.transform;
                    mesh.enabled = false;
                    for (int i = 0; i < bulidingRoofObject.transform.childCount; i++)
                    {
                        bulidingRoofObject.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }

                bulidingRoofObject.transform.parent.GetChild(1).gameObject.SetActive(true);//�ǹ� ���� ������Ʈ��
            }
            else
            {
                if (bulidingRoofObject != null)
                {
                    bulidingRoofObject.transform.GetComponent<MeshRenderer>().enabled = true;
                    for (int i = 0; i < bulidingRoofObject.transform.childCount; i++)
                    {
                        bulidingRoofObject.transform.GetChild(i).gameObject.SetActive(true);
                    }
                    bulidingRoofObject.transform.parent.GetChild(1).gameObject.SetActive(false);//�ǹ� ���� ������Ʈ��
                }
                bulidingRoofObject = null;
            }
            yield return waitTime;

        }
    }

    /// <summary> ���ͷ��� ������Ʈ ��Ī </summary>
    public void InteractionSearch()
    {
        shortDisInteraction = 1000;
        shortInteraction = null;
        TargetInteraction = null;

        Collider[] interactionColliders = Physics.OverlapSphere(transform.position, PlayerStatLYK.Instance.SearchRange.GetValue, intractionTargetLayer);
        if (interactionColliders.Length > 0)
        {
            //���� �ȿ� �ִ� ���ͷ��� ������Ʈ�� UI�� ǥ�� => ����
            //testImage.transform.position = Camera.main.WorldToScreenPoint(interactionColliders[0].transform.position + new Vector3(0, 0.15f, 0));

            foreach (Collider col in interactionColliders)
            {
                Vector3 offset = col.transform.position - transform.position;
                float distanceInteraction = offset.magnitude;

                if (distanceInteraction < PlayerStatLYK.Instance.InteractionRange.GetValue)
                {
                    if (distanceInteraction < shortDisInteraction)
                    {
                        shortDisInteraction = distanceInteraction;
                        shortInteraction = col;
                    }
                }
            }

            TargetInteraction = shortInteraction;//���尡��� ������Ʈ
            shortDisInteraction = 1000;
            shortInteraction = null;

            if (TargetInteraction != null)
            {
                interactionCircle.transform.localPosition = TargetInteraction.transform.position + new Vector3(0, 0.1f, 0);
                interactionCircle.transform.localScale = new Vector3(TargetInteraction.transform.localScale.x * 0.2f, TargetInteraction.transform.localScale.y * 0.2f, 1);

                //���ͷ��� Ÿ�Կ� ���� ��ưUI �̹��� ����
                if (TargetInteraction.TryGetComponent(out InteractionObject _interactionOb))
                {
                    for (int i = 0; i < interactionBt.transform.childCount; i++)
                    {
                        interactionBt.transform.GetChild(i).gameObject.SetActive(false);
                    }

                    if (_interactionOb.obType == InteractionObType.Fishing)
                    {
                        interactionBt.transform.GetChild(0).gameObject.SetActive(true);
                    }
                    else if (_interactionOb.obType == InteractionObType.Tree)
                    {
                        interactionBt.transform.GetChild(1).gameObject.SetActive(true);
                    }
                    else if (_interactionOb.obType == InteractionObType.Mining)
                    {
                        interactionBt.transform.GetChild(2).gameObject.SetActive(true);
                    }
                    else
                    {
                        interactionBt.transform.GetChild(3).gameObject.SetActive(true);
                    }

                }
            }
            else
            {
                interactionCircle.transform.localPosition = new Vector3(0, -100, 0);
            }

        }
        else
        {
            TargetInteraction = null;
            shortInteraction = null;
            shortDisInteraction = 1000;

            interactionCircle.transform.localPosition = new Vector3(0, -100, 0);
        }
    }


    /// <summary> �� ������Ʈ ��Ī </summary>
    public void EnemySearch()
    {
        shortDisEnemy = 1000;
        shortEnemy = null;
        Target = null;

        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, PlayerStatLYK.Instance.AttackRange.GetValue, enemyTargetLayer);
        if (enemyColliders.Length > 0)
        {
            //���� ����� ������Ʈ ã��
            foreach (Collider col in enemyColliders)
            {
                //���⼭ �����ɽ�Ʈ ���༭ ��ֹ������� ����ó��

                Vector3 offset = col.transform.position - transform.position;
                //float distanceEnemy = offset.sqrMagnitude;
                float distanceEnemy = offset.magnitude;

                if (distanceEnemy < shortDisEnemy)
                {
                    shortDisEnemy = distanceEnemy;
                    shortEnemy = col;
                }
            }

            Target = shortEnemy;//���� ����� ������Ʈ
            shortDisEnemy = 1000;
            shortEnemy = null;

            if (Target != null)
            {
                attackCircle.transform.localPosition = Target.transform.position + new Vector3(0, 0.1f, 0);
                attackCircle.transform.localScale = new Vector3(Target.transform.localScale.x * 0.2f, Target.transform.localScale.y * 0.2f, 1);
            }
        }
        else
        {
            Target = null;
            shortEnemy = null;
            shortDisEnemy = 1000;

            attackCircle.transform.localPosition = new Vector3(0, -100, 0);
        }
    }


    /// <summary> ���ͷ��� UI ��ư OnClick </summary>
    public void TryInteractionState()
    {
        if (!TargetInteraction) return;

        ChangeState(PlayerStates.Interaction);
    }

    /// <summary> ���� UI ��ư OnClick </summary>
    public void TryEnemyAttackState()
    {
        if (!Target) return;

        if (Time.time >= lastFireTime)
        {
            transform.LookAt(Target.transform);

            ChangeState(PlayerStates.EnemyAttack);
        }
    }

    /// <summary> ����� ó�� </summary>
    IEnumerator HungryCo()
    {
        //���Ŀ� �������� ���� ���ֱ�
        WaitForSeconds waitTime = new WaitForSeconds(20);//���ҽð�
        float hungryDamageValue = 10;//���ҷ�

        while (true)
        {
            if (Dead) break;
            yield return waitTime;
            PlayerStatLYK.Instance.Hungry -= hungryDamageValue;

        }
        yield return null;
    }

    /// <summary> �񸶸� ó�� </summary>
    IEnumerator ThirstyCo()
    {
        WaitForSeconds waitTime = new WaitForSeconds(20);//���ҽð�
        float thirstyDamageValue = 10;//���ҷ�

        while (true)
        {
            if (Dead) break;
            yield return waitTime;
            PlayerStatLYK.Instance.Thirsty -= thirstyDamageValue;
        }
        yield return null;
    }

    //ȸ��(������)
    public void Dodge()
    {
        if (currentState != states[(int)PlayerStates.Dodge])
        {
            playerAnim.SetBool("Crouch", false);
            playerAnim.SetBool("Move", false);
            ChangeState(PlayerStates.Dodge);
        }
    }

    //���,��ũ����
    public void Crouch()
    {
        if (playerAnim.GetBool("Crouch"))
        {
            playerAnim.SetBool("Crouch", false);
            ChangeState(PlayerStates.Idle);
        }
        else
        {
            ChangeState(PlayerStates.Crouch);
        }
    }

    /// <summary> ���ݹ�ư���� PointerUp�ϸ� ���� </summary>
    public void ChangeStateIdle()
    {
        isAttackBtPointerUp = true;
    }

    /// <summary>��Ȱ �̺�Ʈ </summary>
    public void Respawn()
    {
        Dead = false;
        ChangeState(PlayerStates.Idle);
        PlayerStatLYK.Instance.CurrentHealth = PlayerStatLYK.Instance.Health.GetValue;
        //playerAnim.SetBool("Die", false);
        playerAnim.SetTrigger("Entry");
        respawnUI.gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (EditorApplication.isPlaying)
        {
            //���ͷ��� ����(�����Ϳ�)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.5f, 0), (int)PlayerStatLYK.Instance.InteractionRange.GetValue);
            //�÷��̾� ���� ����(�����Ϳ�)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.5f, 0), (int)PlayerStatLYK.Instance.AttackRange.GetValue);

        }
#endif

    }
}