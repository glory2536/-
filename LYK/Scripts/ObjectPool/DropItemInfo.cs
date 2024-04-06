using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DropItemInfo
{
    [SerializeReference]
    public ItemData dropItemData;//Ű��?

    /// <summary> ���� = 1, �����ִ� �������� 1�̻� </summary>
    public int itemAmount = 1;
    public int itemKey;

    public DropItemInfo(ItemData _dropItemData, int _itemKey, int _itemAmount = 1)
    {
        dropItemData = _dropItemData;
        itemAmount = _itemAmount;
        itemKey = _itemKey;
    }

    /// <summary> ������ ���� �Ѱ��ֱ� </summary>
    public void GetItemData(int _itemIndex)
    {
        itemKey = _itemIndex;
        dropItemData = ItemDataMG.Instance.GetItemData(_itemIndex);
    }

}