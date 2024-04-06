using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ����ü�� �����ϴ� ��� Ŭ������ ��ӹ޾ƾ��ϴ� �ֻ���Ŭ����
/// ���� ��� �̸� ���� => ü��, �ǰ�, ��� ���, ��� �̺�Ʈ ����
/// </summary>
public class LivingEntity : MonoBehaviour//, IDamageable
{
    public bool Dead { get; protected set; }//��� ����

    /// <summary> Ȱ��ȭ�� �� ���� ���� </summary>
    protected virtual void OnEnable()
    {
        Dead = false;
        //currenthealth = maxHealth;
    }

    /// <summary> ������� �Դ� ���(�ǰ�) </summary>
    public virtual void OnDamage(float damage, Transform attackObject = null)
    {
        if (Dead) return;//�̹� ��������� ����ó��
        /*
        currenthealth -= damage;
        
        //ü���� 0 ���� && ���� ���� �ʾҴٸ� ��� ó�� ����
        if(currenthealth <= 0 && !dead)
        {
            Die();
        }
        */
    }

    /// <summary> ��� ó�� </summary>
    public virtual void Die()
    {
        if (Dead) return;//�̹� ��������� ����

        Dead = true;//��� ���¸� true�� ����
    }
}