using Glory.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace PlayerOwnedStates
{
    /// <summary> 대기상태 </summary>
    public class Idle : State<Player>
    {
        public override void Enter(Player entity)
        {
            Debug.Log("PlayerState->Idle_Enter");
            entity.weaponParentPosition.GetChild(0).gameObject.SetActive(true);//주무기 오브젝트
            entity.playerAnim.SetInteger("AttackType", (int)PlayerStatLYK.Instance.WeaponType.GetValue);
        }

        public override void Execute(Player entity)
        {
            //Debug.Log("PlayerState->Idle_Execute");
            entity.EnemySearch();
            entity.InteractionSearch();
        }

        public override void Exit(Player entity)
        {
            Debug.Log("PlayerState->Idle_Exit");
        }
    }

    /// <summary> 이동 상태 </summary>
    public class Move : State<Player>
    {
        float x = 0;
        float y = 0;

        public override void Enter(Player entity)
        {
            Debug.Log("PlayerState->Move_Enter");
            entity.weaponParentPosition.GetChild(0).gameObject.SetActive(true);
            entity.playerAnim.SetBool("Move", true);
            entity.playerAnim.SetInteger("AttackType", (int)PlayerStatLYK.Instance.WeaponType.GetValue);
        }

        public override void Execute(Player entity)
        {
            //Debug.Log("PlayerState->Move_Execute");

            x = entity.virtualJoystick.Horizontal();// => Left/Right
            y = entity.virtualJoystick.Vertical();// => Up/Down

            if (x != 0 || y != 0)
            {
                //시작 이속 기본값 4로 맞추기 => 100 * 0.04f => 4
                float moveSpeed = PlayerStatLYK.Instance.Speed.GetValue * 0.04f;

                //캐릭터가 반대로움직이면 + => -로바꿔주기
                entity.transform.position += new Vector3(x, 0, y) * Time.deltaTime * moveSpeed;
                entity.transform.forward = new Vector3(x, 0, y);//바라보는방향
            }

            entity.EnemySearch();
            entity.InteractionSearch();
        }

        public override void Exit(Player entity)
        {
            Debug.Log("PlayerState->Move_Exit");
            entity.playerAnim.SetBool("Move", false);
        }
    }

    /// <summary> 적 공격 상태 </summary>
    public class EnemyAttack : State<Player>
    {
        float animSpeed;

        public override void Enter(Player entity)
        {
            Debug.Log("PlayerState->EnemyAttack_Enter");

            animSpeed = 1 + (PlayerStatLYK.Instance.AttackSpeed.GetValue / 100);

            entity.weaponParentPosition.GetChild(0).gameObject.SetActive(true);//주무기 오브젝트
            entity.playerAnim.SetBool("Attack", true);//애니메이션 이벤트로 공격대기처리
            entity.playerAnim.SetInteger("AttackType", (int)PlayerStatLYK.Instance.WeaponType.GetValue);

            entity.playerAnim.SetFloat("AttackSpeed", animSpeed);//애니메이션 속도 조절
            entity.isAttackBtPointerUp = false;
        }

        public override void Execute(Player entity)
        {

            if (Time.time >= entity.lastFireTime)
            {
                //공격버튼 누르고있으면
                if (!entity.isAttackBtPointerUp)
                {
                    entity.EnemySearch();
                    if (entity.Target == null)
                    {
                        entity.ChangeState(PlayerStates.Idle);
                    }
                    else
                    {
                        //전체 공격 애니메이션 기본 속도 1초
                        animSpeed = 1 + (PlayerStatLYK.Instance.AttackSpeed.GetValue / 100);
                        entity.playerAnim.SetFloat("AttackSpeed", animSpeed);//애니메이션 속도 조절

                        entity.playerAnim.SetTrigger("AttackTrigger");//공격처리
                        entity.TryEnemyAttackState();
                    }
                }
                else
                {
                    entity.ChangeState(PlayerStates.Idle);
                }
            }
        }

        public override void Exit(Player entity)
        {
            Debug.Log("PlayerState->EnemyAttack_Exit");

            entity.Target = null;
            entity.playerAnim.SetBool("Attack", false);

            //장비 개별 이벤트 있으면 처리
            if (Inventory.Instance.EquipItemRetrun(4) != null)
            {
                if (Inventory.Instance.EquipItemRetrun(4).Equipmentdata.equipPrefab.TryGetComponent(out WeaponInfo weaponInfo))
                {
                    if (weaponInfo.muzzleFlashEffect == null) return;

                    weaponInfo.muzzleFlashEffect.Stop();
                }
            }
        }
    }

    /// <summary> 상호작용 상태 </summary>
    public class Interaction : State<Player>
    {
        Transform weaponPosition;

        public override void Enter(Player entity)
        {
            weaponPosition = null;
            entity.weaponParentPosition.GetChild(0).gameObject.SetActive(false);

            entity.transform.LookAt(entity.TargetInteraction.transform);

            //인터렉션 타입에 따라 이벤트 처리 / Switch?
            if (entity.TargetInteraction.TryGetComponent(out InteractionObject interactionOb))
            {
                if (interactionOb.obType == InteractionObType.Tree)
                {
                    Debug.Log("InteractionObject => Tree");
                    weaponPosition = entity.weaponParentPosition.GetChild(1);//도끼
                    entity.playerAnim.SetInteger("InteractionAttackType", 0);//벌목 애니메이션
                    entity.playerAnim.SetInteger("AttackType", 30);
                }
                else if (interactionOb.obType == InteractionObType.Mining)
                {
                    Debug.Log("InteractionObject => Mining");
                    weaponPosition = entity.weaponParentPosition.GetChild(2);//곡괭이
                    entity.playerAnim.SetInteger("InteractionAttackType", 1);//광물공격 애니메이션
                    entity.playerAnim.SetInteger("AttackType", 30);
                }
                else if (interactionOb.obType == InteractionObType.PickUp)
                {
                    Debug.Log("InteractionObject => PickUp");
                    entity.playerAnim.SetInteger("InteractionAttackType", 2);//픽업 애니메이션
                    entity.playerAnim.SetInteger("AttackType", 30);

                }
                else if (interactionOb.obType == InteractionObType.Fishing)
                {
                    Debug.Log("InteractionObject => Fishing");
                    weaponPosition = entity.weaponParentPosition.GetChild(3);//낚시대
                    entity.playerAnim.SetInteger("InteractionAttackType", 3);//낚시 애니메이션
                    entity.playerAnim.SetInteger("AttackType", 30);
                }
                else if (interactionOb.obType == InteractionObType.Building)
                {
                    entity.weaponParentPosition.GetChild(0).gameObject.SetActive(true);
                    interactionOb.InteractionEvent();
                }
                else if (interactionOb.obType == InteractionObType.Field)
                {
                    interactionOb.InteractionEvent();
                }

                if (weaponPosition != null) weaponPosition.gameObject.SetActive(true);
            }
        }

        public override void Execute(Player entity)
        {
            //throw new System.NotImplementedException();
        }

        public override void Exit(Player entity)
        {
            if (weaponPosition != null) weaponPosition.gameObject.SetActive(false);
        }
    }

    /// <summary> 죽음 행동 </summary>
    public class Die : State<Player>
    {
        public override void Enter(Player entity)
        {
            Debug.Log("PlayerState->Die_Enter");

            entity.Target = null;
            entity.playerAnim.SetTrigger("Die");

            //사망UI
            entity.respawnUI.gameObject.SetActive(true);
        }

        public override void Execute(Player entity)
        {
            //Debug.Log("PlayerState->Die_Execute");
        }

        public override void Exit(Player entity)
        {
            Debug.Log("PlayerState->Die_Exit");
        }
    }

    /// <summary> 구르기 행동 </summary>
    public class Dodge : State<Player>
    {
        float time = 0;

        public override void Enter(Player entity)
        {
            Debug.Log("Dodge_Enter");
            time = 0;
            entity.playerAnim.SetTrigger("Roll");

        }

        public override void Execute(Player entity)
        {
            time += Time.deltaTime;

            if (time < 0.33f)
            {
                entity.transform.position += entity.transform.forward * Time.deltaTime * 10;
            }
        }

        public override void Exit(Player entity)
        {
            Debug.Log("Dodge_Exit");
            time = 0;
        }
    }

    /// <summary> 잠입/웅크리기 행동 </summary>
    public class Crouch : State<Player>
    {
        public override void Enter(Player entity)
        {
            Debug.Log("Crouch_Enter");
            entity.playerAnim.SetBool("Crouch", true);
        }

        public override void Execute(Player entity)
        {
            //Debug.Log("Crouch_Execute");
        }

        public override void Exit(Player entity)
        {
            Debug.Log("Crouch_Exit");
        }
    }
}