using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipSlotType { Weapon,Body,Head,Hand,Shoes}

//��ũ��Ʈ �������ֱ�
public class EquipmentSlots : MonoBehaviour
{
    public EquipSlotType equipSlotType;

    public void Use()
    {
        //EquipmentManager.instance.Equip(this);
    }

}
