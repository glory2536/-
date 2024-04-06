using Glory.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldItem : InteractionObject
{
    [SerializeField] private DropItemInfo dropItem;
    [SerializeField] private DropItemInfo dropItem2;
    [SerializeField] private Vector3 dropItemPos;
    [SerializeField] private Vector3 dropItemPos2;

    private void Start()
    {
        obType = InteractionObType.Field;
    }

    /// <summary> ���ͷ��� ��ư </summary>
    public override void InteractionEvent()
    {
        if (Dead) return;

        Dead = true;
        if (TryGetComponent(out Animator anim))
        {
            anim.enabled = true;
        }
        if (TryGetComponent(out BoxCollider collider))
        {
            collider.enabled = false;
        }

        //������1
        dropItem.dropItemData = ItemDataMG.Instance.GetItemData(dropItem.itemKey);
        ObjectPoolManager.Instance.GetDropItemPool(transform, dropItem, dropItemPos);
        //������2
        dropItem2.dropItemData = ItemDataMG.Instance.GetItemData(dropItem2.itemKey);
        ObjectPoolManager.Instance.GetDropItemPool(transform, dropItem2, dropItemPos2);
    }
}
