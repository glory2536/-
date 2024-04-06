using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary> 몸장비 데이터 정보 </summary>
[Serializable]
public class BodyData : EquipmentData
{
    //몸 고유 스탯(4)
    StatModifier bodyDamage;//공격력
    StatModifier bodyHealth;//체력
    StatModifier bodyDefense;//방어력
    StatModifier bodyDodgeChance;//회피율

    public BodyData(int _ID, Grade _equipGrade, string _equipBodyName, int _equipBodyHealth, int _equipBodyDefense, int _equipBodyDodgeChance, int _equipBodyDamage, GameObject _equipBodyPrefab, Sprite _itemSprite)
    {
        //공용스탯(아이템/장비)(6)
        itemID = _ID;
        itemName = _equipBodyName;
        itemSprite = _itemSprite;
        equipGrade = _equipGrade;
        equipPrefab = _equipBodyPrefab;
        equipType = EquipType.EquipBody;

        //몸 고유 스탯 생성(4)
        bodyDamage = new StatModifier(_equipBodyDamage, StatModType.Float, PlayerStatLYK.Instance.Damage.statIndex);//이걸 바꿔줘야함 Damage에서 바로get하게statIndex지워줘야함 
        bodyHealth = new StatModifier(_equipBodyHealth, StatModType.Float, PlayerStatLYK.Instance.Health.statIndex);
        bodyDefense = new StatModifier(_equipBodyDefense, StatModType.Float, PlayerStatLYK.Instance.Defense.statIndex);
        bodyDodgeChance = new StatModifier(_equipBodyDodgeChance, StatModType.Float, PlayerStatLYK.Instance.Evasion.statIndex);


        //몸 고유 스탯 리스트에 추가(4)
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