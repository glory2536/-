using Glory.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class EquipItemSlotU : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]private int slotIndex;//슬롯 인덱스
    public int SlotIndex { get { return slotIndex; }  set { slotIndex = value; } }
    [SerializeField] EquipType slotEquipType;//슬롯 장착 장비타입
    private SlotType slotType = SlotType.EquipItemSlot;//슬롯 타입

    [SerializeField] private Image equpItemBackgroundImage;//장착아이템 아이콘 이미지 UI
    [SerializeField] private Image equpItemIconImage;//장착아이템 아이콘 이미지 UI
    [SerializeField] private TextMeshProUGUI equipItemAmmountText;//장착 아이템 개수 Text


    private void Awake()
    {
        InitSlot();
    }

    /// <summary> 슬롯 초기화 메서드 </summary>
    public void InitSlot()
    {
        equpItemBackgroundImage = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        equpItemIconImage = transform.GetChild(1).GetChild(1).GetComponent<Image>();
        equipItemAmmountText = transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>();

        SlotIndex = (int)slotEquipType;//★★★수정해주기 이러면 같은 장비 여러개들어가면 오류날듯
    }

    public void EquipItemSlotUI(Sprite _iconImage, Color _itemGradeFrame, int _upgradeValue)
    {
        equpItemBackgroundImage.color = _itemGradeFrame;
        equpItemIconImage.sprite = _iconImage;//아이템이미지
        
        string upGradeValue = (_upgradeValue).ToString() + "/30";//강화수치

    }

    //슬롯 클릭했을때 툴팁 열어주기
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("EquipItemSlotClick");

        Inventory.Instance.ItemToolTip(SlotIndex, slotType);
    }
}
/*
 
    /// <summary> 슬롯 UI 아이템 제거 </summary>
    public void RemoveItem()
    {
        //itemSlotUIParent.gameObject.SetActive(false);
    }

    /// <summary> 아이템 아이콘 추가 및 변경 </summary>
    public void SetItemIcon(Sprite iconImage)
    {
        transform.GetChild(0).GetComponent<Image>().sprite = iconImage;
    }
 */