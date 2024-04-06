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
/// ������� UI ���� ó��, ����/���� ��ũ��Ʈ ����
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("ItemSlot")]
    public ItemSlotUI[] itemSlots;//�����۽���
    [SerializeField] private Transform itemSlotParent;//������ ���� �θ�
    [SerializeField] private GameObject itemSlot;//������ ���� ������

    [Header("EquipItemSlots")]
    public EquipItemSlotU[] equipItemSlots;//���â�����۽���

    [Header("ToolTipUI")]
    public GameObject toolTipUI;//������ ���� ������Ʈ
    [SerializeField] TMP_Text toolTip_ItemName;//����_������ �̸�
    [SerializeField] Image toolTip_ItemBg;//����_������ ���(����׵θ�)
    [SerializeField] Image toolTip_ItemImage;//����_������ �̹���
    [SerializeField] TMP_Text toolTip_ItemText;//����_������ �ؽ�Ʈ(������ ����, ��ȭ��ġ)
    [SerializeField] TMP_Text toolTip_ItemInfo;//����_������ ����(������ ����, ��� ����)
    [SerializeField] Button toolTip_EquipUseButton;//����_�������� ������ư
    [SerializeField] Button toolTip_UnEquipUseButton;//����_�������� ������ư
    [SerializeField] Button toolTip_countableItemUseButton;//����_�Һ������ ����ư

    //DragInventoryUI
    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerEventData;
    private List<RaycastResult> resultList;

    private ItemSlotUI beginDragSlot; // ���� �巡�׸� ������ ����
    private Transform beginDragIconTransform; // �ش� ������ ������ Ʈ������
    private Vector3 beginDragIconPoint;   // �巡�� ���� �� ������ ��ġ
    private Vector3 beginDragCursorPoint; // �巡�� ���� �� Ŀ���� ��ġ
    private int beginDragSlotSiblingIndex;

    [Header("InventoryStatUI")]
    [SerializeField] private TMP_Text damageStatUI;
    [SerializeField] private TMP_Text healthStatUI;
    [SerializeField] private TMP_Text defenseStatUI;
    [SerializeField] private TMP_Text speedStatUI;
    [SerializeField] private TMP_Text inventoryCapacityUI;

    //�κ��丮UI �θ� ����
    public Transform inventoryUI;
    private Transform inventoryUIOriginParent;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        int maxSlotCount = Inventory.Instance.MaxCapacity;//���� �ִ� ����
        int availableSlotCount = Inventory.Instance.Capacity;//��밡���� ���� ����

        itemSlots = new ItemSlotUI[maxSlotCount];
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i] = Instantiate(itemSlot, itemSlotParent).GetComponent<ItemSlotUI>();
        }

        InitInventorySlots(availableSlotCount, maxSlotCount);

        graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
        pointerEventData = new PointerEventData(EventSystem.current);
        resultList = new List<RaycastResult>();//����ĳ��Ʈ ����� ������ ����Ʈ ����

        inventoryUIOriginParent = inventoryUI.parent;
    }


    /// <summary> ��밡���� �κ��丮 UI �ʱ�ȭ </summary>
    public void InitInventorySlots(int slotCount, int maxSlotCount)
    {
        //�κ��丮 ����
        for (int i = 0; i < slotCount; i++)//��밡�� ����
        {
            itemSlots[i].InitSlot(i, true);
        }
        for (int k = slotCount; k < maxSlotCount; k++)//���Ұ��� ����
        {

            itemSlots[k].InitSlot(-1, false);
        }

    }

    /// <summary> �κ��丮 �ε��� ���� UI ���� </summary>
    public void UpdateSlotUI(int index)
    {
        if (!Inventory.Instance.IsValidIndex(index)) return;

        Item item = Inventory.Instance.GetItemInfo(index);

        if (item is CountableItem countableItem)
        {

            itemSlots[index].SetItemIcon(countableItem.Data.itemSprite);//������ �̹���
            itemSlots[index].SetItemAmount(countableItem.CurrentAmount);//������ ����
        }
        //else if (item is EquipmentItem equipItem && item.Data != null)
        else if (item is EquipmentItem equipItem)
        {
            itemSlots[index].SetItemIcon(equipItem.Data.itemSprite);//1.�������̹���
            itemSlots[index].SetItemFrame(ItemGradeFrame((int)equipItem.Equipmentdata.equipGrade));//2.������ ���(�׵θ�)

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

    /// <summary> ������ ���� UI </summary>
    public void ToolTipUI(int index, Item item, SlotType _SlotType)
    {
        if (item is CountableItem countableItem)//�����ִ� ������
        {
            //1.���� ������Ʈ     2.������ �̸�    3.������ �̹���   4.������ ����    5.������ ����
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
                    test += $"ü�� + {potionItem.PotionData.healValue} \r\n";
                if (potionItem.PotionData.hungryValue != 0)
                    test += $"����� + {potionItem.PotionData.hungryValue} \r\n";
                if (potionItem.PotionData.thirstyValue != 0)
                    test += $"�񸶸� + {potionItem.PotionData.thirstyValue} \r\n";

                toolTip_ItemInfo.text = test;
                toolTip_countableItemUseButton.gameObject.SetActive(true);
            }
            else
            {
                toolTip_countableItemUseButton.gameObject.SetActive(false);
            }
        }
        else if (item is EquipmentItem equipItem)//��� ������
        {
            //���� => �����۵��, �ʿ�   =>�⺻����,  =>�ʿ� ����  =>��ư
            //1.���� ������Ʈ     2.������ �̸�    3.������ �̹���   4.��ȭ��ġ  5.������ ����
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


    /// <summary> ������ ���� UI </summary>
    public void EquipItemUI(int _equipItemIndex, Item equipItem)
    {
        toolTipUI.SetActive(false);

        if (equipItem is EquipmentItem newEquipItem)
        {
            //1.�������̹���, 2.�����۵�� 3. �ʿ���ġ, 4.��ȭ��ġ
            equipItemSlots[_equipItemIndex].EquipItemSlotUI(newEquipItem.Equipmentdata.itemSprite, ItemGradeFrame((int)newEquipItem.Equipmentdata.equipGrade), newEquipItem.currentUpgradeValue);
            equipItemSlots[_equipItemIndex].transform.GetChild(1).gameObject.SetActive(true);
        }
        else if (equipItem == null)
        {
            equipItemSlots[_equipItemIndex].transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    /// <summary>������ ��� ������ ����</summary>
    public Color ItemGradeFrame(int EquipGrade)
    {
        //EquipGrade���� ���� �Ѱ���
        //�Ϲ�-> ���-> ���� -> ����ũ
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
    
    /// <summary> �κ��丮 ���� UI  </summary>
    public void InventoryStatUI()
    {
        damageStatUI.text = (PlayerStatLYK.Instance.Damage.GetValue).ToString();
        healthStatUI.text = (PlayerStatLYK.Instance.Health.GetValue).ToString();
        defenseStatUI.text = (PlayerStatLYK.Instance.Defense.GetValue).ToString();
        speedStatUI.text = (PlayerStatLYK.Instance.Speed.GetValue).ToString();
        inventoryCapacityUI.text = $"{Inventory.Instance.Capacity}/{Inventory.Instance.MaxCapacity}";
    }



    //DragInventoryUI �׽�Ʈ
    private void Update()
    {
        //UI�̺�Ʈ ������ ��ġ�� ���콺 Ŭ�� ��ġ�� ����
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

    /// <summary> ���� ó��Ŭ�� </summary>
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

                //���Ŀ������ֱ�
                if (itemSlotParent.TryGetComponent<GridLayoutGroup>(out GridLayoutGroup _gridLayoutGroup))
                {
                    _gridLayoutGroup.enabled = false;
                }

                if (Chest.Instance !=null &&Chest.Instance.chestSlotParent.TryGetComponent<GridLayoutGroup>(out GridLayoutGroup __gridLayoutGroup))
                {
                    __gridLayoutGroup.enabled = false;
                }

                // ��ġ ���, ���� ���
                beginDragIconTransform = beginDragSlot.IconRect.transform;
                beginDragIconPoint = beginDragIconTransform.position;
                beginDragCursorPoint = Input.mousePosition;

                // �� ���� ���̱�
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

    /// <summary> ���� �巡�� �� </summary>
    private void OnPointerDrag()
    {
        if (beginDragSlot == null) return;
        if (beginDragIconTransform == null) return;
        if (beginDragIconPoint == null) return;

        if (Input.GetMouseButton(0))
        {
            // ��ġ �̵�
            beginDragIconTransform.position =
                beginDragIconPoint + (Input.mousePosition - beginDragCursorPoint);
        }
    }

    /// <summary> Ŭ��(�巡��) ���� </summary>
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
                // ��ġ ����
                beginDragIconTransform.position = beginDragIconPoint;

                // UI ���� ����
                beginDragSlot.transform.SetSiblingIndex(beginDragSlotSiblingIndex);

                // �巡�� �Ϸ� ó��
                EndDrag();

                // ���� ����
                beginDragSlot = null;
                beginDragIconTransform = null;

                //���Ŀ������ֱ�
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

    /// <summary> �巡�� ���� �̺�Ʈ </summary>
    private void EndDrag()
    {
        Debug.Log("EndDrag");

        //���⼭ ���� �����ֱ�
        ItemSlotUI endDragSlot = RaycastAndGetFirstComponent<ItemSlotUI>();
        Debug.Log(endDragSlot);

        if (endDragSlot != null)
        {
            TrySwapItems(beginDragSlot, endDragSlot);
        }

    }

    /// <summary> �� ������ ������ ��ȯ </summary>
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