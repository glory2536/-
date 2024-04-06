using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary> ���ͷ��� ��� ������ ���� </summary>
[Serializable]
public class ResourceWeaponData : EquipmentData
{
    //���� ��� ����(2)
    StatModifier woodDamage;//���� ���ݷ�
    StatModifier miningDamage;//ä�� ���ݷ�

    public ResourceWeaponData(int _ID, Grade _equipGrade, string _weaponName, int _woodDamage, int _miningDamage, Sprite _itemSprite, GameObject _weaponPrefab)
    {
        //���뽺��(������/���)(7)
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


        //�ڿ����� ���� ���� ����(2)
        woodDamage = new StatModifier(_woodDamage, StatModType.Float, PlayerStatLYK.Instance.LumberingDamage.statIndex);
        miningDamage = new StatModifier(_miningDamage, StatModType.Float, PlayerStatLYK.Instance.MiningDamage.statIndex);

        //�ڿ����� ���� ���� ����Ʈ�� �߰�(2)
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