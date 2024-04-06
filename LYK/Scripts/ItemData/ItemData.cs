using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 아이템등급_Enum </summary>
public enum Grade
{
    //검은색 -> 초로색 -> 파란색 -> 보라색 -> 노란색
    Null,
    C,
    B,
    A,
    S
}


/*
 데이터 저장 구조
ItemData.cs -> EquipmentData.cs     -> WeaponData.cs, BodyData.cs,HeaData.cs, HandData.cs, ShoesData.cs, ResourceWeaponData.cs
            -> CountableItemData.cs -> ResourcesData.cs, ItemPotion.cs
*/
[Serializable]
public abstract class ItemData
{
    public int itemID;//아이템 고유 인덱스
    public String itemName;//아이템 이름
    public Sprite itemSprite;//아이템 이미지

    /// <summary> 타입에 맞는 새로운 아이템 생성 </summary>
    public abstract Item CreateItem();
}
