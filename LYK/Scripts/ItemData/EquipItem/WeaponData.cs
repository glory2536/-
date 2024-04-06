using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

/// <summary> 무기타입 </summary>
public enum WeaponType
{
    None,
    MeleeOneHand,
    MeleeTwoHand,
    Thrown,
    Pistol,
    ShotGun,
    SniperRifle,
    SMG,
    Bow,
    Rifle,
    Special
}

public enum WeaponAttackType
{
    None,
    SingleAttack,
    RangeAttack
}

/// <summary> 무기데이터 정보 </summary>
[Serializable]
public class WeaponData : EquipmentData
{
    //무기 장비 스탯(10)
    StatModifier weaponDamage;//공격력
    StatModifier weaponCriticalChance;//치명타확률
    StatModifier weaponcriticalDamage;//치명타데미지
    StatModifier weaponAttackSpeed;//공격속도
    StatModifier weaponAttackRange;//공격범위
    StatModifier weaponDefense;//방어력
    StatModifier weaponType;//무기타입
    StatModifier weaponAttackType;//무기공격타입

    public WeaponData(int _ID, WeaponType _weaponType, Grade _equipGrade, string _weaponName, int _weaponDamage, float _attackSpeed, float _attackRange, GameObject _weaponPrefab, Sprite _itemSprite
        , int _defense, int _criticalStrikeChance, int _criticalDamage, int _weaponAttackType)
    {
        //공용스탯(아이템/장비)(7)
        itemID = _ID;
        itemName = _weaponName;
        itemSprite = _itemSprite;
        equipGrade = _equipGrade;
        equipPrefab = _weaponPrefab;
        equipType = EquipType.EquipWeapon;

        //무기 고유 스탯 생성(7)
        weaponDamage = new StatModifier(_weaponDamage, StatModType.Float, PlayerStatLYK.Instance.Damage.statIndex);
        weaponCriticalChance = new StatModifier(_criticalStrikeChance, StatModType.Float, PlayerStatLYK.Instance.CriticalChance.statIndex);
        weaponcriticalDamage = new StatModifier(_criticalDamage, StatModType.Float, PlayerStatLYK.Instance.CriticalHit.statIndex);
        weaponAttackSpeed = new StatModifier(_attackSpeed, StatModType.Float, PlayerStatLYK.Instance.AttackSpeed.statIndex);
        weaponAttackRange = new StatModifier(_attackRange, StatModType.Float, PlayerStatLYK.Instance.AttackRange.statIndex);
        weaponDefense = new StatModifier(_defense, StatModType.Float, PlayerStatLYK.Instance.Defense.statIndex);
        weaponType = new StatModifier((int)_weaponType, StatModType.Float, PlayerStatLYK.Instance.WeaponType.statIndex);
        weaponAttackType = new StatModifier((int)_weaponAttackType, StatModType.Float, PlayerStatLYK.Instance.WeaponAttackType.statIndex);

        //무기 고유 스탯 리스트에 추가(7)
        equipStatList.Add(weaponDamage);
        equipStatList.Add(weaponCriticalChance);
        equipStatList.Add(weaponcriticalDamage);
        equipStatList.Add(weaponAttackSpeed);
        equipStatList.Add(weaponAttackRange);
        equipStatList.Add(weaponDefense);
        equipStatList.Add(weaponType);
        equipStatList.Add(weaponAttackType);

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
        return new WeaponItem(this);
    }
}