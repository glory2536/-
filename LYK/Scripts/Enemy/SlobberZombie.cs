using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlobberZombie : Zombie
{
    [SerializeField] ParticleSystem poisonMuzzle;
    [SerializeField] GameObject poisonProjectile;
    [SerializeField] Transform muzzleStartPosition;

    /// <summary> 좀비_대기상태</summary>
    private IEnumerator State_Idle()
    {
        //상태진입 1회 호출
        Debug.Log("NormalZombieState => Idle_Enter");
        isChange = true;
        targetEntity = null;
        pathFinder.isStopped = true;

        //상태반복
        while (isChange)
        {
            SearchTargets();
            yield return null;
        }

        //상태종료 1회 호출
        Debug.Log("NormalZombieState => Idle_Exit");
        ChangeState(nextState);

    }

    /// <summary> 좀비_이동상태</summary>
    private IEnumerator State_Move()
    {
        //상태진입 1회 호출
        Debug.Log("NormalZombieState => Move_Enter");
        isChange = true;
        anim.SetBool("Move", true);
        pathFinder.isStopped = false;

        //상태반복
        while (isChange)
        {
            //Debug.Log("ZombieState => Move_Execute");
            if (hasTarget)
            {
                pathFinder.SetDestination(targetEntity.transform.position);

                float distance = (targetEntity.transform.position - transform.position).sqrMagnitude;

                if (distance > 150)
                {
                    //너무 멀어지면 Idle상태로 전환
                    isChange = false;
                    nextState = ZombieState.State_Idle;
                }
                else if (distance < attackDistance)
                {
                    //공격거리안에 들어오면 공격
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

    /// <summary> 좀비_공격상태</summary>
    private IEnumerator State_Attack()
    {
        Debug.Log("ZombieState => Attack_Enter");
        isChange = true;
        pathFinder.isStopped = true;
        anim.SetBool("Attack", true);

        //상태반복
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

        //상태종료 1회 호출
        anim.SetBool("Attack", false);
        ChangeState(nextState);
    }

    /// <summary> 좀비_죽음상태</summary>
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