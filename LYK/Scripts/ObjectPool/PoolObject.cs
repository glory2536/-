using Glory.InventorySystem;
using Glory.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using KeyType = System.String;


public class PoolObject : MonoBehaviour
{
    public KeyType key;


    #region 오브젝트 복제 기능
    /// <summary> 게임오브젝트 복제 </summary>
    public PoolObject Clone(Transform parentPosition)
    {
        GameObject cloneObject = Instantiate(gameObject);
        if (!cloneObject.TryGetComponent(out PoolObject po))
            po = cloneObject.AddComponent<PoolObject>();
        cloneObject.transform.parent = parentPosition;
        cloneObject.SetActive(false);

        return po;
    }
    #endregion

    #region 오브젝트 활성화
    /// <summary> 게임오브젝트 활성화 </summary>
    public virtual void Activate()
    {
        gameObject.SetActive(true);
    }
    #endregion

    #region 오브젝트 비활성화
    /// /// <summary> 게임오브젝트 비활성화 </summary>
    public virtual void Deactivate()
    {
        gameObject.SetActive(false);
        //ObjectPoolManager.Instance.Despawn(this);
    }
    #endregion

}
