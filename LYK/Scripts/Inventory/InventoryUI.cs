using Glory.InventorySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 사용자의 UI 조작 처리, 슬롯/슬롯 스크립트 관리
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("ItemSlot")]
    public ItemSlotUI[] itemSlots;//아이템슬롯
    [SerializeField] private Transform itemSlotParent;//아이템 슬롯 부모
    [SerializeField] private GameObject itemSlot;//아이템 슬롯 프리팹

    [Header("EquipItemSlots")]
    public EquipItemSlotU[] equipItemSlots;//장비창아이템슬롯

    [Header("ToolTipUI")]
    public GameObject toolTipUI;//아이템 툴팁 오브젝트
    [SerializeField] TMP_Text toolTip_ItemName;//툴팁_아이템 이름
    [SerializeField] Image toolTip_ItemBg;//툴팁_아이템 배경(등급테두리)
    [SerializeField] Image toolTip_ItemImage;//툴팁_아이템 이미지
    [SerializeField] TMP_Text toolTip_ItemText;//툴팁_아이템 텍스트(아이템 수량, 강화수치)
    [SerializeField] TMP_Text toolTip_ItemInfo;//툴팁_아이템 정보(아이템 정보, 장비 스텟)
    [SerializeField] Button toolTip_EquipUseButton;//툴팁_장비아이템 장착버튼
    [SerializeField] Button toolTip_UnEquipUseButton;//툴팁_장비아이템 해제버튼
    [SerializeField] Button toolTip_countableItemUseButton;//툴팁_소비아이템 사용버튼

    //DragInventoryUI
    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerEventData;
    private List<RaycastResult> resultList;

    private ItemSlotUI beginDragSlot; // 현재 드래그를 시작한 슬롯
    private Transform beginDragIconTransform; // 해당 슬롯의 아이콘 트랜스폼
    private Vector3 beginDragIconPoint;   // 드래그 시작 시 슬롯의 위치
    private Vector3 beginDragCursorPoint; // 드래그 시작 시 커서의 위치
    private int beginDragSlotSiblingIndex;

    [Header("InventoryStatUI")]
    [SerializeField] private TMP_Text damageStatUI;
    [SerializeField] private TMP_Text healthStatUI;
    [SerializeField] private TMP_Text defenseStatUI;
    [SerializeField] private TMP_Text speedStatUI;
    [SerializeField] private TMP_Text inventoryCapacityUI;

    //인벤토리UI 부모 변경
    public Transform inventoryUI;
    private Transform inventoryUIOriginParent;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        int maxSlotCount = Inventory.Instance.MaxCapacity;//슬롯 최대 개수
        int availableSlotCount = Inventory.Instance.Capacity;//사용가능한 슬롯 개수

        itemSlots = new ItemSlotUI[maxSlotCount];
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i] = Instantiate(itemSlot, itemSlotParent).GetComponent<ItemSlotUI>();
        }

        InitInventorySlots(availableSlotCount, maxSlotCount);

        graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
        pointerEventData = new PointerEventData(EventSystem.current);
        resultList = new List<RaycastResult>();//레이캐스트 결과를 저장할 리스트 생성

        inventoryUIOriginParent = inventoryUI.parent;
    }


    /// <summary> 사용가능한 인벤토리 UI 초기화 </summary>
    public void InitInventorySlots(int slotCount, int maxSlotCount)
    {
        //인벤토리 슬롯
        for (int i = 0; i < slotCount; i++)//사용가능 슬롯
        {
            itemSlots[i].InitSlot(i, true);
        }
        for (int k = slotCount; k < maxSlotCount; k++)//사용불가능 슬롯
        {

            itemSlots[k].InitSlot(-1, false);
        }

    }

    /// <summary> 인벤토리 인덱스 슬롯 UI 갱신 </summary>
    public void UpdateSlotUI(int index)
    {
        if (!Inventory.Instance.IsValidIndex(index)) return;

        Item item = Inventory.Instance.GetItemInfo(index);

        if (item is CountableItem countableItem)
        {

            itemSlots[index].SetItemIcon(countableItem.Data.itemSprite);//아이템 이미지
            itemSlots[index].SetItemAmount(countableItem.CurrentAmount);//아이템 개수
        }
        //else if (item is EquipmentItem equipItem && item.Data != null)
        else if (item is EquipmentItem equipItem)
        {
            itemSlots[index].SetItemIcon(equipItem.Data.itemSprite);//1.아이템이미지
            itemSlots[index].SetItemFrame(ItemGradeFrame((int)equipItem.Equipmentdata.equipGrade));//2.아이템 등급(테두리)

            if (equipItem.currentUpgradeValue > 0)
            {
                itemSlots[index].SetItemAmount(equipItem.currentUpgradeValue);
            }
            else
            {
                itemSlots[index].SetItemAmount(0);
            }
        }
        else
        {
            itemSlots[index].RemoveItemSlotUI();
        }
    }

    /// <summary> 아이템 툴팁 UI </summary>
    public void ToolTipUI(int index, Item item, SlotType _SlotType)
    {
        if (item is CountableItem countableItem)//수량있는 아이템
        {
            //1.툴팁 오브젝트     2.아이템 이름    3.아이템 이미지   4.아이템 수량    5.아이템 정보
            toolTipUI.gameObject.SetActive(true);
            toolTip_ItemName.text = countableItem.Data.itemName;
            toolTip_ItemImage.sprite = countableItem.Data.itemSprite;
            toolTip_ItemText.text = (countableItem.CurrentAmount).ToString();
            toolTip_ItemInfo.text = countableItem.CountableData.resourcesInfo;

            toolTip_EquipUseButton.gameObject.SetActive(false);
            toolTip_UnEquipUseButton.gameObject.SetActive(false);
            toolTip_ItemBg.GetComponent<Image>().color = ItemGradeFrame((int)countableItem.CountableData.itemGrade);

            if (item is PotionItem potionItem)
            {
                string test = null;
                if (potionItem.PotionData.healValue != 0)
                    test += $"체력 + {potionItem.PotionData.healValue} \r\n";
                if (potionItem.PotionData.hungryValue != 0)
                    test += $"배고픔 + {potionItem.PotionData.hungryValue} \r\n";
                if (potionItem.PotionData.thirstyValue != 0)
                    test += $"목마름 + {potionItem.PotionData.thirstyValue} \r\n";

                toolTip_ItemInfo.text = test;
                toolTip_countableItemUseButton.gameObject.SetActive(true);
            }
            else
            {
                toolTip_countableItemUseButton.gameObject.SetActive(false);
            }
        }
        else if (item is EquipmentItem equipItem)//장비 아이템
        {
            //미정 => 아이템등급, 초월   =>기본스탯,  =>초월 버프  =>버튼
            //1.툴팁 오브젝트     2.아이템 이름    3.아이템 이미지   4.강화수치  5.아이템 스텟
            toolTipUI.gameObject.SetActive(true);
            toolTip_ItemName.text = equipItem.Data.itemName;
            toolTip_ItemImage.sprite = equipItem.Data.itemSprite;
            toolTip_ItemBg.GetComponent<Image>().color = ItemGradeFrame((int)equipItem.Equipmentdata.equipGrade);

            if (equipItem.currentUpgradeValue > 0)
            {
                toolTip_ItemText.text = string.Format("+{0}", equipItem.currentUpgradeValue);
            }
            else
            {
                toolTip_ItemText.text = "";
            }

            toolTip_countableItemUseButton.gameObject.SetActive(false);

            if (_SlotType == SlotType.InventorItemySlot)
            {
                toolTip_EquipUseButton.gameObject.SetActive(true);
                toolTip_UnEquipUseButton.gameObject.SetActive(false);
            }
            else if (_SlotType == SlotType.EquipItemSlot)
            {
                toolTip_EquipUseButton.gameObject.SetActive(false);
                toolTip_UnEquipUseButton.gameObject.SetActive(true);
            }
            toolTip_countableItemUseButton.gameObject.SetActive(false);

            string statString = null;
            for (int i = 0; i < equipItem.Equipmentdata.equipStatList.Count; i++)
            {
                if (equipItem.Equipmentdata.equipStatList[i].statIndex == 12|| equipItem.Equipmentdata.equipStatList[i].statIndex == 21)
                {
                    continue;
                }
                statString += string.Format("{0} = {1} \r\n", PlayerStatLYK.Instance.GetPlayerStat(equipItem.Equipmentdata.equipStatList[i].statIndex).statName, equipItem.Equipmentdata.equipStatList[i].statValue);
            }
            toolTip_ItemInfo.text = statString;
        }
        else if (item == null)
        {
            return;
        }

    }


    /// <summary> 아이템 장착 UI </summary>
    public void EquipItemUI(int _equipItemIndex, Item equipItem)
    {
        toolTipUI.SetActive(false);

        if (equipItem is EquipmentItem newEquipItem)
        {
            //1.아이템이미지, 2.아이템등급 3. 초월수치, 4.강화수치
            equipItemSlots[_equipItemIndex].EquipItemSlotUI(newEquipItem.Equipmentdata.itemSprite, ItemGradeFrame((int)newEquipItem.Equipmentdata.equipGrade), newEquipItem.currentUpgradeValue);
            equipItemSlots[_equipItemIndex].transform.GetChild(1).gameObject.SetActive(true);
        }
        else if (equipItem == null)
        {
            equipItemSlots[_equipItemIndex].transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    /// <summary>아이템 등급 프레임 변경</summary>
    public Color ItemGradeFrame(int EquipGrade)
    {
        //EquipGrade값에 따라 넘겨줌
        //일반-> 고급-> 레어 -> 유니크
        Color itemFrameColor = Color.black;

        if (EquipGrade == 0)
        {
            Debug.Log("EquipGrade => Null");
            itemFrameColor = Color.black;
        }
        else if (EquipGrade == 1)
        {
            Debug.Log("EquipGrade => C");
            itemFrameColor = Color.black;
        }
        else if (EquipGrade == 2)
        {
            Debug.Log("EquipGrade => B");
            itemFrameColor = Color.green;
        }
        else if (EquipGrade == 3)
        {
            Debug.Log("EquipGrade => A");
            itemFrameColor = Color.blue;
        }
        else if (EquipGrade == 4)
        {
            Debug.Log("EquipGrade => S");
            itemFrameColor = new Color(141, 0, 255, 255);
        }

        return itemFrameColor;
    }
    
    /// <summary> 인벤토리 스텟 UI  </summary>
    public void InventoryStatUI()
    {
        damageStatUI.text = (PlayerStatLYK.Instance.Damage.GetValue).ToString();
        healthStatUI.text = (PlayerStatLYK.Instance.Health.GetValue).ToString();
        defenseStatUI.text = (PlayerStatLYK.Instance.Defense.GetValue).ToString();
        speedStatUI.text = (PlayerStatLYK.Instance.Speed.GetValue).ToString();
        inventoryCapacityUI.text = $"{Inventory.Instance.Capacity}/{Inventory.Instance.MaxCapacity}";
    }



    //DragInventoryUI 테스트
    private void Update()
    {
        //UI이벤트 데이터 위치를 마우스 클릭 위치로 설정
        pointerEventData.position = Input.mousePosition;
        OnPointerDown();
        OnPointerDrag();
        OnPointerUp();

    }

    private T RaycastAndGetFirstComponent<T>() where T : Component
    {
        resultList.Clear();

        graphicRaycaster.Raycast(pointerEventData, resultList);
        Debug.Log(resultList.Count);
        if (resultList.Count == 0)
            return null;

        foreach (var result in resultList)
        {
            if (result.gameObject.TryGetComponent<T>(out T t))
            {
                return t;
            }
        }

        return null;
    }

    /// <summary> 슬롯 처음클릭 </summary>
    private void OnPointerDown()
    {
        // Left Click : Begin Drag
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("OnPointerDown");
            beginDragSlot = RaycastAndGetFirstComponent<ItemSlotUI>();
            Debug.Log(beginDragSlot);

            if (beginDragSlot != null)
            {
                if (beginDragSlot.slotType == SlotType.InventorItemySlot)
                {
                    if (Inventory.Instance.GetItemInfo(beginDragSlot.SlotIndex) == null || Inventory.Instance.GetItemInfo(beginDragSlot.SlotIndex).Data == null) return;
                }
                else if (beginDragSlot.slotType == SlotType.ChestSlot)
                {
                    if (Chest.Instance.GetChestItemData(beginDragSlot.SlotIndex).Data == null) return;              
                }

                //추후에지워주기
                if (itemSlotParent.TryGetComponent<GridLayoutGroup>(out GridLayoutGroup _gridLayoutGroup))
                {
                    _gridLayoutGroup.enabled = false;
                }

                if (Chest.Instance !=null &&Chest.Instance.chestSlotParent.TryGetComponent<GridLayoutGroup>(out GridLayoutGroup __gridLayoutGroup))
                {
                    __gridLayoutGroup.enabled = false;
                }

                // 위치 기억, 참조 등록
                beginDragIconTransform = beginDragSlot.IconRect.transform;
                beginDragIconPoint = beginDragIconTransform.position;
                beginDragCursorPoint = Input.mousePosition;

                // 맨 위에 보이기
                beginDragSlotSiblingIndex = beginDragSlot.transform.GetSiblingIndex();
                beginDragSlot.transform.SetAsLastSibling();

                //_beginDragSlot.SetHighlightOnTop(false);
            }
            else
            {
                beginDragSlot = null;
            }
        }
    }

    /// <summary> 슬롯 드래그 중 </summary>
    private void OnPointerDrag()
    {
        if (beginDragSlot == null) return;
        if (beginDragIconTransform == null) return;
        if (beginDragIconPoint == null) return;

        if (Input.GetMouseButton(0))
        {
            // 위치 이동
            beginDragIconTransform.position =
                beginDragIconPoint + (Input.mousePosition - beginDragCursorPoint);
        }
    }

    /// <summary> 클릭(드래그) 종료 </summary>
    private void OnPointerUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("OnPointerUp");
            if (beginDragIconTransform == null) return;
            if (beginDragIconPoint == null) return;

            // End Drag
            if (beginDragSlot != null)
            {
                // 위치 복원
                beginDragIconTransform.position = beginDragIconPoint;

                // UI 순서 복원
                beginDragSlot.transform.SetSiblingIndex(beginDragSlotSiblingIndex);

                // 드래그 완료 처리
                EndDrag();

                // 참조 제거
                beginDragSlot = null;
                beginDragIconTransform = null;

                //추후에지워주기
                if (itemSlotParent.TryGetComponent<GridLayoutGroup>(out GridLayoutGroup _gridLayoutGroup))
                {
                    _gridLayoutGroup.enabled = true;
                }
                if (Chest.Instance != null &&Chest.Instance.chestSlotParent.TryGetComponent<GridLayoutGroup>(out GridLayoutGroup __gridLayoutGroup))
                {
                    __gridLayoutGroup.enabled = true;

                }
            }
        }
    }

    /// <summary> 드래그 종료 이벤트 </summary>
    private void EndDrag()
    {
        Debug.Log("EndDrag");

        //여기서 슬롯 비교해주기
        ItemSlotUI endDragSlot = RaycastAndGetFirstComponent<ItemSlotUI>();
        Debug.Log(endDragSlot);

        if (endDragSlot != null)
        {
            TrySwapItems(beginDragSlot, endDragSlot);
        }

    }

    /// <summary> 두 슬롯의 아이템 교환 </summary>
    private void TrySwapItems(ItemSlotUI from, ItemSlotUI to)
    {
        if (from == to) return;

        Inventory.Instance.SlotDataSwap(from, to);
    }

    private bool IsOverUI() => EventSystem.current.IsPointerOverGameObject();

    public void InventoryUIParentChange(Transform parentTransform)
    {
        if (parentTransform == null)
        {
            inventoryUI.SetParent(inventoryUIOriginParent);
            inventoryUI.SetAsFirstSibling();
            graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
            return;
        }
        inventoryUI.SetParent(parentTransform);
        graphicRaycaster = parentTransform.GetComponentInParent<GraphicRaycaster>();
    }
}