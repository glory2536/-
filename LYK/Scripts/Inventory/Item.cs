using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/*
 클래스 구성
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

/// <summary> 수량을 셀 수 있는 아이템(Inventory_Item_CountableItem) </summary>
public class CountableItem : Item
{
    public CountableItemData CountableData { get; private set; }
    public int CurrentAmount { get; protected set; }//현재 아이템 개수
    public int MaxAmount => CountableData.maxAmount;//하나의 슬롯이 가질 수 있는 최대 개수
    public bool IsMax => CurrentAmount >= CountableData.maxAmount;//수량이 가득 찼는지 여부
    public bool IsEmpty => CurrentAmount <= 0;//개수가 없는지 여부

    /// <summary> 개수 지정(범위 제한) </summary>
    public void SetItemAmount(int amount)
    {
        CurrentAmount = Mathf.Clamp(amount, 0, MaxAmount);
    }

    /// <summary> 아이템 개수 추가 및 최대치 초과량 반환(초과량 없을 경우 0반환) </summary>
    public int AddItemAmount(int addAmount)
    {
        int amount = CurrentAmount + addAmount;
        SetItemAmount(amount);

        //초과량있으면 빈 슬롯에 추가해줘야함 처음에 추가해줄때 체크해줘서 자리없으면 UI로표시 처리

        return (amount > MaxAmount) ? (amount - MaxAmount) : 0;//초과량 있으면 반환 없으면 0반환

    }

    public int SubtractItemAmount(int _subtractAmount)
    {
        int amount = CurrentAmount - _subtractAmount;
        SetItemAmount(amount);

        return (amount <= 0) ? (_subtractAmount - CurrentAmount) : 0;
    }

    /// <summary> 소비아이템 아이템사용 </summary> 
    public void UseItemAmount(int UseAmount)
    {
        int amount = CurrentAmount - UseAmount;
        SetItemAmount(amount);
        if (amount < 0) return;
    }

    /*
    //개수를 나누어 복제 => 사용할지 미정
    public CountableItem SeperateAndClone(int amount)
    {
        //수량이 한개 이하일 경우, 복제 불가
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


/// <summary>장비 아이템(Inventory_Item_EquipmentItem) =>개별적데이터 : 강화수치, 초월수치 등 </summary>
public class EquipmentItem : Item
{
    public EquipmentData Equipmentdata { get; private set; }

    //public int currentTranscendence;//현재 아이템 초월 수치
    //public int maxTranscendence;//최대 아이템 초월 수치

    public int currentUpgradeValue;//현재 강화수치
    public int maxUpgradeValue;//최대강화수치


    /*
    //현재 내구도 => 사용할지 미정
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
 
        currentUpgradeValue = _currentUpgradeValue;//강화수치 0로시작 => 추후에 바로밑에서 json으로 받아오기
        maxUpgradeValue = 30;//최대강화수치 30으로 고정
    }

}

/// <summary> 수량 아이템 - 재료아이템 </summary>
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

/// <summary> 수량 아이템 - 포션아이템, 인터페이스 사용 </summary>
public class PotionItem : CountableItem, IUseAbleItem
{
    public PotionItemData PotionData { get; private set; }

    public PotionItem(PotionItemData data, int amount = 1) : base(data, amount)
    {
        PotionData = data;
    }

    /// <summary> 포션아이템 사용 </summary>
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


/// <summary> 장비 - 무기 아이템 </summary>
public class WeaponItem : EquipmentItem
{

    public WeaponData weapondata { get; private set; }

    public WeaponItem(WeaponData data) : base(data)
    {
        weapondata = data;
    }

}

/// <summary> 장비 - 몸 아이템 </summary>
public class BodyItem : EquipmentItem
{
    public BodyItem(BodyData data) : base(data) { }
}

/// <summary> 장비 - 손 아이템 </summary>
public class HandItem : EquipmentItem
{
    public HandItem(HandData data) : base(data) { }
}

/// <summary> 장비 - 신발 아이템 </summary>
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