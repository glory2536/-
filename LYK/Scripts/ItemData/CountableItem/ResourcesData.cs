using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;


[Serializable]
public class ResourcesData : CountableItemData
{

    public ResourcesData(int _ID, string _ResourceName, int _MaxAmount, string _ResourcesInfo, Sprite _itemSprite,int _itemGrade)
    {
        itemID = _ID;
        itemName = _ResourceName;
        maxAmount = _MaxAmount;
        resourcesInfo = _ResourcesInfo;
        itemSprite = _itemSprite;
        itemGrade = (Grade)_itemGrade;
    }

    public override Item CreateItem()
    {
        return new ResourceItem(this);
    }
}