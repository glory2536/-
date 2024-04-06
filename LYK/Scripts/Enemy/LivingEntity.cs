using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 생명체로 동작하는 모든 클래스가 상속받아야하는 최상위클래스
/// 공통 기능 미리 구현 => 체력, 피격, 사망 기능, 사망 이벤트 제공
/// </summary>
public class LivingEntity : MonoBehaviour//, IDamageable
{
    public bool Dead { get; protected set; }//사망 상태

    /// <summary> 활성화될 때 상태 리셋 </summary>
    protected virtual void OnEnable()
    {
        Dead = false;
        //currenthealth = maxHealth;
    }

    /// <summary> 대미지를 입는 기능(피격) </summary>
    public virtual void OnDamage(float damage, Transform attackObject = null)
    {
        if (Dead) return;//이미 사망했으면 리턴처리
        /*
        currenthealth -= damage;
        
        //체력이 0 이하 && 아직 죽지 않았다면 사망 처리 실행
        if(currenthealth <= 0 && !dead)
        {
            Die();
        }
        */
    }

    /// <summary> 사망 처리 </summary>
    public virtual void Die()
    {
        if (Dead) return;//이미 사망했으면 리턴

        Dead = true;//사망 상태를 true로 변경
    }
}