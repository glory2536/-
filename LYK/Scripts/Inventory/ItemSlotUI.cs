using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Glory.InventorySystem;

public enum SlotType { InventorItemySlot, EquipItemSlot, ChestSlot }

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    //슬롯 인덱스
    [SerializeField]private int slotIndex;
    public int SlotIndex { get { return slotIndex; } private set { slotIndex = value; } }

    //아이템 접근가능 여부
    private bool isAccessibleSlot;
    public bool IsAccessibleSlot { get { return isAccessibleSlot; } private set { isAccessibleSlot = value; } }
    public SlotType slotType = SlotType.InventorItemySlot;

    [SerializeField] private Image itemIconImage;//아이템 아이콘 이미지 UI
    [SerializeField] private TextMeshProUGUI itemAmmountText;//아이템 개수 Text


    //Drag
    public RectTransform IconRect => _iconRect;
    private RectTransform _iconRect;


    /// <summary> 슬롯 초기화 메서드 </summary>
    public void InitSlot(int index, bool isAccessState)
    {
        SlotIndex = index;
        SetSlotAccessiableState(isAccessState);
        //Drag테스트
        _iconRect = this.GetComponent<RectTransform>();
    }

    /// <summary> 슬롯 활성화/비활성화 상태 처리 메서드 </summary>
    public void SetSlotAccessiableState(bool isAccessState)
    {
        if (isAccessState)//사용가능한 슬롯
        {
            GetComponent<Image>().raycastTarget = true;
            GetComponent<Image>().color = Color.white;
        }
        else//사용불가능 슬롯
        {
            GetComponent<Image>().raycastTarget = false;
            GetComponent<Image>().color = new Color(1, 1, 1, 0.2f);
        }
        IsAccessibleSlot = isAccessState;
        itemIconImage = transform.GetChild(0).GetComponent<Image>();//아이템이미지
        itemAmmountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();//수량,강화수치 텍스트
    }

    /// <summary> 다른슬롯과 아이템 아이콘 교환  </summary>
    public void SwapIcon(ItemSlotUI other)
    {
        if (other == null) return;
        if (other == this) return;
        if (!this.IsAccessibleSlot) return;
        if (!other.IsAccessibleSlot) return;

        if (other.itemIconImage != null)
        {
            //교환 처리
        }
        else
        {
            //없는 경우 이동처리
        }
    }

    /// <summary> 슬롯에서 아이템 UI 제거 </summary>
    public void RemoveItemSlotUI()
    {
        itemIconImage.sprite = null;
        SetItemFrame(Color.white);
        itemIconImage.color = new Color(1, 1, 1, 0);
        HideItemAmountText();
    }

    /// <summary> 아이템 등급에 따른 Frame 처리 </summary>
    public void SetItemFrame(Color itemColor)
    {
        GetComponent<Image>().color = itemColor;
    }

    /// <summary> 아이템 개수 텍스트 설정 </summary>
    public void SetItemAmount(int amount)
    {
        itemAmmountText.gameObject.SetActive(true);
        if (amount < 1)
        {
            itemAmmountText.text = "";
        }
        else
        {
            itemAmmountText.text = amount.ToString();
        }
    }

    /// <summary> 아이템 아이콘 추가 및 변경 </summary>
    public void SetItemIcon(Sprite iconImage)
    {
        itemIconImage.color = new Color(1, 1, 1, 1);
        itemIconImage.sprite = iconImage;
    }

    ///<summary> 아이템 개수 텍스트 꺼주기</summary>
    public void HideItemAmountText()
    {
        itemAmmountText.gameObject.SetActive(false);
    }

    /// <summary> 슬롯 터치 </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Inventory.Instance.ItemToolTip(SlotIndex, slotType);
    }
}