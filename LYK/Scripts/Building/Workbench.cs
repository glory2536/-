using Glory.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftItem
{
    //생산 아이템 
    public ItemData itemData;

    //소모재료_1
    public ItemData craftResourceItem;
    public int craftResourceItemAmount;

    //소모재료_2
    public ItemData craftResourceItem2;
    public int craftResourceItemAmount2;

    public CraftItem(int _itemDataIndex, int? _craftItem1Index, int _craftItemAmount1, int? _craftItem2Index, int _craftItemAmount2)
    {
        itemData = ItemDataMG.Instance.GetItemData(_itemDataIndex);

        if (_craftItem1Index == null)
        {
            craftResourceItem = null;
            craftResourceItemAmount = 0;
        }
        else
        {
            craftResourceItem = ItemDataMG.Instance.GetItemData((int)_craftItem1Index);
            craftResourceItemAmount = _craftItemAmount1;
        }


        if (_craftItem2Index == null)
        {
            craftResourceItem2 = null;
            craftResourceItemAmount2 = 0;
        }
        else
        {
            craftResourceItem2 = ItemDataMG.Instance.GetItemData((int)_craftItem2Index);
            craftResourceItemAmount2 = _craftItemAmount2;
        }
    }
}

public class Workbench : InteractionObject
{
    public GameObject workbenchUI;
    //생산아이템들
    public List<CraftItem> workbenchItems = new();

    public GameObject craftSlotPrefab;
    public Transform craftSlotParent;
    public Transform craftSlotToolTip;

    int touchSlotIndex;


    private void Start()
    {
        SetWorkbenchItems();
    }

    #region 작업대 아이템 추가
    /// <summary>작업대 아이템 추가 </summary>
    private void SetWorkbenchItems()
    {
        //1_야구방망이
        workbenchItems.Add(new CraftItem(201, 1, 3, null, 0));
        //2_나무판자
        workbenchItems.Add(new CraftItem(3, 1, 3, null, 0));
        //3_철 도끼
        workbenchItems.Add(new CraftItem(701, 7, 2, 1, 1));
        //4_철 곡괭이
        workbenchItems.Add(new CraftItem(705, 7, 2, 1, 1));
        //5_방랑자복장
        workbenchItems.Add(new CraftItem(301, 13, 2, null, 0));
        //6_비니
        workbenchItems.Add(new CraftItem(401, 13, 2, null, 0));
        //7_가죽장갑
        workbenchItems.Add(new CraftItem(501, 14, 3, null, 0));
        //8_가죽신발
        workbenchItems.Add(new CraftItem(602, 14, 3, null, 0));
        //9_철괴
        workbenchItems.Add(new CraftItem(8, 7, 3, null, 0));
        //10_은괴
        workbenchItems.Add(new CraftItem(10, 9, 3, null, 0));
        //11_금괴
        workbenchItems.Add(new CraftItem(12, 11, 3, null, 0));
        //12 기본권총
        workbenchItems.Add(new CraftItem(204, 1, 1, null, 0));
        //13 라이플
        workbenchItems.Add(new CraftItem(207, 1, 1, null, 0));
        //13 화염방사기
        workbenchItems.Add(new CraftItem(222, 1, 1, null, 0));
        //14 티타늄헬멧
        workbenchItems.Add(new CraftItem(410, 13, 1, null, 0));
        //15 티타늄장갑
        workbenchItems.Add(new CraftItem(510, 13, 1, null, 0));
        //16 티타늄신발
        workbenchItems.Add(new CraftItem(610, 13, 1, null, 0));
        //17 티타늄슈트
        workbenchItems.Add(new CraftItem(310, 13, 1, null, 0));

        for (int i = 0; i < workbenchItems.Count; i++)
        {
            GameObject ob = Instantiate(craftSlotPrefab, craftSlotParent);
            int indexNumber = i;
            ob.name = indexNumber.ToString();
            ob.GetComponent<Button>().onClick.AddListener(() => SlotBtClickEvent(indexNumber));
            ob.transform.GetChild(0).GetComponent<Image>().sprite = workbenchItems[indexNumber].itemData.itemSprite;
            ob.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = workbenchItems[indexNumber].itemData.itemName;

            if (workbenchItems[indexNumber].itemData is EquipmentData _equipItem)
            {
                ob.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().color = Inventory.Instance._inventoryUI.ItemGradeFrame((int)_equipItem.equipGrade);

            }
            else if (workbenchItems[indexNumber].itemData is CountableItemData _countableItem)
            {
                ob.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().color = Inventory.Instance._inventoryUI.ItemGradeFrame((int)_countableItem.itemGrade);
            }
        }

        craftSlotToolTip.GetChild(4).GetComponent<Button>().onClick.AddListener(CraftButton);
    }
    #endregion

    /// <summary> 인터렉션 버튼눌렀을때 실행 </summary>
    public override void InteractionEvent()
    {
        workbenchUI.SetActive(true);
    }

    #region 건물 버튼UI 이벤트들
    /// <summary> 건물 슬롯 클릭 이벤트 </summary>
    private void SlotBtClickEvent(int _slotIndex)
    {
        craftSlotToolTip.gameObject.SetActive(true);
        craftSlotToolTip.GetChild(0).GetComponent<Image>().sprite = workbenchItems[_slotIndex].itemData.itemSprite;
        craftSlotToolTip.GetChild(1).GetComponent<TMP_Text>().text = workbenchItems[_slotIndex].itemData.itemName;
        if (workbenchItems[_slotIndex].craftResourceItem != null)
        {
            //재료1
            craftSlotToolTip.GetChild(2).GetChild(0).GetComponent<Image>().sprite = workbenchItems[_slotIndex].craftResourceItem.itemSprite;
            craftSlotToolTip.GetChild(2).GetChild(1).GetComponent<TMP_Text>().text =
                $"{Inventory.Instance.GetCurrentAmount(workbenchItems[_slotIndex].craftResourceItem.itemName)} / {workbenchItems[_slotIndex].craftResourceItemAmount}";
        }
        if (workbenchItems[_slotIndex].craftResourceItem2 != null)
        {
            //재료2
            craftSlotToolTip.GetChild(3).gameObject.SetActive(true);
            craftSlotToolTip.GetChild(3).GetChild(0).GetComponent<Image>().sprite = workbenchItems[_slotIndex].craftResourceItem2.itemSprite;
            craftSlotToolTip.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text =
                $"{Inventory.Instance.GetCurrentAmount(workbenchItems[_slotIndex].craftResourceItem2.itemName)} / {workbenchItems[_slotIndex].craftResourceItemAmount2}";
        }
        else
        {
            craftSlotToolTip.GetChild(3).gameObject.SetActive(false);
        }

        touchSlotIndex = _slotIndex;
    }

    /// <summary> 건물 건설 버튼 이벤트 </summary>
    void CraftButton()
    {
        Debug.Log(touchSlotIndex);
        if (workbenchItems[touchSlotIndex].craftResourceItem == null) return;

        if (Inventory.Instance.GetCurrentAmount(workbenchItems[touchSlotIndex].craftResourceItem.itemName) >= workbenchItems[touchSlotIndex].craftResourceItemAmount)
        {
            Debug.Log(workbenchItems[touchSlotIndex].craftResourceItem.itemName);
            if (workbenchItems[touchSlotIndex].craftResourceItem2 != null)
            {
                if (Inventory.Instance.GetCurrentAmount(workbenchItems[touchSlotIndex].craftResourceItem2.itemName) > workbenchItems[touchSlotIndex].craftResourceItemAmount2)
                {
                    //인벤토리에 먼저 자리있나 확인
                    Inventory.Instance.CountAbleItemSubtract(workbenchItems[touchSlotIndex].craftResourceItem.itemName, workbenchItems[touchSlotIndex].craftResourceItemAmount);
                    Inventory.Instance.CountAbleItemSubtract(workbenchItems[touchSlotIndex].craftResourceItem2.itemName, workbenchItems[touchSlotIndex].craftResourceItemAmount2);
                    craftSlotToolTip.gameObject.SetActive(false);
                    Inventory.Instance.ItemAdd(workbenchItems[touchSlotIndex].itemData);
                    return;
                }
            }
            else
            {
                //인벤토리에 먼저 자리있나 확인
                Inventory.Instance.CountAbleItemSubtract(workbenchItems[touchSlotIndex].craftResourceItem.itemName, workbenchItems[touchSlotIndex].craftResourceItemAmount);
                craftSlotToolTip.gameObject.SetActive(false);
                Inventory.Instance.ItemAdd(workbenchItems[touchSlotIndex].itemData);
                return;
            }
        }

        Debug.Log("false => 재료부족하다고 팝업창띄우기");
    }
    #endregion
}
