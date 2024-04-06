using Glory.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Chest : InteractionObject
{

    #region Singleton
    private static Chest instance;
    public static Chest Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<Chest>();
                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    GameObject newObj = new GameObject();
                    newObj.name = typeof(Chest).Name;
                    instance = newObj.AddComponent<Chest>();
                }
            }
            return instance;
        }
    }
    #endregion


    //보관함 아이템
    [SerializeField] private Item[] chestItems;//Json으로 저장
    //보관함 아이템 UI
    public ItemSlotUI[] itemSlotsUI;

    public Item GetChestItemData(int _index) => chestItems[_index];
    public Item SetChestItemData(int _index, Item _itemData) => chestItems[_index] = _itemData;

    //보관함 크기
    [SerializeField, Range(8, 20)]
    private int capacity = 10;
    public int Capacity { get { return capacity; } private set { capacity = value; } }

    //보관함 최대 크기
    [SerializeField, Range(8, 20)]
    private int maxCapacity = 20;
    public int MaxCapacity { get { return maxCapacity; } }

    [SerializeField] private GameObject chestUI;
    public Transform chestSlotParent;//보관함 슬롯 부모(Left)
    //[SerializeField] private GameObject chestSlot;//보관함 슬롯 => 인벤토리로바꿔주면좋을듯?

    [SerializeField] private GameObject itemSlot;//아이템 슬롯 프리팹

    public Transform chestInventoryParent;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        //인터렉션 타입
        obType = InteractionObType.Building;

        //Json 데이터
        if (JsonMG.instance.jsonChestItemData.LoadItemDataAll().Length > 0)
        {
            Debug.Log(JsonMG.instance.jsonChestItemData.LoadItemDataAll().Length + "chestItems.Length >0 /=> 보관함 데이터 존재");

            //보관함 아이템 배열 초기화
            capacity = JsonMG.instance.jsonChestItemData.capacity;
            chestItems = new Item[MaxCapacity];
            itemSlotsUI = new ItemSlotUI[MaxCapacity];

            for (int i = 0; i < chestItems.Length; i++)
            {
                chestItems[i] = new Item(null);
                itemSlotsUI[i] = Instantiate(itemSlot, chestSlotParent).GetComponent<ItemSlotUI>();
                itemSlotsUI[i].slotType = SlotType.ChestSlot;
            }

            //슬롯초기화
            InitInventorySlots(Capacity, MaxCapacity);

            //Json 보관함 아이템 데이터 불러오기
            chestItems = JsonMG.instance.jsonChestItemData.LoadItemDataAll();

        }
        else
        {
            Debug.Log("chestItems.Length <=0 /=> 보관함 처음 건설");
            JsonMG.instance.jsonChestItemData.capacity = maxCapacity;

            //보관함 아이템 배열 초기화
            chestItems = new Item[MaxCapacity];
            itemSlotsUI = new ItemSlotUI[MaxCapacity];
            for (int i = 0; i < chestItems.Length; i++)//
            {
                chestItems[i] = new Item(null);
                itemSlotsUI[i] = Instantiate(itemSlot, chestSlotParent).GetComponent<ItemSlotUI>();
                itemSlotsUI[i].slotType = SlotType.ChestSlot;
            }

            //슬롯초기화
            InitInventorySlots(Capacity, MaxCapacity);

            //Json 보관함 아이템 초기화 및 저장
            //JsonMG.instance.jsonChestItemData.capacity = maxCapacity;
            JsonMG.instance.jsonChestItemData.SaveItemDataAll(chestItems);
        }
    }


    /// <summary> 인덱스가 수용 범위 내에 있는지 검사 </summary>
    public bool IsValidIndex(int index)
    {
        return index >= 0 && index < Capacity;
    }

    /// <summary> 인터렉션 버튼눌렀을때 실행 </summary>
    public override void InteractionEvent()
    {
        //인벤토리UI 부모 보관함으로 이동
        Inventory.Instance._inventoryUI.InventoryUIParentChange(chestInventoryParent);
        chestUI.SetActive(true);
        for (int i = 0; i < chestItems.Length; i++)
        {
            //chestItems[i] = JsonMG.instance.jsonData.chestItems[i];
            UpdateSlotUI(i);
        }
    }

    public void ChestExit()
    {
        chestUI.SetActive(false);
        //인벤토리UI 부모 인벤토리로 이동
        Inventory.Instance._inventoryUI.InventoryUIParentChange(null);
    }

    /// <summary>보관함 슬롯 UI 갱신 </summary>
    public void UpdateSlotUI(int _index)
    {
        if (!Inventory.Instance.IsValidIndex(_index)) return;

        Item item = chestItems[_index];

        if (item is CountableItem _countableItem)
        {
            itemSlotsUI[_index].SetItemIcon(_countableItem.Data.itemSprite);//아이템 이미지
            itemSlotsUI[_index].SetItemAmount(_countableItem.CurrentAmount);//아이템 개수
        }
        else if (item is EquipmentItem _equipItem)
        {
            itemSlotsUI[_index].SetItemIcon(_equipItem.Data.itemSprite);//1.아이템이미지
            itemSlotsUI[_index].SetItemFrame(Inventory.Instance._inventoryUI.ItemGradeFrame((int)_equipItem.Equipmentdata.equipGrade));//2.아이템 등급(테두리)

            if (_equipItem.currentUpgradeValue > 0)
            {
                itemSlotsUI[_index].SetItemAmount(_equipItem.currentUpgradeValue);
            }
        }
        else if (item == null || item.Data == null)
        {

            itemSlotsUI[_index].RemoveItemSlotUI();
        }

        /*
        for (int i = 0; i < chestItems.Length; i++)
        {
            if (chestItems[i] != null)
            {
                Debug.Log(chestItems[i].Data + " => " + i);
            }
        }
        */

        JsonMG.instance.jsonChestItemData.SaveItemDataAll(chestItems);
    }

    /// <summary> 슬롯초기화(사용가능한 슬롯, 최대슬롯) </summary>
    public void InitInventorySlots(int slotCount, int maxSlotCount)
    {
        Debug.Log(slotCount + "=>SlotCOunt");
        Debug.Log(maxSlotCount + "=>maxSlotCount");

        //보관함 슬롯
        for (int i = 0; i < slotCount; i++)//사용가능 슬롯
        {
            itemSlotsUI[i].InitSlot(i, true);
        }
        for (int k = slotCount; k < maxSlotCount; k++)//사용불가능 슬롯
        {

            itemSlotsUI[k].InitSlot(-1, false);
        }

    }
}