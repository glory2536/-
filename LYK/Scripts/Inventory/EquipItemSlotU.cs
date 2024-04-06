using Glory.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class EquipItemSlotU : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]private int slotIndex;//���� �ε���
    public int SlotIndex { get { return slotIndex; }  set { slotIndex = value; } }
    [SerializeField] EquipType slotEquipType;//���� ���� ���Ÿ��
    private SlotType slotType = SlotType.EquipItemSlot;//���� Ÿ��

    [SerializeField] private Image equpItemBackgroundImage;//���������� ������ �̹��� UI
    [SerializeField] private Image equpItemIconImage;//���������� ������ �̹��� UI
    [SerializeField] private TextMeshProUGUI equipItemAmmountText;//���� ������ ���� Text


    private void Awake()
    {
        InitSlot();
    }

    /// <summary> ���� �ʱ�ȭ �޼��� </summary>
    public void InitSlot()
    {
        equpItemBackgroundImage = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        equpItemIconImage = transform.GetChild(1).GetChild(1).GetComponent<Image>();
        equipItemAmmountText = transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>();

        SlotIndex = (int)slotEquipType;//�ڡڡڼ������ֱ� �̷��� ���� ��� ���������� ��������
    }

    public void EquipItemSlotUI(Sprite _iconImage, Color _itemGradeFrame, int _upgradeValue)
    {
        equpItemBackgroundImage.color = _itemGradeFrame;
        equpItemIconImage.sprite = _iconImage;//�������̹���
        
        string upGradeValue = (_upgradeValue).ToString() + "/30";//��ȭ��ġ

    }

    //���� Ŭ�������� ���� �����ֱ�
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("EquipItemSlotClick");

        Inventory.Instance.ItemToolTip(SlotIndex, slotType);
    }
}
/*
 
    /// <summary> ���� UI ������ ���� </summary>
    public void RemoveItem()
    {
        //itemSlotUIParent.gameObject.SetActive(false);
    }

    /// <summary> ������ ������ �߰� �� ���� </summary>
    public void SetItemIcon(Sprite iconImage)
    {
        transform.GetChild(0).GetComponent<Image>().sprite = iconImage;
    }
 */