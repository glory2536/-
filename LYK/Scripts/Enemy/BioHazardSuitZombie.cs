using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BioHazardSuitZombie : Zombie
{
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private SkinnedMeshRenderer rendererMesh;

    /// <summary> ����_������</summary>
    private IEnumerator State_Idle()
    {
        Debug.Log("ZombieState => Idle_Enter");
        isChange = true;
        targetEntity = null;
        pathFinder.isStopped = true;

        //���¹ݺ�
        while (isChange)
        {
            //Debug.Log("ZombieState => Idle_Execute");
            SearchTargets();
            yield return null;
        }

        Debug.Log("ZombieState => Idle_Exit");
        ChangeState(nextState);
    }

    /// <summary> ����_�̵�����</summary>
    private IEnumerator State_Move()
    {
        Debug.Log("ZombieState => Move_Enter");
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
        ZombieAttack();

        while (isChange)
        {
            //Debug.Log("ZombieState => Attack_Execute");

            if (hasTarget)
            {
                //pathFinder.SetDestination(targetEntity.transform.position);

                float distance = (targetEntity.transform.position - transform.position).sqrMagnitude;

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

        anim.SetBool("Attack", false);
        ChangeState(nextState);
    }

    //3�ʰ� ����+ �̼� 20���� ���� + 3���� ����
    /// <summary> ����_��������</summary>
    private IEnumerator State_Die()
    {
        Debug.Log("BioHazardSuitZombieState => Die_Enter");
        pathFinder.isStopped = false;
        isChange = true;
        pathFinder.speed = pathFinder.speed + pathFinder.speed * 0.5f;
        float time = 0;

        while (isChange)
        {
            //Debug.Log("ZombieState => Die_Execute");
            if (time > 3)
            {
                //����
                isChange = false;
                break;
            }
            //���߰�
            pathFinder.SetDestination(targetEntity.transform.position);

            //������
            if (rendererMesh.materials[0].color == Color.red)
            {
                rendererMesh.materials[0].color = Color.white;
            }
            else
            {
                rendererMesh.materials[0].color = Color.red;
            }

            time += Time.deltaTime;
            yield return null;
        }

        //������ƼŬ
        explosionParticle.Play();

        float explosionRadius = 2;
        Collider[] players = Physics.OverlapSphere(this.transform.position, explosionRadius, targetLayer);
        if (players.Length > 0)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].TryGetComponent(out Player player))
                {
                    player.OnDamage(Damage * 3);//�⺻������ X 3��
                }
            }
        }

        GetComponent<CapsuleCollider>().enabled = false;
        pathFinder.isStopped = true;
        pathFinder.enabled = false;
        anim.SetTrigger("Die");
    }
}