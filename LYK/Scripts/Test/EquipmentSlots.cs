using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipSlotType { Weapon,Body,Head,Hand,Shoes}

//스크립트 삭제해주기
public class EquipmentSlots : MonoBehaviour
{
    public EquipSlotType equipSlotType;

    public void Use()
    {
        //EquipmentManager.instance.Equip(this);
    }

}
