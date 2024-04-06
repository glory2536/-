using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> �����۵��_Enum </summary>
public enum Grade
{
    //������ -> �ʷλ� -> �Ķ��� -> ����� -> �����
    Null,
    C,
    B,
    A,
    S
}


/*
 ������ ���� ����
ItemData.cs -> EquipmentData.cs     -> WeaponData.cs, BodyData.cs,HeaData.cs, HandData.cs, ShoesData.cs, ResourceWeaponData.cs
            -> CountableItemData.cs -> ResourcesData.cs, ItemPotion.cs
*/
[Serializable]
public abstract class ItemData
{
    public int itemID;//������ ���� �ε���
    public String itemName;//������ �̸�
    public Sprite itemSprite;//������ �̹���

    /// <summary> Ÿ�Կ� �´� ���ο� ������ ���� </summary>
    public abstract Item CreateItem();
}
