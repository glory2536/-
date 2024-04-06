using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary> 신발장비 데이터 정보 </summary>
[Serializable]
public class ShoesData : EquipmentData
{
    //신발 고유 스탯(5)
    StatModifier shoesDamage;//공격력
    StatModifier shoesHealth;//체력
    StatModifier shoesDefense;//방어력
    StatModifier shoesDodgeChance;//회피율
    StatModifier shoesMoveSpeed;//이동속도

    public ShoesData(int _ID, Grade _equipGrade, string _equipShoesName, int _equipShoesHealth, int _equipShoesDefense, int _equipShoesDodgeChance, int _equipShoesDamage, int _equipShoesMoveSpeed, GameObject _equipShoesPrefab, Sprite _itemSprite)
    {
        //공용스탯(아이템/장비)(6)
        itemID = _ID;
        itemName = _equipShoesName;
        itemSprite = _itemSprite;
        equipGrade = _equipGrade;
        equipPrefab = _equipShoesPrefab;
        equipType = EquipType.EquipShoes;

        //신발 고유 스탯 생성(5)
        shoesDamage = new StatModifier(_equipShoesDamage, StatModType.Float, PlayerStatLYK.Instance.Damage.statIndex);
        shoesHealth = new StatModifier(_equipShoesHealth, StatModType.Float, PlayerStatLYK.Instance.Health.statIndex);
        shoesDefense = new StatModifier(_equipShoesDefense, StatModType.Float, PlayerStatLYK.Instance.Defense.statIndex);
        shoesDodgeChance = new StatModifier(_equipShoesDodgeChance, StatModType.Float, PlayerStatLYK.Instance.Evasion.statIndex);
        shoesMoveSpeed = new StatModifier(_equipShoesMoveSpeed, StatModType.Float, PlayerStatLYK.Instance.Speed.statIndex);

        //신발 고유 스탯 리스트에 추가(5)
        equipStatList.Add(shoesDamage);
        equipStatList.Add(shoesHealth);
        equipStatList.Add(shoesDefense);
        equipStatList.Add(shoesDodgeChance);
        equipStatList.Add(shoesMoveSpeed);

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
        return new ShoesItem(this);
    }
}