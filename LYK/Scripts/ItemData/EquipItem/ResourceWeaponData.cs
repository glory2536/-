using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary> 인터렉션 장비 데이터 정보 </summary>
[Serializable]
public class ResourceWeaponData : EquipmentData
{
    //무기 장비 스탯(2)
    StatModifier woodDamage;//벌목 공격력
    StatModifier miningDamage;//채굴 공격력

    public ResourceWeaponData(int _ID, Grade _equipGrade, string _weaponName, int _woodDamage, int _miningDamage, Sprite _itemSprite, GameObject _weaponPrefab)
    {
        //공용스탯(아이템/장비)(7)
        itemID = _ID;
        itemName = _weaponName;
        itemSprite = _itemSprite;
        equipGrade = _equipGrade;
        equipPrefab = _weaponPrefab;

        if (_woodDamage > 0 && _miningDamage == 0)
        {
            equipType = EquipType.WoodWeapon;
        }
        else if (_woodDamage == 0 && _miningDamage > 0)
        {
            equipType = EquipType.MiningWeapon;
        }


        //자원무기 고유 스탯 생성(2)
        woodDamage = new StatModifier(_woodDamage, StatModType.Float, PlayerStatLYK.Instance.LumberingDamage.statIndex);
        miningDamage = new StatModifier(_miningDamage, StatModType.Float, PlayerStatLYK.Instance.MiningDamage.statIndex);

        //자원무기 고유 스탯 리스트에 추가(2)
        equipStatList.Add(woodDamage);
        equipStatList.Add(miningDamage);

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
        return new ResourceWeaponItem(this);
    }
}