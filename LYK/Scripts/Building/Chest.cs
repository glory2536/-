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


    //������ ������
    [SerializeField] private Item[] chestItems;//Json���� ����
    //������ ������ UI
    public ItemSlotUI[] itemSlotsUI;

    public Item GetChestItemData(int _index) => chestItems[_index];
    public Item SetChestItemData(int _index, Item _itemData) => chestItems[_index] = _itemData;

    //������ ũ��
    [SerializeField, Range(8, 20)]
    private int capacity = 10;
    public int Capacity { get { return capacity; } private set { capacity = value; } }

    //������ �ִ� ũ��
    [SerializeField, Range(8, 20)]
    private int maxCapacity = 20;
    public int MaxCapacity { get { return maxCapacity; } }

    [SerializeField] private GameObject chestUI;
    public Transform chestSlotParent;//������ ���� �θ�(Left)
    //[SerializeField] private GameObject chestSlot;//������ ���� => �κ��丮�ιٲ��ָ�������?

    [SerializeField] private GameObject itemSlot;//������ ���� ������

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

        //���ͷ��� Ÿ��
        obType = InteractionObType.Building;

        //Json ������
        if (JsonMG.instance.jsonChestItemData.LoadItemDataAll().Length > 0)
        {
            Debug.Log(JsonMG.instance.jsonChestItemData.LoadItemDataAll().Length + "chestItems.Length >0 /=> ������ ������ ����");

            //������ ������ �迭 �ʱ�ȭ
            capacity = JsonMG.instance.jsonChestItemData.capacity;
            chestItems = new Item[MaxCapacity];
            itemSlotsUI = new ItemSlotUI[MaxCapacity];

            for (int i = 0; i < chestItems.Length; i++)
            {
                chestItems[i] = new Item(null);
                itemSlotsUI[i] = Instantiate(itemSlot, chestSlotParent).GetComponent<ItemSlotUI>();
                itemSlotsUI[i].slotType = SlotType.ChestSlot;
            }

            //�����ʱ�ȭ
            InitInventorySlots(Capacity, MaxCapacity);

            //Json ������ ������ ������ �ҷ�����
            chestItems = JsonMG.instance.jsonChestItemData.LoadItemDataAll();

        }
        else
        {
            Debug.Log("chestItems.Length <=0 /=> ������ ó�� �Ǽ�");
            JsonMG.instance.jsonChestItemData.capacity = maxCapacity;

            //������ ������ �迭 �ʱ�ȭ
            chestItems = new Item[MaxCapacity];
            itemSlotsUI = new ItemSlotUI[MaxCapacity];
            for (int i = 0; i < chestItems.Length; i++)//
            {
                chestItems[i] = new Item(null);
                itemSlotsUI[i] = Instantiate(itemSlot, chestSlotParent).GetComponent<ItemSlotUI>();
                itemSlotsUI[i].slotType = SlotType.ChestSlot;
            }

            //�����ʱ�ȭ
            InitInventorySlots(Capacity, MaxCapacity);

            //Json ������ ������ �ʱ�ȭ �� ����
            //JsonMG.instance.jsonChestItemData.capacity = maxCapacity;
            JsonMG.instance.jsonChestItemData.SaveItemDataAll(chestItems);
        }
    }


    /// <summary> �ε����� ���� ���� ���� �ִ��� �˻� </summary>
    public bool IsValidIndex(int index)
    {
        return index >= 0 && index < Capacity;
    }

    /// <summary> ���ͷ��� ��ư�������� ���� </summary>
    public override void InteractionEvent()
    {
        //�κ��丮UI �θ� ���������� �̵�
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
        //�κ��丮UI �θ� �κ��丮�� �̵�
        Inventory.Instance._inventoryUI.InventoryUIParentChange(null);
    }

    /// <summary>������ ���� UI ���� </summary>
    public void UpdateSlotUI(int _index)
    {
        if (!Inventory.Instance.IsValidIndex(_index)) return;

        Item item = chestItems[_index];

        if (item is CountableItem _countableItem)
        {
            itemSlotsUI[_index].SetItemIcon(_countableItem.Data.itemSprite);//������ �̹���
            itemSlotsUI[_index].SetItemAmount(_countableItem.CurrentAmount);//������ ����
        }
        else if (item is EquipmentItem _equipItem)
        {
            itemSlotsUI[_index].SetItemIcon(_equipItem.Data.itemSprite);//1.�������̹���
            itemSlotsUI[_index].SetItemFrame(Inventory.Instance._inventoryUI.ItemGradeFrame((int)_equipItem.Equipmentdata.equipGrade));//2.������ ���(�׵θ�)

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

    /// <summary> �����ʱ�ȭ(��밡���� ����, �ִ뽽��) </summary>
    public void InitInventorySlots(int slotCount, int maxSlotCount)
    {
        Debug.Log(slotCount + "=>SlotCOunt");
        Debug.Log(maxSlotCount + "=>maxSlotCount");

        //������ ����
        for (int i = 0; i < slotCount; i++)//��밡�� ����
        {
            itemSlotsUI[i].InitSlot(i, true);
        }
        for (int k = slotCount; k < maxSlotCount; k++)//���Ұ��� ����
        {

            itemSlotsUI[k].InitSlot(-1, false);
        }

    }
}