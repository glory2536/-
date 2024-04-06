using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public EquipmentSlots[] currentEquipment;

    public delegate void OnEquipmentChanged(EquipmentSlots newItem, EquipmentSlots oldItem);
    public OnEquipmentChanged onEquipmentChanged;


    public void Equip(EquipmentSlots newItem)
    {
        int slotIndex = (int)newItem.equipSlotType;

        EquipmentSlots oldItem = null;

        if (currentEquipment[slotIndex] != null)
        {
            oldItem = currentEquipment[slotIndex];
        }

        if(onEquipmentChanged != null)
        {
            onEquipmentChanged.Invoke(newItem, oldItem);
        }

        currentEquipment[slotIndex] = newItem;
    }

    public void Unequip(int slotIndex)
    {
        if (currentEquipment[slotIndex] != null)
        {
            EquipmentSlots oldItem = currentEquipment[slotIndex];

            currentEquipment[slotIndex] = null;

            if(onEquipmentChanged != null)
            {
                onEquipmentChanged.Invoke(null, oldItem);
            }
        }


    }

}