using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary> �Ź���� ������ ���� </summary>
[Serializable]
public class ShoesData : EquipmentData
{
    //�Ź� ���� ����(5)
    StatModifier shoesDamage;//���ݷ�
    StatModifier shoesHealth;//ü��
    StatModifier shoesDefense;//����
    StatModifier shoesDodgeChance;//ȸ����
    StatModifier shoesMoveSpeed;//�̵��ӵ�

    public ShoesData(int _ID, Grade _equipGrade, string _equipShoesName, int _equipShoesHealth, int _equipShoesDefense, int _equipShoesDodgeChance, int _equipShoesDamage, int _equipShoesMoveSpeed, GameObject _equipShoesPrefab, Sprite _itemSprite)
    {
        //���뽺��(������/���)(6)
        itemID = _ID;
        itemName = _equipShoesName;
        itemSprite = _itemSprite;
        equipGrade = _equipGrade;
        equipPrefab = _equipShoesPrefab;
        equipType = EquipType.EquipShoes;

        //�Ź� ���� ���� ����(5)
        shoesDamage = new StatModifier(_equipShoesDamage, StatModType.Float, PlayerStatLYK.Instance.Damage.statIndex);
        shoesHealth = new StatModifier(_equipShoesHealth, StatModType.Float, PlayerStatLYK.Instance.Health.statIndex);
        shoesDefense = new StatModifier(_equipShoesDefense, StatModType.Float, PlayerStatLYK.Instance.Defense.statIndex);
        shoesDodgeChance = new StatModifier(_equipShoesDodgeChance, StatModType.Float, PlayerStatLYK.Instance.Evasion.statIndex);
        shoesMoveSpeed = new StatModifier(_equipShoesMoveSpeed, StatModType.Float, PlayerStatLYK.Instance.Speed.statIndex);

        //�Ź� ���� ���� ����Ʈ�� �߰�(5)
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