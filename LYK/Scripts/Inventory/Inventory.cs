using Glory.ObjectPool;
using GooglePlayGames.BasicApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
1. �κ��丮
Inventory��ũ��Ʈ=>�κ��丮 ��ü �����۵��� ����, �κ��丮 ������ ������ ���۵��� ���

2. �κ��丮 ������(�κ��丮 ���Կ� ���� ������ �����Ͱ�)(�������� �������� ������ ����)(ex=>������,��ȭ��ġ,���� ��)
Item ��ӱ���
�ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�
Item               
  - CountableItem    => ������ �� �� �ִ� ������
       - ResourceItem
       - PortionItem

  - EquipmentItem    => ��� ������
       - WeaponItem
       - BodyItem
       - HeadItem
       - HandItem
       - ShoesItem
       - ResourceWeaponItem


3. �κ��丮 UI
�ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�
InventoryUI     => �κ��丮 UI ó��, Inventory�� ��ȣ�ۿ�
ItemSlotUI      => �κ��丮 ���� �� ���� UI


4.������ ������(������ �����ͺ��̽�)(���� ������ ������ ����)
ItemData ��ӱ���
�ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�
ItemData 
   - CountableItemData    => ������ �� �� �ִ� ������ ������
        - ResourcesData
        - PotionItemData
   - EquipmentItemData    => ��� ������ ������
        - WeaponData
        - BodyData
        - HeaData
        - HandData
        - ShoesData
        - ResourceWeaponData

 */
namespace Glory.InventorySystem
{
    //[ExecuteInEditMode]
    public class Inventory : MonoBehaviour
    {
        #region Singleton
        private static Inventory instance;
        public static Inventory Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = FindObjectOfType<Inventory>();
                    if (obj != null)
                    {
                        instance = obj;
                    }
                    else
                    {
                        var newObj = new GameObject().AddComponent<Inventory>();
                        instance = newObj;
                    }
                }
                return instance;
            }
        }
        #endregion


        [SerializeField, Range(8, 100)]
        private int capacity = 20;//�κ��丮 ũ��
        public int Capacity { get { return capacity; } private set { capacity = value; } }


        [SerializeField, Range(8, 100)]
        private int maxCapacity = 100;//�κ��丮 �ִ� ũ��
        public int MaxCapacity { get { return maxCapacity; } }


        public InventoryUI _inventoryUI;//�κ��丮 UI���� �̺�Ʈ ��ũ��Ʈ

        [SerializeField]
        private Item[] items;//������ ���
        public Item GetItemInfo(int itemIndex) => items[itemIndex];

        [SerializeField]
        private Item[] equipItems;//������ ������ ���

        private int currentClickIndex; //����Ŭ���� ���� �ε���
        [SerializeField] private GameObject[] baseGameObject;//��� �⺻ ������Ʈ


        private void Start()
        {
            //C#������ class�� �迭�� ����ÿ� new�� �����ϰ�, �ٽ� ���� ��Ҹ��� new ������ �������?(�˻��غ���) => �����ָ� ������(null ������ ����)
            Invoke("Init", 2);//=>������ �������½ð�
        }

        public void Init()
        {
            //���� ��������
            equipItems = new Item[Enum.GetValues(typeof(EquipType)).Length];//0=�Ӹ�,1=��,2=�Ź�,4=��,5=����
            for (int i = 0; i < equipItems.Length; i++)
            {
                equipItems[i] = new Item(null);
            }

            //Json �κ��丮 ������
            if (JsonMG.instance.jsonInventoryItemData.LoadItemDataAll().Length > 0)
            {
                Debug.Log(JsonMG.instance.jsonInventoryItemData.LoadItemDataAll().Length + "jsonInventoryItemData.Length >0 /=> ������ ������ ����");

                //������ ������ �迭 �ʱ�ȭ
                Capacity = JsonMG.instance.jsonInventoryItemData.capacity;
                maxCapacity = JsonMG.instance.jsonInventoryItemData.maxCapacity;

                items = new Item[MaxCapacity];

                for (int i = 0; i < items.Length; i++)//
                {
                    items[i] = new Item(null);
                }

                //Json ������ ������ ������ �ҷ�����
                items = JsonMG.instance.jsonInventoryItemData.LoadItemDataAll();
                for (int i = 0; i < items.Length; i++)
                {
                    _inventoryUI.UpdateSlotUI(i);
                }
            }
            else//ó�� ����
            {
                Debug.Log("InventoryItems.Length <=0 /=> ������ ó�� �Ǽ�");
                JsonMG.instance.jsonInventoryItemData.capacity = capacity;
                JsonMG.instance.jsonInventoryItemData.maxCapacity = maxCapacity;

                //������ ������ �迭 �ʱ�ȭ
                items = new Item[MaxCapacity];
                for (int i = 0; i < items.Length; i++)//
                {
                    items[i] = new Item(null);
                }


                //Json ������ ������ �ʱ�ȭ �� ����

                JsonMG.instance.jsonInventoryItemData.capacity = capacity;
                JsonMG.instance.jsonInventoryItemData.maxCapacity = maxCapacity;
                JsonMG.instance.jsonInventoryItemData.SaveItemDataAll(items);
                JsonMG.instance.SaveDataToJson();
            }
        }

        /// <summary> �ε����� �κ��丮 ���� ���� ���� �ִ��� �˻� </summary>
        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < Capacity;
        }

        /// <summary> �κ��丮�� ����ִ� ���� ������ ����ִ� index�� ��ȯ, ������ -1��ȯ </summary>
        private int FindEmptySlotIndex()
        {

            for (int i = 0; i < capacity; i++)
            {
                if (items[i].Data == null)
                {
                    Debug.Log("FindEmptySlotIndex ==>" + i);
                    return i;
                }
            }

            Debug.Log("EmptySlot ==> Null / UIǥ��");//�ڡڡڿ��⼭ UI�� ǥ�����ֱ�

            return -1;
        }

        /// <summary> �κ��丮�� CountableItemData Ÿ�� ���� �������� �����ϰ� �ִ�����ƴϸ� index��ȯ, ������ -1��ȯ </summary>
        private int FindCountableItemSlotIndex(CountableItemData countableItem)
        {
            for (int i = 0; i < Capacity; i++)
            {
                var currentSlotItem = items[i];

                if (currentSlotItem == null || currentSlotItem.Data == null)
                    continue;

                if (currentSlotItem.Data.itemID == countableItem.itemID && currentSlotItem is CountableItem ci)
                {
                    if (!ci.IsMax)
                    {
                        Debug.Log("FindCountableItemSlotIndex ==>" + i);
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>�ش� ������ �������� ���� �ִ��� ����, ������������true ������ false��ȯ</summary>
        public bool HasItem(int index)
        {
            return IsValidIndex(index) && items[index] != null;
        }

        /// <summary> �ش� ������ �� �� �ִ� ���������� ���� </summary>
        public bool IsCountableItem(int index)
        {
            return HasItem(index) && items[index] is CountableItem;
        }

        /// <summary> �ش� ������ ���� ������ ���� ���� => �߸��� �ε��� = -1, �� ���� = 0, �� �� ���� ������ = 1 ���� </summary>
        public int GetCurrentAmount(int index)
        {
            if (!IsValidIndex(index)) return -1;//�߸��� �ε���
            if (items[index] == null) return 0;//�� ����

            CountableItem ci = items[index] as CountableItem;
            if (ci == null)
                return 1;//�� �� ���� ������

            return ci.CurrentAmount;//�� �� �ִ� �������̸� ���� ����
        }


        /// <summary> �ش� ������ ������ ���� ���� </summary>
        public ItemData GetItemData(int _index)
        {
            if (!IsValidIndex(_index)) return null;
            if (items[_index] == null) return null;

            return items[_index].Data;
        }

        /// <summary> �Ű������� ���� �̸��� ������ ���� ���� </summary>
        public int GetCurrentAmount(string _itemName)
        {
            int itemCount = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] is CountableItem _countableItem)
                {
                    if (_countableItem.Data.itemName.Equals(_itemName))
                    {
                        itemCount += _countableItem.CurrentAmount;
                    }
                }
            }

            return itemCount;
        }

        /// <summary> �ش� �ε��� ���� ���� �� UI ���� </summary>
        public void UpdateSlot(int index)
        {
            if (!IsValidIndex(index)) return;

            Item item = items[index];

            if (item != null)
            {
                if (item is CountableItem ci)//���� �ִ� ������
                {
                    if (ci.IsEmpty)
                    {
                        items[index] = new Item(null);
                    }
                    else
                    {

                    }
                }
                else if (item is EquipmentItem equipItem)//��������
                {

                }
                else
                {
                    //Debug.LogError("Error => ���� ������ Ȯ���ϱ�");
                }
            }
            else
            {
                Debug.Log(index + " Slot => EmptySlot");
            }


            //�׽�Ʈ��
            JsonMG.instance.jsonInventoryItemData.SaveItemDataAll(items);
            JsonMG.instance.SaveDataToJson();

            _inventoryUI.UpdateSlotUI(index);

        }

        /// <summary> �κ��丮�� ������ �߰� => �ִµ� ������ ���� ������ ���� ����,������ 0�̸� ��� ������ �߰��Ϸ� </summary>
        public int ItemAdd(ItemData itemData, int amount = 1)
        {
            int index;
            Debug.Log(itemData + "=>itemData   " + amount + "=>amount");

            //1. ������ �ִ� ������
            if (itemData is CountableItemData ciData)
            {
                bool findNextCountable = true;
                index = -1;

                while (amount > 0)
                {
                    if (findNextCountable)
                    {
                        index = FindCountableItemSlotIndex(ciData);//�ش� �������� �κ��丮�� �����ϴ��� �˻�

                        if (index == -1)//�κ��丮�� �ش� �����۾��ų� maxCount
                        {
                            findNextCountable = false;//�� ���� Ž�� ����
                        }
                        else//���� ������ ã�� ���, ���� ������Ű�� �ʰ��� ���� �� amount�� �ʱ�ȭ
                        {
                            CountableItem ci = items[index] as CountableItem;
                            amount = ci.AddItemAmount(amount);
                            UpdateSlot(index);
                        }
                    }
                    else//1-2. �� ���� Ž��
                    {
                        index = FindEmptySlotIndex();

                        //�� ������ ������ ����
                        if (index == -1)
                        {
                            Debug.Log("�κ��丮�� �ڸ����� => UI�� ǥ�����ֱ�");
                            break;
                        }
                        else//�� ���� �߽߰�, ���Կ� ������ �߰� �� �׿��� ���
                        {
                            CountableItem ci = ciData.CreateItem() as CountableItem;//���ο� ������ ����

                            ci.SetItemAmount(amount);

                            items[index] = ci;//���Կ� �߰�

                            amount = (amount > ciData.maxAmount) ? (amount - ciData.maxAmount) : 0;//���� ���� ���

                            UpdateSlot(index);

                        }
                    }
                }
            }
            else//2. ������ ���� ������
            {
                //index = -1;
                //2-1. 1���� �ִ� ���
                if (amount == 1)
                {
                    index = FindEmptySlotIndex();
                    Debug.Log(index + "=>Tetsts");
                    if (index != -1)
                    {
                        //�������� �����Ͽ� ���Կ� �߰�
                        items[index] = itemData.CreateItem();

                        amount = 0;

                        UpdateSlot(index);
                    }
                }

                /*
                //2-2. 2�� �̻��� ���� ���� �������� ���ÿ� �߰��ϴ� ���
                index = -1;
                for (; amount > 0; amount--)
                {
                    //������ ���� �ε����� ���� �ε������� ���� Ž��
                    index = FindEmptySlotIndex();

                    //�� ���� ���� ��� ���� ����
                    if (index == -1)
                    {
                        break;
                    }

                    //�������� �����Ͽ� ���Կ� �߰�
                    //items[index] = itemData.CreateItem(); => �ٽ� ����

                    UpdateSlot(index);
                }
                */
            }

            return amount;
        }


        /// <summary> �����ִ� ������ ���� </summary>
        public void CountAbleItemSubtract(String _itemName, int _subtractAmount)
        {
            //List<CountableItem> countableItemList = new();
            Dictionary<int, CountableItem> countableItemList = new();
            int addAmount = 0;

            for (int i = 0; i < items.Length; i++)
            {
                //if (items[i].Data == null) continue;
                if (items[i] == null) continue;

                if (items[i] is CountableItem _countableItem)
                {
                    if (_countableItem.Data.itemName.Equals(_itemName))
                    {
                        Debug.Log("ItemEquials");
                        addAmount += _countableItem.CurrentAmount;
                        countableItemList.Add(i, _countableItem);
                    }
                }
            }

            if (addAmount >= _subtractAmount)
            {
                Debug.Log("InventoryAmount>subtractAmount");
                foreach (var countItem in countableItemList)
                {
                    _subtractAmount = countItem.Value.SubtractItemAmount(_subtractAmount);
                    UpdateSlot(countItem.Key);

                }
            }
        }

        /// <summary> �ش� ������ ������ ���� </summary>
        public void Remove(int index)
        {
            if (!IsValidIndex(index)) return;

            //items[index] = null;
            items[index] = new Item(null);
            UpdateSlot(index);
        }

        /// <summary> �ش� ������ ������ ��� </summary>
        public void ItemUse()
        {
            int index = currentClickIndex;
            if (items[index] == null) return;

            //��� ������ �������� ���
            if (items[index] is IUseAbleItem uItem)
            {
                //�����ۻ��
                bool succeeded = uItem.Use();

                if (succeeded)
                {
                    if (items[index] is PotionItem potionItem)
                    {
                        potionItem.PotionUseEvent();
                        if (potionItem.CurrentAmount == 0)
                        {
                            Remove(index);
                            _inventoryUI.toolTipUI.SetActive(false);
                            return;
                        }
                        _inventoryUI.ToolTipUI(index, items[index], SlotType.InventorItemySlot);
                    }
                    UpdateSlot(index);
                }
            }
        }


        /// <summary> �κ��丮 ������ ���� </summary>
        public void ItemToolTip(int index, SlotType _SlotType)
        {
            currentClickIndex = 0;
            currentClickIndex = index;

            //1.�κ��丮 ���� ������
            if (_SlotType == SlotType.InventorItemySlot)
            {
                if (!IsValidIndex(index)) return;
                if (items[index] == null) return;

                _inventoryUI.ToolTipUI(index, items[index], _SlotType);
            }
            else if (_SlotType == SlotType.EquipItemSlot)//2.����κ��丮 ����
            {
                if (equipItems[index] == null) return;

                currentClickIndex = index;
                _inventoryUI.ToolTipUI(index, equipItems[index], _SlotType);
            }
            else if (_SlotType == SlotType.ChestSlot)
            {
                if (Chest.Instance.GetChestItemData(index) == null) return;

                _inventoryUI.ToolTipUI(index, Chest.Instance.GetChestItemData(index), SlotType.InventorItemySlot);
            }
        }

        /// <summary> ��� ������ ���� </summary>
        public void ItemEquip()
        {
            if (!IsValidIndex(currentClickIndex)) return;//�ε����� ��밡���� ���� �������� �Ǵ�
            if (items[currentClickIndex] == null) return;//�ε��� ������ �� ������ �ƴ��� �Ǵ�

            Item item = items[currentClickIndex];
            int currentEquipItemIndex;

            //1. �κ��丮 -> �������â(�������) 2. �κ��丮 <-> �������â(�������۱�ȯ) 
            if (item is EquipmentItem clickEquipItem)//��������
            {
                //1. �κ��丮 -> �������â(�������)
                if (equipItems[(int)clickEquipItem.Equipmentdata.equipType].Data == null)
                {
                    Debug.Log("t");
                    equipItems[(int)clickEquipItem.Equipmentdata.equipType] = item;
                    items[currentClickIndex] = new Item(null);

                    currentEquipItemIndex = (int)clickEquipItem.Equipmentdata.equipType;

                    ItemChange(clickEquipItem.Equipmentdata.equipType, null, clickEquipItem.Equipmentdata.equipPrefab);

                    //NewItem ���� �߰�
                    for (int i = 0; i < clickEquipItem.Equipmentdata.equipStatList.Count; i++)
                    {
                        PlayerStatLYK.Instance.GetPlayerStat(clickEquipItem.Equipmentdata.equipStatList[i].statIndex).AddModifier(clickEquipItem.Equipmentdata.equipStatList[i]);
                    }

                    //UIó��
                    _inventoryUI.EquipItemUI(currentEquipItemIndex, equipItems[(int)clickEquipItem.Equipmentdata.equipType]);//������� UI �߰�                                                                                                                            
                    _inventoryUI.UpdateSlotUI(currentClickIndex);//���� �κ��丮 UI����

                }
                //2.�κ��丮 <-> �������â(�������۱�ȯ)
                else if (equipItems[(int)clickEquipItem.Equipmentdata.equipType].Data != null)
                {

                    currentEquipItemIndex = (int)clickEquipItem.Equipmentdata.equipType;//���� ��� ���� �ε���

                    Item oldItem = equipItems[currentEquipItemIndex];//OldItem
                    equipItems[currentEquipItemIndex] = item;//NewItem���� / �κ��丮 -> ���â

                    items[currentClickIndex] = oldItem;//OldItem��ü / ���â -> �κ��丮


                    if (oldItem is EquipmentItem oldEquipItem)
                    {
                        ItemChange(clickEquipItem.Equipmentdata.equipType, oldEquipItem.Equipmentdata.equipPrefab, clickEquipItem.Equipmentdata.equipPrefab);

                        //OldItem���� ����
                        for (int i = 0; i < oldEquipItem.Equipmentdata.equipStatList.Count; i++)
                        {
                            PlayerStatLYK.Instance.GetPlayerStat(oldEquipItem.Equipmentdata.equipStatList[i].statIndex).RemoveModifier(oldEquipItem.Equipmentdata.equipStatList[i]);
                        }

                        //NewItem ���� �߰�
                        for (int i = 0; i < clickEquipItem.Equipmentdata.equipStatList.Count; i++)
                        {
                            PlayerStatLYK.Instance.GetPlayerStat(clickEquipItem.Equipmentdata.equipStatList[i].statIndex).AddModifier(clickEquipItem.Equipmentdata.equipStatList[i]);
                        }
                    }

                    //UIó��
                    _inventoryUI.EquipItemUI(currentEquipItemIndex, equipItems[(int)clickEquipItem.Equipmentdata.equipType]);//������� UI
                    _inventoryUI.UpdateSlotUI(currentClickIndex);//�κ��丮UI
                }

                if (PlayerStatLYK.Instance.gameObject.TryGetComponent(out Animator playerAnim))
                {
                    playerAnim.SetInteger("AttackType", (int)PlayerStatLYK.Instance.GetPlayerStat(12).GetValue);
                    _inventoryUI.InventoryStatUI();
                }
            }
        }

        /// <summary> ������ ��� ������Ʈ ��ȯ </summary>
        public void ItemChange(EquipType _equipType, GameObject _oldItem, GameObject _newItem)
        {
            if (_oldItem == null && _newItem != null)//���ο� ������ ����
            {
                baseGameObject[(int)_equipType].SetActive(false);
                _newItem.SetActive(true);
            }
            else if (_oldItem != null && _newItem != null)//������ ��ȯ
            {
                _oldItem.SetActive(false);
                _newItem.SetActive(true);
            }
            else if (_oldItem != null && _newItem == null)//�������
            {
                _oldItem.SetActive(false);
                baseGameObject[(int)_equipType].SetActive(true);//�⺻ ������Ʈ
            }

        }

        /// <summary> ���� ��� ��ü </summary>
        public void UnEquip()
        {
            if (items[currentClickIndex] == null) return;//�ε��� ������ �� ������ �ƴ��� �Ǵ�

            Item item = equipItems[currentClickIndex];

            //1.�κ��丮 ���Կ� �������� �����ϸ�
            if (item != null)
            {
                if (item is EquipmentItem inventoryEquipItem)//��������
                {
                    if (FindEmptySlotIndex() == -1)
                    {
                        Debug.Log("���濡 �ڸ����ٰ� ó���ϰ� return");
                    }
                    else
                    {
                        Debug.Log("��� ���� -> �κ��丮");
                        //��� ���� -> �κ��丮 ���� , ��� ���� ����ֱ�

                        ItemAdd(equipItems[currentClickIndex].Data);

                        equipItems[currentClickIndex] = new Item(null);
                        ItemChange(inventoryEquipItem.Equipmentdata.equipType, inventoryEquipItem.Equipmentdata.equipPrefab, null);

                        //OldItem���� ����
                        for (int i = 0; i < inventoryEquipItem.Equipmentdata.equipStatList.Count; i++)
                        {
                            PlayerStatLYK.Instance.GetPlayerStat(inventoryEquipItem.Equipmentdata.equipStatList[i].statIndex).RemoveModifier(inventoryEquipItem.Equipmentdata.equipStatList[i]);
                        }

                        _inventoryUI.EquipItemUI(currentClickIndex, null);//������� UI����
                    }
                }
            }

            if (PlayerStatLYK.Instance.gameObject.TryGetComponent(out Animator playerAnim))
            {
                playerAnim.SetInteger("AttackType", (int)PlayerStatLYK.Instance.GetPlayerStat(12).GetValue);
                _inventoryUI.InventoryStatUI();
            }
        }

        /// <summary> ���� ������ ��� ���� ���� </summary>
        public EquipmentItem EquipItemRetrun(int equipSlotIndex)
        {
            if (equipItems[equipSlotIndex] is EquipmentItem equipitem) return equipitem;

            return null;
        }

        /// <summary> ���� ��ȯ </summary>
        public void SlotDataSwap(ItemSlotUI _indexA, ItemSlotUI _indexB)
        {
            Debug.Log(_indexA);
            Debug.Log(_indexB);

            if (_indexA.slotType == SlotType.ChestSlot && _indexB.slotType == SlotType.ChestSlot)
            {
                Debug.Log("ChestSlot -> ChestSlot");
                if (!Chest.Instance.IsValidIndex(_indexA.SlotIndex)) return;
                if (!Chest.Instance.IsValidIndex(_indexB.SlotIndex)) return;

                //������
                Item itemA = Chest.Instance.GetChestItemData(_indexA.SlotIndex);
                Item itemB = Chest.Instance.GetChestItemData(_indexB.SlotIndex);
                Chest.Instance.SetChestItemData(_indexA.SlotIndex, itemB);
                Chest.Instance.SetChestItemData(_indexB.SlotIndex, itemA);

                //UI
                Chest.Instance.UpdateSlotUI(_indexA.SlotIndex);
                Chest.Instance.UpdateSlotUI(_indexB.SlotIndex);

            }
            else if (_indexA.slotType == SlotType.ChestSlot || _indexB.slotType == SlotType.ChestSlot)
            {
                Debug.Log("Inventory -> ChestSlot or ChestSlot -> Inventory");
                if (_indexA.slotType == SlotType.ChestSlot)
                {
                    Debug.Log("ChestSlot ->Inventory ");
                    //������
                    Item itemA = Chest.Instance.GetChestItemData(_indexA.SlotIndex);
                    Item itemB = items[_indexB.SlotIndex];
                    Chest.Instance.SetChestItemData(_indexA.SlotIndex, itemB);
                    items[_indexB.SlotIndex] = itemA;

                    //UI
                    Chest.Instance.UpdateSlotUI(_indexA.SlotIndex);
                    UpdateSlot(_indexB.SlotIndex);
                }
                else if (_indexB.slotType == SlotType.ChestSlot)
                {
                    Debug.Log("Inventory -> ChestSlot");
                    //������
                    Item itemA = items[_indexA.SlotIndex];
                    Item itemB = Chest.Instance.GetChestItemData(_indexB.SlotIndex);
                    items[_indexA.SlotIndex] = itemB;
                    Chest.Instance.SetChestItemData(_indexB.SlotIndex, itemA);

                    //UI
                    UpdateSlot(_indexA.SlotIndex);
                    Chest.Instance.UpdateSlotUI(_indexB.SlotIndex);
                }
            }
            else
            {
                Debug.Log("Inventory -> Inventory");
                if (!IsValidIndex(_indexA.SlotIndex)) return;
                if (!IsValidIndex(_indexB.SlotIndex)) return;

                //������
                Item itemA = items[_indexA.SlotIndex];
                Item itemB = items[_indexB.SlotIndex];
                items[_indexA.SlotIndex] = itemB;
                items[_indexB.SlotIndex] = itemA;

                //UI
                UpdateSlot(_indexA.SlotIndex);
                UpdateSlot(_indexB.SlotIndex);
            }
        }

    }
}