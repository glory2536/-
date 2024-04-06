using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/*
 Ŭ���� ����
Item.cs     -> EquipmentItem.cs     -> WeaponItem.cs, BodyItem.cs,HeadItem.cs,HandItem.cs,ShoesItem.cs,ResourceWeaponItem
            -> CountableItem.cs     -> ResourceItem.cs, PotionItem.cs
*/
[System.Serializable]
public class Item
{
    [field: SerializeField, SerializeReference]
    public ItemData Data { get; private set; }

    public Item(ItemData data) => Data = data;

}

/// <summary> ������ �� �� �ִ� ������(Inventory_Item_CountableItem) </summary>
public class CountableItem : Item
{
    public CountableItemData CountableData { get; private set; }
    public int CurrentAmount { get; protected set; }//���� ������ ����
    public int MaxAmount => CountableData.maxAmount;//�ϳ��� ������ ���� �� �ִ� �ִ� ����
    public bool IsMax => CurrentAmount >= CountableData.maxAmount;//������ ���� á���� ����
    public bool IsEmpty => CurrentAmount <= 0;//������ ������ ����

    /// <summary> ���� ����(���� ����) </summary>
    public void SetItemAmount(int amount)
    {
        CurrentAmount = Mathf.Clamp(amount, 0, MaxAmount);
    }

    /// <summary> ������ ���� �߰� �� �ִ�ġ �ʰ��� ��ȯ(�ʰ��� ���� ��� 0��ȯ) </summary>
    public int AddItemAmount(int addAmount)
    {
        int amount = CurrentAmount + addAmount;
        SetItemAmount(amount);

        //�ʰ��������� �� ���Կ� �߰�������� ó���� �߰����ٶ� üũ���༭ �ڸ������� UI��ǥ�� ó��

        return (amount > MaxAmount) ? (amount - MaxAmount) : 0;//�ʰ��� ������ ��ȯ ������ 0��ȯ

    }

    public int SubtractItemAmount(int _subtractAmount)
    {
        int amount = CurrentAmount - _subtractAmount;
        SetItemAmount(amount);

        return (amount <= 0) ? (_subtractAmount - CurrentAmount) : 0;
    }

    /// <summary> �Һ������ �����ۻ�� </summary> 
    public void UseItemAmount(int UseAmount)
    {
        int amount = CurrentAmount - UseAmount;
        SetItemAmount(amount);
        if (amount < 0) return;
    }

    /*
    //������ ������ ���� => ������� ����
    public CountableItem SeperateAndClone(int amount)
    {
        //������ �Ѱ� ������ ���, ���� �Ұ�
        if (CurrentAmount <= 1) return null;

        if (amount > CurrentAmount - 1)
            amount = CurrentAmount - 1;

        CurrentAmount -= amount;
        return Clone(amount);
    }
    protected abstract CountableItem Clone(int amount);
    */

    public CountableItem(CountableItemData data, int amount = 1) : base(data)
    {
        CountableData = data;
        SetItemAmount(amount);
    }

}


/// <summary>��� ������(Inventory_Item_EquipmentItem) =>������������ : ��ȭ��ġ, �ʿ���ġ �� </summary>
public class EquipmentItem : Item
{
    public EquipmentData Equipmentdata { get; private set; }

    //public int currentTranscendence;//���� ������ �ʿ� ��ġ
    //public int maxTranscendence;//�ִ� ������ �ʿ� ��ġ

    public int currentUpgradeValue;//���� ��ȭ��ġ
    public int maxUpgradeValue;//�ִ밭ȭ��ġ


    /*
    //���� ������ => ������� ����
    public int Durability
    {
        get => durability;
        set
        {
            if (value < 0) value = 0;
            if(value > Equipmentdata.max)
        }
    }
    private int durability;
    */

    public EquipmentItem(EquipmentData data, int _currentUpgradeValue = 0) : base(data)
    {
        Equipmentdata = data;
 
        currentUpgradeValue = _currentUpgradeValue;//��ȭ��ġ 0�ν��� => ���Ŀ� �ٷιؿ��� json���� �޾ƿ���
        maxUpgradeValue = 30;//�ִ밭ȭ��ġ 30���� ����
    }

}

/// <summary> ���� ������ - �������� </summary>
public class ResourceItem : CountableItem
{
    public ResourceItem(ResourcesData data, int amount = 1) : base(data, amount) { }

    /*
    protected override CountableItem Clone(int amount)
    {
        return new PotionItem(CountableData as PotionItemData, amount);
    }
    */
}

/// <summary> ���� ������ - ���Ǿ�����, �������̽� ��� </summary>
public class PotionItem : CountableItem, IUseAbleItem
{
    public PotionItemData PotionData { get; private set; }

    public PotionItem(PotionItemData data, int amount = 1) : base(data, amount)
    {
        PotionData = data;
    }

    /// <summary> ���Ǿ����� ��� </summary>
    public bool Use()
    {
        CurrentAmount--;
        return true;
    }

    public void PotionUseEvent()
    {
        PlayerStatLYK.Instance.CurrentHealth += PotionData.healValue;
        PlayerStatLYK.Instance.Hungry += PotionData.hungryValue;
        PlayerStatLYK.Instance.Thirsty += PotionData.thirstyValue;
    }

    /*
    protected override CountableItem Clone(int amount)
    {
        return new PotionItem(CountableData as PotionItemData, amount);
    }
    */
}


/// <summary> ��� - ���� ������ </summary>
public class WeaponItem : EquipmentItem
{

    public WeaponData weapondata { get; private set; }

    public WeaponItem(WeaponData data) : base(data)
    {
        weapondata = data;
    }

}

/// <summary> ��� - �� ������ </summary>
public class BodyItem : EquipmentItem
{
    public BodyItem(BodyData data) : base(data) { }
}

/// <summary> ��� - �� ������ </summary>
public class HandItem : EquipmentItem
{
    public HandItem(HandData data) : base(data) { }
}

/// <summary> ��� - �Ź� ������ </summary>
public class ShoesItem : EquipmentItem
{
    public ShoesItem(ShoesData data) : base(data) { }
}

public class HeadItem : EquipmentItem
{
    public HeadItem(HeadData data) : base(data) { }
}

public class ResourceWeaponItem : EquipmentItem
{
    public ResourceWeaponItem(ResourceWeaponData data) : base(data) { }
}