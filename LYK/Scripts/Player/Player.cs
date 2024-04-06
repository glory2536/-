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
    //플레이어 상태관리
    private State<Player>[] states;
    private State<Player> currentState;

    //조이스틱
    public Joystick virtualJoystick;//조이스틱

    //타겟 오브젝트
    private Collider target;//타겟 적
    private Collider targetInteraction;//타겟 인터렉션 오브젝트
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

    /// <summary> 무기 장비 부모 위치 / 자식들 => 0=주무기,1=벌목,2=채굴,3=낚시 </summary>
    public Transform weaponParentPosition;

    //파티클
    public ParticleSystem woodChopping;//벌목파티클 프리팹
    public ParticleSystem StoneMining;//채굴파티클 프리팹

    //레이어
    public LayerMask intractionTargetLayer;
    public LayerMask enemyTargetLayer;
    public LayerMask buildingRoofLayer;

    
    [HideInInspector] public float lastFireTime;//총을 마지막으로 발사한 시점

    [SerializeField] private Transform interactionCircle;//인터렉션 타겟 표시 프리팹(원형)
    [SerializeField] private Transform attackCircle;//공격 타겟 표시 프리팹(원형)


    float shortDisEnemy = 1000;
    Collider shortEnemy = null;

    float shortDisInteraction = 1000;
    Collider shortInteraction = null;

    public Animator playerAnim { get; private set; }

    /// <summary> 자식 이미지 => 0= Fish, 1 = Axe, 2= PickAxe, 3=Search </summary>
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

        //Player가 가질 수 있는 상태 개수만큼 메모리 할당, 각 상태의 클래스 메모리 할당
        states = new State<Player>[System.Enum.GetValues(typeof(PlayerStates)).Length];
        states[(int)PlayerStates.Idle] = new PlayerOwnedStates.Idle();
        states[(int)PlayerStates.Move] = new PlayerOwnedStates.Move();
        states[(int)PlayerStates.EnemyAttack] = new PlayerOwnedStates.EnemyAttack();
        states[(int)PlayerStates.Interaction] = new PlayerOwnedStates.Interaction();
        states[(int)PlayerStates.Die] = new PlayerOwnedStates.Die();

        states[(int)PlayerStates.Dodge] = new PlayerOwnedStates.Dodge();
        states[(int)PlayerStates.Crouch] = new PlayerOwnedStates.Crouch();

        currentState = states[(int)PlayerStates.Idle];//플레이어 처음 상태
    }

    /// <summary> 플레이어 초기 설정 값 </summary>
    public void Setup()
    {

        //타겟 표시 프리팹 생성
        interactionCircle = Instantiate(interactionCircle, new Vector3(0, -100, 0), Quaternion.Euler(new Vector3(90, 0, 0))).transform;
        attackCircle = Instantiate(attackCircle, new Vector3(0, -100, 0), Quaternion.Euler(new Vector3(90, 0, 0))).transform;
        //파티클 생성
        woodChopping = Instantiate(woodChopping, new Vector3(0, -100, 0), Quaternion.Euler(new Vector3(90, 0, 0))).GetComponent<ParticleSystem>();
        StoneMining = Instantiate(StoneMining, new Vector3(0, -100, 0), Quaternion.Euler(new Vector3(90, 0, 0))).GetComponent<ParticleSystem>();

        interactionBt.onClick.AddListener(TryInteractionState);

        //플레이어 스탯 불러오기
        PlayerStatLYK.Instance.CurrentLevel = DatabaseManager.Instance.databaseData.userLevel;
        PlayerStatLYK.Instance.CurrentHealth = DatabaseManager.Instance.databaseData.lastHP;
        PlayerStatLYK.Instance.Hungry = DatabaseManager.Instance.databaseData.lastHungryValue;
        PlayerStatLYK.Instance.Thirsty = DatabaseManager.Instance.databaseData.lastThirstyValue;
        PlayerStatLYK.Instance.CurrentExp = DatabaseManager.Instance.databaseData.playerExp;

        StartCoroutine(BuildingRoof());
        StartCoroutine(HungryCo());
        StartCoroutine(ThirstyCo());
    }

    /// <summary> 플레이어 현재 행동상태 반환 </summary>
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

    /// <summary> 플레이어 행동상태 바꾸기 </summary>
    public void ChangeState(PlayerStates newState)
    {
        if (Dead) return;
        //새로 바꾸려는 상태가 비어있거나 같은상태면 리턴
        if (states[(int)newState] == null || currentState == states[(int)newState]) return;

        //현재 재생중인 상태가 있으면 Exit() 메소드 호출
        if (currentState != null)
        {
            currentState.Exit(this);
        }

        currentState = states[(int)newState];
        currentState.Enter(this);
    }


    /// <summary>인터렉션 애니메이션 이벤트/summary>
    public void InteractObjectEvent()
    {
        if (TargetInteraction == null) return;
        if (TargetInteraction.TryGetComponent(out InteractionObject interactionOb))
        {
            if (interactionOb is Tree)
            {
                //Tree 타격
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
                //Mineral 타격
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

    /// <summary> LivingEntity.cs 상속 -> 피격처리 메소드 </summary>
    public override void OnDamage(float damage, Transform attackObject = null)
    {
        if (Dead) return;

        //피해량 - 방어력
        float damageValue = damage - PlayerStatLYK.Instance.Defense.GetValue;
        if (damageValue <= 0)//피해량 < 방어력
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
            //죽음
            PlayerStatLYK.Instance.CurrentHealth = 0;
        }
    }

    /// <summary> 죽음처리 메소드 </summary>
    public override void Die()
    {
        Debug.Log("Die!");
        ChangeState(PlayerStates.Die);
        base.Die();
    }

    /// <summary> 공격 애니메이션 이벤트 </summary>
    public void AttackAnimEvent()
    {
        if (Target == null) return;
        if (Dead) return;

        if (Target.TryGetComponent(out LivingEntity attackTarget))
        {
            //무기 개별 이벤트있으면 불러오기
            if (Inventory.Instance.EquipItemRetrun(4) != null)
            {
                if (Inventory.Instance.EquipItemRetrun(4).Equipmentdata.equipPrefab.TryGetComponent<WeaponInfo>(out WeaponInfo weaponInfo))
                {
                    weaponInfo.Shot(Target.transform);
                }
            }

            if ((WeaponAttackType)PlayerStatLYK.Instance.WeaponAttackType.GetValue == WeaponAttackType.SingleAttack)
            {
                //단일공격
                Debug.Log("SingleAttack");
                attackTarget.OnDamage(PlayerStatLYK.Instance.Damage.GetValue, this.transform);

            }
            else if ((WeaponAttackType)PlayerStatLYK.Instance.WeaponAttackType.GetValue == WeaponAttackType.RangeAttack)
            {
                //범위공격
                Debug.Log("RangeAttack");
                Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, PlayerStatLYK.Instance.AttackRange.GetValue, enemyTargetLayer);
                if (targetsInRadius.Length > 0)
                {
                    for (int i = 0; i < targetsInRadius.Length; i++)
                    {
                        Transform target = targetsInRadius[i].transform;
                        Vector3 dirToTarget = (target.position - transform.position).normalized;

                        //플레이어와 forward와 target이 이루는 각이 설장한 각도 내라면
                        //Vector3.Angle => 0~180 도 사이값만 반환 => 벡터 사이의 가장 작은 각도를 반환하기 때문
                        float viewAngle = 100;//=>임시 각도
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

    /// <summary> 플레이어가 건물에 들어갔을 때 건물 지붕 제거 </summary>
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
                        Debug.Log("다른지붕");
                        //bulidingRoofObject.gameObject.SetActive(true);
                        bulidingRoofObject.transform.GetComponent<MeshRenderer>().enabled = true;
                        for (int i = 0; i < bulidingRoofObject.transform.childCount; i++)
                        {
                            bulidingRoofObject.transform.GetChild(i).gameObject.SetActive(true);
                        }
                        bulidingRoofObject.transform.parent.GetChild(1).gameObject.SetActive(false);//건물 내부 오브젝트들
                    }
                    bulidingRoofObject = hitInfo.transform;
                    mesh.enabled = false;
                    for (int i = 0; i < bulidingRoofObject.transform.childCount; i++)
                    {
                        bulidingRoofObject.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }

                bulidingRoofObject.transform.parent.GetChild(1).gameObject.SetActive(true);//건물 내부 오브젝트들
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
                    bulidingRoofObject.transform.parent.GetChild(1).gameObject.SetActive(false);//건물 내부 오브젝트들
                }
                bulidingRoofObject = null;
            }
            yield return waitTime;

        }
    }

    /// <summary> 인터렉션 오브젝트 서칭 </summary>
    public void InteractionSearch()
    {
        shortDisInteraction = 1000;
        shortInteraction = null;
        TargetInteraction = null;

        Collider[] interactionColliders = Physics.OverlapSphere(transform.position, PlayerStatLYK.Instance.SearchRange.GetValue, intractionTargetLayer);
        if (interactionColliders.Length > 0)
        {
            //범위 안에 있는 인터렉션 오브젝트들 UI로 표시 => 미정
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

            TargetInteraction = shortInteraction;//가장가까운 오브젝트
            shortDisInteraction = 1000;
            shortInteraction = null;

            if (TargetInteraction != null)
            {
                interactionCircle.transform.localPosition = TargetInteraction.transform.position + new Vector3(0, 0.1f, 0);
                interactionCircle.transform.localScale = new Vector3(TargetInteraction.transform.localScale.x * 0.2f, TargetInteraction.transform.localScale.y * 0.2f, 1);

                //인터렉션 타입에 따라 버튼UI 이미지 변경
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


    /// <summary> 적 오브젝트 서칭 </summary>
    public void EnemySearch()
    {
        shortDisEnemy = 1000;
        shortEnemy = null;
        Target = null;

        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, PlayerStatLYK.Instance.AttackRange.GetValue, enemyTargetLayer);
        if (enemyColliders.Length > 0)
        {
            //가장 가까운 오브젝트 찾기
            foreach (Collider col in enemyColliders)
            {
                //여기서 레이케스트 쏴줘서 장애물있으면 리턴처리

                Vector3 offset = col.transform.position - transform.position;
                //float distanceEnemy = offset.sqrMagnitude;
                float distanceEnemy = offset.magnitude;

                if (distanceEnemy < shortDisEnemy)
                {
                    shortDisEnemy = distanceEnemy;
                    shortEnemy = col;
                }
            }

            Target = shortEnemy;//가장 가까운 오브젝트
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


    /// <summary> 인터렉션 UI 버튼 OnClick </summary>
    public void TryInteractionState()
    {
        if (!TargetInteraction) return;

        ChangeState(PlayerStates.Interaction);
    }

    /// <summary> 공격 UI 버튼 OnClick </summary>
    public void TryEnemyAttackState()
    {
        if (!Target) return;

        if (Time.time >= lastFireTime)
        {
            transform.LookAt(Target.transform);

            ChangeState(PlayerStates.EnemyAttack);
        }
    }

    /// <summary> 배고픔 처리 </summary>
    IEnumerator HungryCo()
    {
        //추후에 스탯으로 따로 빼주기
        WaitForSeconds waitTime = new WaitForSeconds(20);//감소시간
        float hungryDamageValue = 10;//감소량

        while (true)
        {
            if (Dead) break;
            yield return waitTime;
            PlayerStatLYK.Instance.Hungry -= hungryDamageValue;

        }
        yield return null;
    }

    /// <summary> 목마름 처리 </summary>
    IEnumerator ThirstyCo()
    {
        WaitForSeconds waitTime = new WaitForSeconds(20);//감소시간
        float thirstyDamageValue = 10;//감소량

        while (true)
        {
            if (Dead) break;
            yield return waitTime;
            PlayerStatLYK.Instance.Thirsty -= thirstyDamageValue;
        }
        yield return null;
    }

    //회피(구르기)
    public void Dodge()
    {
        if (currentState != states[(int)PlayerStates.Dodge])
        {
            playerAnim.SetBool("Crouch", false);
            playerAnim.SetBool("Move", false);
            ChangeState(PlayerStates.Dodge);
        }
    }

    //기습,웅크리기
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

    /// <summary> 공격버튼에서 PointerUp하면 실행 </summary>
    public void ChangeStateIdle()
    {
        isAttackBtPointerUp = true;
    }

    /// <summary>부활 이벤트 </summary>
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
            //인터렉션 범위(에디터용)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.5f, 0), (int)PlayerStatLYK.Instance.InteractionRange.GetValue);
            //플레이어 공격 범위(에디터용)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.5f, 0), (int)PlayerStatLYK.Instance.AttackRange.GetValue);

        }
#endif

    }
}