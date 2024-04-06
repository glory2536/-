using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class PotionItemData : CountableItemData
{
    public int healValue;
    public int hungryValue;
    public int thirstyValue;
    
    public PotionItemData(int _ID, string _ResourceName, int _MaxAmount, int _healValue, int _hungryValue, int _thirstyValue, Sprite _itemSprite, int _itemGrade)
    {
        itemID = _ID;
        itemName = _ResourceName;
        maxAmount = _MaxAmount;
        healValue = _healValue;
        hungryValue = _hungryValue;
        thirstyValue = _thirstyValue;
        itemSprite = _itemSprite;
        itemGrade = (Grade)_itemGrade;
    }

    public override Item CreateItem()
    {
        return new PotionItem(this);
    }
}