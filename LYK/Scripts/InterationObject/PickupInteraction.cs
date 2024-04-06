using Glory.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupInteraction : InteractionObject
{
    public DropItemInfo dropItemInfo;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        Dead = false;
        obType = InteractionObType.PickUp;
    }

    /// <summary> 인터렉션 버튼 </summary>
    public override void InteractionEvent()
    {
        if (dropItemInfo.itemKey >= 0) dropItemInfo.dropItemData = ItemDataMG.Instance.GetItemData(dropItemInfo.itemKey);

        if (dropItemInfo.itemKey != 0)
        {
            ObjectPoolManager.Instance.GetDropItemPool(this.gameObject.transform, dropItemInfo);
        }

        this.gameObject.SetActive(false);
    }
}
