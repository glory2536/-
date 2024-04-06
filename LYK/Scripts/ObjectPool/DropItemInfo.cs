using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DropItemInfo
{
    [SerializeReference]
    public ItemData dropItemData;//키값?

    /// <summary> 장비는 = 1, 수량있는 아이템은 1이상 </summary>
    public int itemAmount = 1;
    public int itemKey;

    public DropItemInfo(ItemData _dropItemData, int _itemKey, int _itemAmount = 1)
    {
        dropItemData = _dropItemData;
        itemAmount = _itemAmount;
        itemKey = _itemKey;
    }

    /// <summary> 아이템 정보 넘겨주기 </summary>
    public void GetItemData(int _itemIndex)
    {
        itemKey = _itemIndex;
        dropItemData = ItemDataMG.Instance.GetItemData(_itemIndex);
    }

}