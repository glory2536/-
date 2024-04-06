using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary> 노말좀비 </summary>
public class NormalZombie : Zombie
{

    /// <summary> 좀비_대기상태</summary>
    private IEnumerator State_Idle()
    {
        Debug.Log("NormalZombieState => Idle_Enter");
        isChange = true;
        targetEntity = null;
        pathFinder.isStopped = true;

        while (isChange)
        {
            //Debug.Log("ZombieState => Idle_Execute");
            SearchTargets();
            yield return null;
        }

        Debug.Log("NormalZombieState => Idle_Exit");
        ChangeState(nextState);//종료 1회 호출 편하게 하려고 nextState로 받아와서 실행

    }

    /// <summary> 좀비_이동상태</summary>
    private IEnumerator State_Move()
    {
        Debug.Log("NormalZombieState => Move_Enter");
        isChange = true;
        anim.SetBool("Move", true);
        pathFinder.isStopped = false;

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
        //상태진입 1회 호출
        Debug.Log("ZombieState => Attack_Enter");
        isChange = true;
        pathFinder.isStopped = true;
        anim.SetBool("Attack", true);

        //상태반복
        while (isChange)
        {
            if (hasTarget)
            {
                //pathFinder.SetDestination(targetEntity.transform.position);

                float distance = (targetEntity.transform.position - transform.position).magnitude;

                if (distance > 150)
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
        //상태진입 1회 호출
        Debug.Log("ZombieState => Die_Enter");
        Dead = true;       
        
        if(TryGetComponent(out CapsuleCollider capsuleCollider))
        {
            capsuleCollider.enabled = false;
        }
        else if(TryGetComponent(out BoxCollider boxCollider))
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
}