using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary> 손장비 데이터 정보 </summary>
[Serializable]
public class HandData : EquipmentData
{
    //손 고유 스탯(5)
    StatModifier handDamage;//공격력
    StatModifier handHealth;//체력
    StatModifier handDefense;//방어력
    StatModifier handDodgeChance;//회피율
    StatModifier handAttackSpeed;//공격속도

    public HandData(int _ID, Grade _equipGrade, string _equipHandName, int _equipHandHealth, int _equipHandDefense, int _equipHandDodgeChance, int _equipHandDamage, int _equipHandAttackSpeed, GameObject _equipHandPrefab, Sprite _itemSprite)
    {
        //공용스탯(아이템/장비)(6)
        itemID = _ID;
        itemName = _equipHandName;
        itemSprite = _itemSprite;
        equipGrade = _equipGrade;
        equipPrefab = _equipHandPrefab;
        equipType = EquipType.EquipHand;

        //손 고유 스탯 생성(5)
        handDamage = new StatModifier(_equipHandDamage, StatModType.Float, PlayerStatLYK.Instance.Damage.statIndex);
        handHealth = new StatModifier(_equipHandHealth, StatModType.Float, PlayerStatLYK.Instance.Health.statIndex);
        handDefense = new StatModifier(_equipHandDefense, StatModType.Float, PlayerStatLYK.Instance.Defense.statIndex);
        handDodgeChance = new StatModifier(_equipHandDodgeChance, StatModType.Float, PlayerStatLYK.Instance.Evasion.statIndex);
        handAttackSpeed = new StatModifier(_equipHandAttackSpeed, StatModType.Float, PlayerStatLYK.Instance.AttackSpeed.statIndex);


        //머리 고유 스탯 리스트에 추가(5)
        equipStatList.Add(handDamage);
        equipStatList.Add(handHealth);
        equipStatList.Add(handDefense);
        equipStatList.Add(handDodgeChance);
        equipStatList.Add(handAttackSpeed);

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
        return new HandItem(this);
    }
}