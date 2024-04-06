using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class CountableItemData : ItemData
{
    public int maxAmount;//������ �ִ� ���� ����
    public string resourcesInfo;
    public Grade itemGrade;

    public override Item CreateItem()
    {
        return new Item(this);
    }
}