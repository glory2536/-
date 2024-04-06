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
    //���� �ε���
    [SerializeField]private int slotIndex;
    public int SlotIndex { get { return slotIndex; } private set { slotIndex = value; } }

    //������ ���ٰ��� ����
    private bool isAccessibleSlot;
    public bool IsAccessibleSlot { get { return isAccessibleSlot; } private set { isAccessibleSlot = value; } }
    public SlotType slotType = SlotType.InventorItemySlot;

    [SerializeField] private Image itemIconImage;//������ ������ �̹��� UI
    [SerializeField] private TextMeshProUGUI itemAmmountText;//������ ���� Text


    //Drag
    public RectTransform IconRect => _iconRect;
    private RectTransform _iconRect;


    /// <summary> ���� �ʱ�ȭ �޼��� </summary>
    public void InitSlot(int index, bool isAccessState)
    {
        SlotIndex = index;
        SetSlotAccessiableState(isAccessState);
        //Drag�׽�Ʈ
        _iconRect = this.GetComponent<RectTransform>();
    }

    /// <summary> ���� Ȱ��ȭ/��Ȱ��ȭ ���� ó�� �޼��� </summary>
    public void SetSlotAccessiableState(bool isAccessState)
    {
        if (isAccessState)//��밡���� ����
        {
            GetComponent<Image>().raycastTarget = true;
            GetComponent<Image>().color = Color.white;
        }
        else//���Ұ��� ����
        {
            GetComponent<Image>().raycastTarget = false;
            GetComponent<Image>().color = new Color(1, 1, 1, 0.2f);
        }
        IsAccessibleSlot = isAccessState;
        itemIconImage = transform.GetChild(0).GetComponent<Image>();//�������̹���
        itemAmmountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();//����,��ȭ��ġ �ؽ�Ʈ
    }

    /// <summary> �ٸ����԰� ������ ������ ��ȯ  </summary>
    public void SwapIcon(ItemSlotUI other)
    {
        if (other == null) return;
        if (other == this) return;
        if (!this.IsAccessibleSlot) return;
        if (!other.IsAccessibleSlot) return;

        if (other.itemIconImage != null)
        {
            //��ȯ ó��
        }
        else
        {
            //���� ��� �̵�ó��
        }
    }

    /// <summary> ���Կ��� ������ UI ���� </summary>
    public void RemoveItemSlotUI()
    {
        itemIconImage.sprite = null;
        SetItemFrame(Color.white);
        itemIconImage.color = new Color(1, 1, 1, 0);
        HideItemAmountText();
    }

    /// <summary> ������ ��޿� ���� Frame ó�� </summary>
    public void SetItemFrame(Color itemColor)
    {
        GetComponent<Image>().color = itemColor;
    }

    /// <summary> ������ ���� �ؽ�Ʈ ���� </summary>
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

    /// <summary> ������ ������ �߰� �� ���� </summary>
    public void SetItemIcon(Sprite iconImage)
    {
        itemIconImage.color = new Color(1, 1, 1, 1);
        itemIconImage.sprite = iconImage;
    }

    ///<summary> ������ ���� �ؽ�Ʈ ���ֱ�</summary>
    public void HideItemAmountText()
    {
        itemAmmountText.gameObject.SetActive(false);
    }

    /// <summary> ���� ��ġ </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Inventory.Instance.ItemToolTip(SlotIndex, slotType);
    }
}