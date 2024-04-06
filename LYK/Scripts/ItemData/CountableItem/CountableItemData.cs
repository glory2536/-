using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class CountableItemData : ItemData
{
    public int maxAmount;//아이템 최대 저장 개수
    public string resourcesInfo;
    public Grade itemGrade;

    public override Item CreateItem()
    {
        return new Item(this);
    }
}