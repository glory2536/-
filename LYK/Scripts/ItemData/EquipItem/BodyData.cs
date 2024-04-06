using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary> ����� ������ ���� </summary>
[Serializable]
public class BodyData : EquipmentData
{
    //�� ���� ����(4)
    StatModifier bodyDamage;//���ݷ�
    StatModifier bodyHealth;//ü��
    StatModifier bodyDefense;//����
    StatModifier bodyDodgeChance;//ȸ����

    public BodyData(int _ID, Grade _equipGrade, string _equipBodyName, int _equipBodyHealth, int _equipBodyDefense, int _equipBodyDodgeChance, int _equipBodyDamage, GameObject _equipBodyPrefab, Sprite _itemSprite)
    {
        //���뽺��(������/���)(6)
        itemID = _ID;
        itemName = _equipBodyName;
        itemSprite = _itemSprite;
        equipGrade = _equipGrade;
        equipPrefab = _equipBodyPrefab;
        equipType = EquipType.EquipBody;

        //�� ���� ���� ����(4)
        bodyDamage = new StatModifier(_equipBodyDamage, StatModType.Float, PlayerStatLYK.Instance.Damage.statIndex);//�̰� �ٲ������ Damage���� �ٷ�get�ϰ�statIndex��������� 
        bodyHealth = new StatModifier(_equipBodyHealth, StatModType.Float, PlayerStatLYK.Instance.Health.statIndex);
        bodyDefense = new StatModifier(_equipBodyDefense, StatModType.Float, PlayerStatLYK.Instance.Defense.statIndex);
        bodyDodgeChance = new StatModifier(_equipBodyDodgeChance, StatModType.Float, PlayerStatLYK.Instance.Evasion.statIndex);


        //�� ���� ���� ����Ʈ�� �߰�(4)
        equipStatList.Add(bodyDamage);
        equipStatList.Add(bodyHealth);
        equipStatList.Add(bodyDefense);
        equipStatList.Add(bodyDodgeChance);
        
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
        return new BodyItem(this);
    }
}