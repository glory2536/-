using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary> 머리장비 데이터 정보 </summary>
[Serializable]
public class HeadData : EquipmentData
{
    //머리 고유 스탯(4)
    StatModifier headDamage;//공격력
    StatModifier headHealth;//체력
    StatModifier headDefense;//방어력
    StatModifier headDodgeChance;//회피율

    public HeadData(int _ID, Grade _equipGrade, string _equipHeadName, int _equipHeadHealth, int _equipHeadDefense, int _equipHeadDodgeChance, int _equipHeadDamage, GameObject _equipHeadPrefab, Sprite _itemSprite)
    {
        //공용스탯(아이템/장비)(6)
        itemID = _ID;
        itemName = _equipHeadName;
        itemSprite = _itemSprite;
        equipGrade = _equipGrade;
        equipPrefab = _equipHeadPrefab;
        equipType = EquipType.EquipHead;

        //머리 고유 스탯 생성(4)
        headDamage = new StatModifier(_equipHeadDamage, StatModType.Float, PlayerStatLYK.Instance.Damage.statIndex);
        headHealth = new StatModifier(_equipHeadHealth, StatModType.Float, PlayerStatLYK.Instance.Health.statIndex);
        headDefense = new StatModifier(_equipHeadDefense, StatModType.Float, PlayerStatLYK.Instance.Defense.statIndex);
        headDodgeChance = new StatModifier(_equipHeadDodgeChance, StatModType.Float, PlayerStatLYK.Instance.Evasion.statIndex);

        //머리 고유 스탯 리스트에 추가(4)
        equipStatList.Add(headDamage);
        equipStatList.Add(headHealth);
        equipStatList.Add(headDefense);
        equipStatList.Add(headDodgeChance);

        for (int i = equipStatList.Count - 1; i >= 0; i--)
        {
            if (equipStatList[i].statValue == 0)
            {
                equipStatList.RemoveAt(i);
            }
        }
    }

    public override Item CreateItem()
    {
        return new HeadItem(this);
    }
}