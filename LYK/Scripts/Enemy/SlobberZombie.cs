using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlobberZombie : Zombie
{
    [SerializeField] ParticleSystem poisonMuzzle;
    [SerializeField] GameObject poisonProjectile;
    [SerializeField] Transform muzzleStartPosition;

    /// <summary> ����_������</summary>
    private IEnumerator State_Idle()
    {
        //�������� 1ȸ ȣ��
        Debug.Log("NormalZombieState => Idle_Enter");
        isChange = true;
        targetEntity = null;
        pathFinder.isStopped = true;

        //���¹ݺ�
        while (isChange)
        {
            SearchTargets();
            yield return null;
        }

        //�������� 1ȸ ȣ��
        Debug.Log("NormalZombieState => Idle_Exit");
        ChangeState(nextState);

    }

    /// <summary> ����_�̵�����</summary>
    private IEnumerator State_Move()
    {
        //�������� 1ȸ ȣ��
        Debug.Log("NormalZombieState => Move_Enter");
        isChange = true;
        anim.SetBool("Move", true);
        pathFinder.isStopped = false;

        //���¹ݺ�
        while (isChange)
        {
            //Debug.Log("ZombieState => Move_Execute");
            if (hasTarget)
            {
                pathFinder.SetDestination(targetEntity.transform.position);

                float distance = (targetEntity.transform.position - transform.position).sqrMagnitude;

                if (distance > 150)
                {
                    //�ʹ� �־����� Idle���·� ��ȯ
                    isChange = false;
                    nextState = ZombieState.State_Idle;
                }
                else if (distance < attackDistance)
                {
                    //���ݰŸ��ȿ� ������ ����
                    nextState = ZombieState.State_Attack;
                    isChange = false;
                }
                if (hpbar != null) hpbar.HpBar(transform.position + new Vector3(0, 2f, 0), Currenthealth, MaxHealth);
            }
            else
            {
                targetEntity = null;
                nextState = ZombieState.State_Idle;
                isChange = false;
            }

            yield return null;
        }

        Debug.Log("ZombieState => Move_Exit");
        anim.SetBool("Move", false);
        ChangeState(nextState);
    }

    /// <summary> ����_���ݻ���</summary>
    private IEnumerator State_Attack()
    {
        Debug.Log("ZombieState => Attack_Enter");
        isChange = true;
        pathFinder.isStopped = true;
        anim.SetBool("Attack", true);

        //���¹ݺ�
        while (isChange)
        {
            if (hasTarget)
            {
                float distance = (targetEntity.transform.position - transform.position).magnitude;

                if (distance > 30)
                {
                    isChange = false;
                    nextState = ZombieState.State_Idle;
                }
                else if (distance < attackDistance)
                {
                    ZombieAttack();
                }
                else if (distance > attackDistance)
                {
                    isChange = false;
                    nextState = ZombieState.State_Move;
                }
            }
            else
            {
                nextState = ZombieState.State_Idle;
                isChange = false;
            }

            yield return null;
        }

        //�������� 1ȸ ȣ��
        anim.SetBool("Attack", false);
        ChangeState(nextState);
    }

    /// <summary> ����_��������</summary>
    private IEnumerator State_Die()
    {
        Debug.Log("ZombieState => Die_Enter");
        Dead = true;

        if (TryGetComponent(out CapsuleCollider capsuleCollider))
        {
            capsuleCollider.enabled = false;
        }
        else if (TryGetComponent(out BoxCollider boxCollider))
        {
            boxCollider.enabled = false;
        }

        pathFinder.isStopped = true;
        //pathFinder.enabled = false;
        anim.SetTrigger("Die");
        isChange = true;

        while (isChange)
        {
            //Debug.Log("ZombieState => Die_Execute");
            yield return null;
        }

    }

    public void AnimationEvent()
    {
        poisonMuzzle.Play();
        GameObject poisonParticle = Instantiate(poisonProjectile, muzzleStartPosition.position, Quaternion.identity) ;
        if(poisonParticle.TryGetComponent(out PoisonParticle _poison))
        {
            _poison.target = targetEntity.transform;
            _poison.SetUp(Damage);
        }
    }
}