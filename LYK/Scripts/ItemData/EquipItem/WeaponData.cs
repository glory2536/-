using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

/// <summary> ����Ÿ�� </summary>
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

/// <summary> ���ⵥ���� ���� </summary>
[Serializable]
public class WeaponData : EquipmentData
{
    //���� ��� ����(10)
    StatModifier weaponDamage;//���ݷ�
    StatModifier weaponCriticalChance;//ġ��ŸȮ��
    StatModifier weaponcriticalDamage;//ġ��Ÿ������
    StatModifier weaponAttackSpeed;//���ݼӵ�
    StatModifier weaponAttackRange;//���ݹ���
    StatModifier weaponDefense;//����
    StatModifier weaponType;//����Ÿ��
    StatModifier weaponAttackType;//�������Ÿ��

    public WeaponData(int _ID, WeaponType _weaponType, Grade _equipGrade, string _weaponName, int _weaponDamage, float _attackSpeed, float _attackRange, GameObject _weaponPrefab, Sprite _itemSprite
        , int _defense, int _criticalStrikeChance, int _criticalDamage, int _weaponAttackType)
    {
        //���뽺��(������/���)(7)
        itemID = _ID;
        itemName = _weaponName;
        itemSprite = _itemSprite;
        equipGrade = _equipGrade;
        equipPrefab = _weaponPrefab;
        equipType = EquipType.EquipWeapon;

        //���� ���� ���� ����(7)
        weaponDamage = new StatModifier(_weaponDamage, StatModType.Float, PlayerStatLYK.Instance.Damage.statIndex);
        weaponCriticalChance = new StatModifier(_criticalStrikeChance, StatModType.Float, PlayerStatLYK.Instance.CriticalChance.statIndex);
        weaponcriticalDamage = new StatModifier(_criticalDamage, StatModType.Float, PlayerStatLYK.Instance.CriticalHit.statIndex);
        weaponAttackSpeed = new StatModifier(_attackSpeed, StatModType.Float, PlayerStatLYK.Instance.AttackSpeed.statIndex);
        weaponAttackRange = new StatModifier(_attackRange, StatModType.Float, PlayerStatLYK.Instance.AttackRange.statIndex);
        weaponDefense = new StatModifier(_defense, StatModType.Float, PlayerStatLYK.Instance.Defense.statIndex);
        weaponType = new StatModifier((int)_weaponType, StatModType.Float, PlayerStatLYK.Instance.WeaponType.statIndex);
        weaponAttackType = new StatModifier((int)_weaponAttackType, StatModType.Float, PlayerStatLYK.Instance.WeaponAttackType.statIndex);

        //���� ���� ���� ����Ʈ�� �߰�(7)
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