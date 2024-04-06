using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary> �Ӹ���� ������ ���� </summary>
[Serializable]
public class HeadData : EquipmentData
{
    //�Ӹ� ���� ����(4)
    StatModifier headDamage;//���ݷ�
    StatModifier headHealth;//ü��
    StatModifier headDefense;//����
    StatModifier headDodgeChance;//ȸ����

    public HeadData(int _ID, Grade _equipGrade, string _equipHeadName, int _equipHeadHealth, int _equipHeadDefense, int _equipHeadDodgeChance, int _equipHeadDamage, GameObject _equipHeadPrefab, Sprite _itemSprite)
    {
        //���뽺��(������/���)(6)
        itemID = _ID;
        itemName = _equipHeadName;
        itemSprite = _itemSprite;
        equipGrade = _equipGrade;
        equipPrefab = _equipHeadPrefab;
        equipType = EquipType.EquipHead;

        //�Ӹ� ���� ���� ����(4)
        headDamage = new StatModifier(_equipHeadDamage, StatModType.Float, PlayerStatLYK.Instance.Damage.statIndex);
        headHealth = new StatModifier(_equipHeadHealth, StatModType.Float, PlayerStatLYK.Instance.Health.statIndex);
        headDefense = new StatModifier(_equipHeadDefense, StatModType.Float, PlayerStatLYK.Instance.Defense.statIndex);
        headDodgeChance = new StatModifier(_equipHeadDodgeChance, StatModType.Float, PlayerStatLYK.Instance.Evasion.statIndex);

        //�Ӹ� ���� ���� ����Ʈ�� �߰�(4)
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