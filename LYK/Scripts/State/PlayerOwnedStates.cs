using Glory.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace PlayerOwnedStates
{
    /// <summary> ������ </summary>
    public class Idle : State<Player>
    {
        public override void Enter(Player entity)
        {
            Debug.Log("PlayerState->Idle_Enter");
            entity.weaponParentPosition.GetChild(0).gameObject.SetActive(true);//�ֹ��� ������Ʈ
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

    /// <summary> �̵� ���� </summary>
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
                //���� �̼� �⺻�� 4�� ���߱� => 100 * 0.04f => 4
                float moveSpeed = PlayerStatLYK.Instance.Speed.GetValue * 0.04f;

                //ĳ���Ͱ� �ݴ�ο����̸� + => -�ιٲ��ֱ�
                entity.transform.position += new Vector3(x, 0, y) * Time.deltaTime * moveSpeed;
                entity.transform.forward = new Vector3(x, 0, y);//�ٶ󺸴¹���
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

    /// <summary> �� ���� ���� </summary>
    public class EnemyAttack : State<Player>
    {
        float animSpeed;

        public override void Enter(Player entity)
        {
            Debug.Log("PlayerState->EnemyAttack_Enter");

            animSpeed = 1 + (PlayerStatLYK.Instance.AttackSpeed.GetValue / 100);

            entity.weaponParentPosition.GetChild(0).gameObject.SetActive(true);//�ֹ��� ������Ʈ
            entity.playerAnim.SetBool("Attack", true);//�ִϸ��̼� �̺�Ʈ�� ���ݴ��ó��
            entity.playerAnim.SetInteger("AttackType", (int)PlayerStatLYK.Instance.WeaponType.GetValue);

            entity.playerAnim.SetFloat("AttackSpeed", animSpeed);//�ִϸ��̼� �ӵ� ����
            entity.isAttackBtPointerUp = false;
        }

        public override void Execute(Player entity)
        {

            if (Time.time >= entity.lastFireTime)
            {
                //���ݹ�ư ������������
                if (!entity.isAttackBtPointerUp)
                {
                    entity.EnemySearch();
                    if (entity.Target == null)
                    {
                        entity.ChangeState(PlayerStates.Idle);
                    }
                    else
                    {
                        //��ü ���� �ִϸ��̼� �⺻ �ӵ� 1��
                        animSpeed = 1 + (PlayerStatLYK.Instance.AttackSpeed.GetValue / 100);
                        entity.playerAnim.SetFloat("AttackSpeed", animSpeed);//�ִϸ��̼� �ӵ� ����

                        entity.playerAnim.SetTrigger("AttackTrigger");//����ó��
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

            //��� ���� �̺�Ʈ ������ ó��
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

    /// <summary> ��ȣ�ۿ� ���� </summary>
    public class Interaction : State<Player>
    {
        Transform weaponPosition;

        public override void Enter(Player entity)
        {
            weaponPosition = null;
            entity.weaponParentPosition.GetChild(0).gameObject.SetActive(false);

            entity.transform.LookAt(entity.TargetInteraction.transform);

            //���ͷ��� Ÿ�Կ� ���� �̺�Ʈ ó�� / Switch?
            if (entity.TargetInteraction.TryGetComponent(out InteractionObject interactionOb))
            {
                if (interactionOb.obType == InteractionObType.Tree)
                {
                    Debug.Log("InteractionObject => Tree");
                    weaponPosition = entity.weaponParentPosition.GetChild(1);//����
                    entity.playerAnim.SetInteger("InteractionAttackType", 0);//���� �ִϸ��̼�
                    entity.playerAnim.SetInteger("AttackType", 30);
                }
                else if (interactionOb.obType == InteractionObType.Mining)
                {
                    Debug.Log("InteractionObject => Mining");
                    weaponPosition = entity.weaponParentPosition.GetChild(2);//���
                    entity.playerAnim.SetInteger("InteractionAttackType", 1);//�������� �ִϸ��̼�
                    entity.playerAnim.SetInteger("AttackType", 30);
                }
                else if (interactionOb.obType == InteractionObType.PickUp)
                {
                    Debug.Log("InteractionObject => PickUp");
                    entity.playerAnim.SetInteger("InteractionAttackType", 2);//�Ⱦ� �ִϸ��̼�
                    entity.playerAnim.SetInteger("AttackType", 30);

                }
                else if (interactionOb.obType == InteractionObType.Fishing)
                {
                    Debug.Log("InteractionObject => Fishing");
                    weaponPosition = entity.weaponParentPosition.GetChild(3);//���ô�
                    entity.playerAnim.SetInteger("InteractionAttackType", 3);//���� �ִϸ��̼�
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

    /// <summary> ���� �ൿ </summary>
    public class Die : State<Player>
    {
        public override void Enter(Player entity)
        {
            Debug.Log("PlayerState->Die_Enter");

            entity.Target = null;
            entity.playerAnim.SetTrigger("Die");

            //���UI
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

    /// <summary> ������ �ൿ </summary>
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

    /// <summary> ����/��ũ���� �ൿ </summary>
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