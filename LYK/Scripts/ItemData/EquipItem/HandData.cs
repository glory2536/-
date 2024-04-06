using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary> ����� ������ ���� </summary>
[Serializable]
public class HandData : EquipmentData
{
    //�� ���� ����(5)
    StatModifier handDamage;//���ݷ�
    StatModifier handHealth;//ü��
    StatModifier handDefense;//����
    StatModifier handDodgeChance;//ȸ����
    StatModifier handAttackSpeed;//���ݼӵ�

    public HandData(int _ID, Grade _equipGrade, string _equipHandName, int _equipHandHealth, int _equipHandDefense, int _equipHandDodgeChance, int _equipHandDamage, int _equipHandAttackSpeed, GameObject _equipHandPrefab, Sprite _itemSprite)
    {
        //���뽺��(������/���)(6)
        itemID = _ID;
        itemName = _equipHandName;
        itemSprite = _itemSprite;
        equipGrade = _equipGrade;
        equipPrefab = _equipHandPrefab;
        equipType = EquipType.EquipHand;

        //�� ���� ���� ����(5)
        handDamage = new StatModifier(_equipHandDamage, StatModType.Float, PlayerStatLYK.Instance.Damage.statIndex);
        handHealth = new StatModifier(_equipHandHealth, StatModType.Float, PlayerStatLYK.Instance.Health.statIndex);
        handDefense = new StatModifier(_equipHandDefense, StatModType.Float, PlayerStatLYK.Instance.Defense.statIndex);
        handDodgeChance = new StatModifier(_equipHandDodgeChance, StatModType.Float, PlayerStatLYK.Instance.Evasion.statIndex);
        handAttackSpeed = new StatModifier(_equipHandAttackSpeed, StatModType.Float, PlayerStatLYK.Instance.AttackSpeed.statIndex);


        //�Ӹ� ���� ���� ����Ʈ�� �߰�(5)
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