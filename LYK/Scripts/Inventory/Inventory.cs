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
1. 인벤토리
Inventory스크립트=>인벤토리 전체 아이템들을 관리, 인벤토리 내부의 실질적 동작들을 담당

2. 인벤토리 아이템(인벤토리 슬롯에 들어가는 아이템 데이터값)(아이템의 개별적인 데이터 관리)(ex=>내구도,강화수치,수량 등)
Item 상속구조
ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
Item               
  - CountableItem    => 수량을 셀 수 있는 아이템
       - ResourceItem
       - PortionItem

  - EquipmentItem    => 장비 아이템
       - WeaponItem
       - BodyItem
       - HeadItem
       - HandItem
       - ShoesItem
       - ResourceWeaponItem


3. 인벤토리 UI
ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
InventoryUI     => 인벤토리 UI 처리, Inventory와 상호작용
ItemSlotUI      => 인벤토리 내의 각 슬롯 UI


4.아이템 데이터(아이템 데이터베이스)(공통 아이템 데이터 관리)
ItemData 상속구조
ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
ItemData 
   - CountableItemData    => 수량을 셀 수 있는 아이템 데이터
        - ResourcesData
        - PotionItemData
   - EquipmentItemData    => 장비 아이템 데이터
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
        private int capacity = 20;//인벤토리 크기
        public int Capacity { get { return capacity; } private set { capacity = value; } }


        [SerializeField, Range(8, 100)]
        private int maxCapacity = 100;//인벤토리 최대 크기
        public int MaxCapacity { get { return maxCapacity; } }


        public InventoryUI _inventoryUI;//인벤토리 UI관련 이벤트 스크립트

        [SerializeField]
        private Item[] items;//아이템 목록
        public Item GetItemInfo(int itemIndex) => items[itemIndex];

        [SerializeField]
        private Item[] equipItems;//장착된 아이템 목록

        private int currentClickIndex; //현재클릭한 슬롯 인덱스
        [SerializeField] private GameObject[] baseGameObject;//장비 기본 오브젝트


        private void Start()
        {
            //C#에서는 class를 배열로 선언시에 new로 선언하고, 다시 개별 요소마다 new 선언을 해줘야함?(검색해보기) => 안해주면 에러남(null 포인터 예외)
            Invoke("Init", 2);//=>데이터 가져오는시간
        }

        public void Init()
        {
            //착용 장비아이템
            equipItems = new Item[Enum.GetValues(typeof(EquipType)).Length];//0=머리,1=몸,2=신발,4=손,5=무기
            for (int i = 0; i < equipItems.Length; i++)
            {
                equipItems[i] = new Item(null);
            }

            //Json 인벤토리 아이템
            if (JsonMG.instance.jsonInventoryItemData.LoadItemDataAll().Length > 0)
            {
                Debug.Log(JsonMG.instance.jsonInventoryItemData.LoadItemDataAll().Length + "jsonInventoryItemData.Length >0 /=> 보관함 데이터 존재");

                //보관함 아이템 배열 초기화
                Capacity = JsonMG.instance.jsonInventoryItemData.capacity;
                maxCapacity = JsonMG.instance.jsonInventoryItemData.maxCapacity;

                items = new Item[MaxCapacity];

                for (int i = 0; i < items.Length; i++)//
                {
                    items[i] = new Item(null);
                }

                //Json 보관함 아이템 데이터 불러오기
                items = JsonMG.instance.jsonInventoryItemData.LoadItemDataAll();
                for (int i = 0; i < items.Length; i++)
                {
                    _inventoryUI.UpdateSlotUI(i);
                }
            }
            else//처음 접근
            {
                Debug.Log("InventoryItems.Length <=0 /=> 보관함 처음 건설");
                JsonMG.instance.jsonInventoryItemData.capacity = capacity;
                JsonMG.instance.jsonInventoryItemData.maxCapacity = maxCapacity;

                //보관함 아이템 배열 초기화
                items = new Item[MaxCapacity];
                for (int i = 0; i < items.Length; i++)//
                {
                    items[i] = new Item(null);
                }


                //Json 보관함 아이템 초기화 및 저장

                JsonMG.instance.jsonInventoryItemData.capacity = capacity;
                JsonMG.instance.jsonInventoryItemData.maxCapacity = maxCapacity;
                JsonMG.instance.jsonInventoryItemData.SaveItemDataAll(items);
                JsonMG.instance.SaveDataToJson();
            }
        }

        /// <summary> 인덱스가 인벤토리 수용 범위 내에 있는지 검사 </summary>
        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < Capacity;
        }

        /// <summary> 인벤토리에 비어있는 슬롯 있으면 비어있는 index값 반환, 없으면 -1반환 </summary>
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

            Debug.Log("EmptySlot ==> Null / UI표시");//★★★여기서 UI로 표시해주기

            return -1;
        }

        /// <summary> 인벤토리에 CountableItemData 타입 같은 아이템이 존재하고 최대수량아니면 index반환, 없으면 -1반환 </summary>
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

        /// <summary>해당 슬롯이 아이템을 갖고 있는지 여부, 아이템있으면true 없으면 false반환</summary>
        public bool HasItem(int index)
        {
            return IsValidIndex(index) && items[index] != null;
        }

        /// <summary> 해당 슬롯이 셀 수 있는 아이템인지 여부 </summary>
        public bool IsCountableItem(int index)
        {
            return HasItem(index) && items[index] is CountableItem;
        }

        /// <summary> 해당 슬롯의 현재 아이템 개수 리턴 => 잘못된 인덱스 = -1, 빈 슬롯 = 0, 셀 수 없는 아이템 = 1 리턴 </summary>
        public int GetCurrentAmount(int index)
        {
            if (!IsValidIndex(index)) return -1;//잘못된 인덱스
            if (items[index] == null) return 0;//빈 슬롯

            CountableItem ci = items[index] as CountableItem;
            if (ci == null)
                return 1;//셀 수 없는 아이템

            return ci.CurrentAmount;//셀 수 있는 아이템이면 수량 리턴
        }


        /// <summary> 해당 슬롯의 아이템 정보 리턴 </summary>
        public ItemData GetItemData(int _index)
        {
            if (!IsValidIndex(_index)) return null;
            if (items[_index] == null) return null;

            return items[_index].Data;
        }

        /// <summary> 매개변수와 같은 이름의 아이템 개수 리턴 </summary>
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

        /// <summary> 해당 인덱스 슬롯 상태 및 UI 갱신 </summary>
        public void UpdateSlot(int index)
        {
            if (!IsValidIndex(index)) return;

            Item item = items[index];

            if (item != null)
            {
                if (item is CountableItem ci)//수량 있는 아이템
                {
                    if (ci.IsEmpty)
                    {
                        items[index] = new Item(null);
                    }
                    else
                    {

                    }
                }
                else if (item is EquipmentItem equipItem)//장비아이템
                {

                }
                else
                {
                    //Debug.LogError("Error => 무슨 아이템 확인하기");
                }
            }
            else
            {
                Debug.Log(index + " Slot => EmptySlot");
            }


            //테스트용
            JsonMG.instance.jsonInventoryItemData.SaveItemDataAll(items);
            JsonMG.instance.SaveDataToJson();

            _inventoryUI.UpdateSlotUI(index);

        }

        /// <summary> 인벤토리에 아이템 추가 => 넣는데 실패한 남는 아이템 개수 리턴,리턴이 0이면 모든 아이템 추가완료 </summary>
        public int ItemAdd(ItemData itemData, int amount = 1)
        {
            int index;
            Debug.Log(itemData + "=>itemData   " + amount + "=>amount");

            //1. 수량이 있는 아이템
            if (itemData is CountableItemData ciData)
            {
                bool findNextCountable = true;
                index = -1;

                while (amount > 0)
                {
                    if (findNextCountable)
                    {
                        index = FindCountableItemSlotIndex(ciData);//해당 아이템이 인벤토리에 존재하는지 검색

                        if (index == -1)//인벤토리에 해당 아이템없거나 maxCount
                        {
                            findNextCountable = false;//빈 슬롯 탐색 시작
                        }
                        else//기존 슬롯을 찾은 경우, 개수 증가시키고 초과량 존재 시 amount에 초기화
                        {
                            CountableItem ci = items[index] as CountableItem;
                            amount = ci.AddItemAmount(amount);
                            UpdateSlot(index);
                        }
                    }
                    else//1-2. 빈 슬롯 탐색
                    {
                        index = FindEmptySlotIndex();

                        //빈 슬롯이 없으면 종료
                        if (index == -1)
                        {
                            Debug.Log("인벤토리에 자리없음 => UI로 표시해주기");
                            break;
                        }
                        else//빈 슬롯 발견시, 슬롯에 아이템 추가 및 잉여량 계산
                        {
                            CountableItem ci = ciData.CreateItem() as CountableItem;//새로운 아이템 생성

                            ci.SetItemAmount(amount);

                            items[index] = ci;//슬롯에 추가

                            amount = (amount > ciData.maxAmount) ? (amount - ciData.maxAmount) : 0;//남은 개수 계산

                            UpdateSlot(index);

                        }
                    }
                }
            }
            else//2. 수량이 없는 아이템
            {
                //index = -1;
                //2-1. 1개만 넣는 경우
                if (amount == 1)
                {
                    index = FindEmptySlotIndex();
                    Debug.Log(index + "=>Tetsts");
                    if (index != -1)
                    {
                        //아이템을 생성하여 슬롯에 추가
                        items[index] = itemData.CreateItem();

                        amount = 0;

                        UpdateSlot(index);
                    }
                }

                /*
                //2-2. 2개 이상의 수량 없는 아이템을 동시에 추가하는 경우
                index = -1;
                for (; amount > 0; amount--)
                {
                    //아이템 넣은 인덱스의 다음 인덱스부터 슬롯 탐색
                    index = FindEmptySlotIndex();

                    //다 넣지 못한 경우 루프 종료
                    if (index == -1)
                    {
                        break;
                    }

                    //아이템을 생성하여 슬롯에 추가
                    //items[index] = itemData.CreateItem(); => 다시 수정

                    UpdateSlot(index);
                }
                */
            }

            return amount;
        }


        /// <summary> 수량있는 아이템 차감 </summary>
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

        /// <summary> 해당 슬롯의 아이템 제거 </summary>
        public void Remove(int index)
        {
            if (!IsValidIndex(index)) return;

            //items[index] = null;
            items[index] = new Item(null);
            UpdateSlot(index);
        }

        /// <summary> 해당 슬롯의 아이템 사용 </summary>
        public void ItemUse()
        {
            int index = currentClickIndex;
            if (items[index] == null) return;

            //사용 가능한 아이템인 경우
            if (items[index] is IUseAbleItem uItem)
            {
                //아이템사용
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


        /// <summary> 인벤토리 아이템 툴팁 </summary>
        public void ItemToolTip(int index, SlotType _SlotType)
        {
            currentClickIndex = 0;
            currentClickIndex = index;

            //1.인벤토리 슬롯 아이템
            if (_SlotType == SlotType.InventorItemySlot)
            {
                if (!IsValidIndex(index)) return;
                if (items[index] == null) return;

                _inventoryUI.ToolTipUI(index, items[index], _SlotType);
            }
            else if (_SlotType == SlotType.EquipItemSlot)//2.장비인벤토리 슬롯
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

        /// <summary> 장비 아이템 장착 </summary>
        public void ItemEquip()
        {
            if (!IsValidIndex(currentClickIndex)) return;//인덱스가 사용가능한 슬롯 범위인지 판단
            if (items[currentClickIndex] == null) return;//인덱스 슬롯이 빈 슬롯이 아닌지 판단

            Item item = items[currentClickIndex];
            int currentEquipItemIndex;

            //1. 인벤토리 -> 장착장비창(장비장착) 2. 인벤토리 <-> 장착장비창(장비아이템교환) 
            if (item is EquipmentItem clickEquipItem)//장비아이템
            {
                //1. 인벤토리 -> 장착장비창(장비장착)
                if (equipItems[(int)clickEquipItem.Equipmentdata.equipType].Data == null)
                {
                    Debug.Log("t");
                    equipItems[(int)clickEquipItem.Equipmentdata.equipType] = item;
                    items[currentClickIndex] = new Item(null);

                    currentEquipItemIndex = (int)clickEquipItem.Equipmentdata.equipType;

                    ItemChange(clickEquipItem.Equipmentdata.equipType, null, clickEquipItem.Equipmentdata.equipPrefab);

                    //NewItem 스탯 추가
                    for (int i = 0; i < clickEquipItem.Equipmentdata.equipStatList.Count; i++)
                    {
                        PlayerStatLYK.Instance.GetPlayerStat(clickEquipItem.Equipmentdata.equipStatList[i].statIndex).AddModifier(clickEquipItem.Equipmentdata.equipStatList[i]);
                    }

                    //UI처리
                    _inventoryUI.EquipItemUI(currentEquipItemIndex, equipItems[(int)clickEquipItem.Equipmentdata.equipType]);//장착장비 UI 추가                                                                                                                            
                    _inventoryUI.UpdateSlotUI(currentClickIndex);//기존 인벤토리 UI제거

                }
                //2.인벤토리 <-> 장착장비창(장비아이템교환)
                else if (equipItems[(int)clickEquipItem.Equipmentdata.equipType].Data != null)
                {

                    currentEquipItemIndex = (int)clickEquipItem.Equipmentdata.equipType;//현재 장비 슬롯 인덱스

                    Item oldItem = equipItems[currentEquipItemIndex];//OldItem
                    equipItems[currentEquipItemIndex] = item;//NewItem장착 / 인벤토리 -> 장비창

                    items[currentClickIndex] = oldItem;//OldItem해체 / 장비창 -> 인벤토리


                    if (oldItem is EquipmentItem oldEquipItem)
                    {
                        ItemChange(clickEquipItem.Equipmentdata.equipType, oldEquipItem.Equipmentdata.equipPrefab, clickEquipItem.Equipmentdata.equipPrefab);

                        //OldItem스탯 제거
                        for (int i = 0; i < oldEquipItem.Equipmentdata.equipStatList.Count; i++)
                        {
                            PlayerStatLYK.Instance.GetPlayerStat(oldEquipItem.Equipmentdata.equipStatList[i].statIndex).RemoveModifier(oldEquipItem.Equipmentdata.equipStatList[i]);
                        }

                        //NewItem 스탯 추가
                        for (int i = 0; i < clickEquipItem.Equipmentdata.equipStatList.Count; i++)
                        {
                            PlayerStatLYK.Instance.GetPlayerStat(clickEquipItem.Equipmentdata.equipStatList[i].statIndex).AddModifier(clickEquipItem.Equipmentdata.equipStatList[i]);
                        }
                    }

                    //UI처리
                    _inventoryUI.EquipItemUI(currentEquipItemIndex, equipItems[(int)clickEquipItem.Equipmentdata.equipType]);//장착장비 UI
                    _inventoryUI.UpdateSlotUI(currentClickIndex);//인벤토리UI
                }

                if (PlayerStatLYK.Instance.gameObject.TryGetComponent(out Animator playerAnim))
                {
                    playerAnim.SetInteger("AttackType", (int)PlayerStatLYK.Instance.GetPlayerStat(12).GetValue);
                    _inventoryUI.InventoryStatUI();
                }
            }
        }

        /// <summary> 아이템 장비 오브젝트 교환 </summary>
        public void ItemChange(EquipType _equipType, GameObject _oldItem, GameObject _newItem)
        {
            if (_oldItem == null && _newItem != null)//새로운 아이템 장착
            {
                baseGameObject[(int)_equipType].SetActive(false);
                _newItem.SetActive(true);
            }
            else if (_oldItem != null && _newItem != null)//아이템 교환
            {
                _oldItem.SetActive(false);
                _newItem.SetActive(true);
            }
            else if (_oldItem != null && _newItem == null)//장비해제
            {
                _oldItem.SetActive(false);
                baseGameObject[(int)_equipType].SetActive(true);//기본 오브젝트
            }

        }

        /// <summary> 장착 장비 해체 </summary>
        public void UnEquip()
        {
            if (items[currentClickIndex] == null) return;//인덱스 슬롯이 빈 슬롯이 아닌지 판단

            Item item = equipItems[currentClickIndex];

            //1.인벤토리 슬롯에 아이템이 존재하면
            if (item != null)
            {
                if (item is EquipmentItem inventoryEquipItem)//장비아이템
                {
                    if (FindEmptySlotIndex() == -1)
                    {
                        Debug.Log("가방에 자리없다고 처리하고 return");
                    }
                    else
                    {
                        Debug.Log("장비 슬롯 -> 인벤토리");
                        //장비 슬롯 -> 인벤토리 슬롯 , 장비 슬롯 비워주기

                        ItemAdd(equipItems[currentClickIndex].Data);

                        equipItems[currentClickIndex] = new Item(null);
                        ItemChange(inventoryEquipItem.Equipmentdata.equipType, inventoryEquipItem.Equipmentdata.equipPrefab, null);

                        //OldItem스탯 제거
                        for (int i = 0; i < inventoryEquipItem.Equipmentdata.equipStatList.Count; i++)
                        {
                            PlayerStatLYK.Instance.GetPlayerStat(inventoryEquipItem.Equipmentdata.equipStatList[i].statIndex).RemoveModifier(inventoryEquipItem.Equipmentdata.equipStatList[i]);
                        }

                        _inventoryUI.EquipItemUI(currentClickIndex, null);//장착장비 UI제거
                    }
                }
            }

            if (PlayerStatLYK.Instance.gameObject.TryGetComponent(out Animator playerAnim))
            {
                playerAnim.SetInteger("AttackType", (int)PlayerStatLYK.Instance.GetPlayerStat(12).GetValue);
                _inventoryUI.InventoryStatUI();
            }
        }

        /// <summary> 현재 착용한 장비 슬롯 정보 </summary>
        public EquipmentItem EquipItemRetrun(int equipSlotIndex)
        {
            if (equipItems[equipSlotIndex] is EquipmentItem equipitem) return equipitem;

            return null;
        }

        /// <summary> 슬롯 교환 </summary>
        public void SlotDataSwap(ItemSlotUI _indexA, ItemSlotUI _indexB)
        {
            Debug.Log(_indexA);
            Debug.Log(_indexB);

            if (_indexA.slotType == SlotType.ChestSlot && _indexB.slotType == SlotType.ChestSlot)
            {
                Debug.Log("ChestSlot -> ChestSlot");
                if (!Chest.Instance.IsValidIndex(_indexA.SlotIndex)) return;
                if (!Chest.Instance.IsValidIndex(_indexB.SlotIndex)) return;

                //데이터
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
                    //데이터
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
                    //데이터
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

                //데이터
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