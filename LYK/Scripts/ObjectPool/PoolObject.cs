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


    #region ������Ʈ ���� ���
    /// <summary> ���ӿ�����Ʈ ���� </summary>
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

    #region ������Ʈ Ȱ��ȭ
    /// <summary> ���ӿ�����Ʈ Ȱ��ȭ </summary>
    public virtual void Activate()
    {
        gameObject.SetActive(true);
    }
    #endregion

    #region ������Ʈ ��Ȱ��ȭ
    /// /// <summary> ���ӿ�����Ʈ ��Ȱ��ȭ </summary>
    public virtual void Deactivate()
    {
        gameObject.SetActive(false);
        //ObjectPoolManager.Instance.Despawn(this);
    }
    #endregion

}
