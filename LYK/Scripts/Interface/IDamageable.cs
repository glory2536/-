using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 데미지 입는 모든 오브젝트에 들어가는 인터페이스 </summary>
public interface IDamageable
{
    void OnDamage(float damage);
}
