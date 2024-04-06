using Glory.InventorySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ShopItem
{
    [SerializeReference] public ItemData itemData;//아이템_정보
    public int itemCount;//아이템_개수
    public int weight;//아이템_가중치
    public int buyCost;//아이템_비용

    public ShopItem(ItemData _itemData, int _itemCount, int _weight, int _buyCost)
    {
        itemData = _itemData;
        itemCount = _itemCount;
        weight = _weight;
        buyCost = _buyCost;
    }
}

public class BusShop : InteractionObject
{
    //슬롯
    [SerializeField, Range(8, 20)]
    private int capacity = 8;
    [SerializeField] private GameObject busShopUI;
    [SerializeField] private RectTransform slotsParent;
    [SerializeField] private GameObject shopSlotPrefab;

    //아이템
    [SerializeField] private ShopItem[] shopItems;//상점_전체아이템들
    [SerializeField] private List<ShopItem> itemDict = new();//현재_상점_아이템들
    private double sumOfWeights;//가중치

    //타이머
    Timer timer = new();
    DateTime startTime;//타이머_시작_시간
    [SerializeField] private int maxTime = 7200;//상점_리셋_시간
    [SerializeField] private float timeLeft;//남은시간
    [SerializeField] private TMP_Text timeLeftText;//남은시간_텍스트

    //기타
    bool isBusShop = true;
    Coroutine timerCoroutine = null;

    /// <summary> 전체 아이템의 가중치 합 </summary>
    public double SumOfWeights
    {
        get
        {
            //if (sumOfWeights <= 0) CalculateSumIfDirty();
            CalculateSumIfDirty();

            return sumOfWeights;
        }
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        obType = InteractionObType.Building;
        shopItems = new ShopItem[capacity];

        for (int i = 0; i < shopItems.Length; i++)
        {
            shopItems[i] = new ShopItem(null, 0, 0, 0);
            Instantiate(shopSlotPrefab, slotsParent);//슬롯생성
        }

        AddItemList();//아이템딕셔너리 추가
        CalculateSumIfDirty();//최종 가중치 합

        if (JsonMG.instance.jsonData.busShopItemList.Length > 0)
        {
            for (int i = 0; i < JsonMG.instance.jsonData.busShopItemList.Length; i++)
            {
                shopItems[i] = JsonMG.instance.jsonData.busShopItemList[i];
            }
        }
        else
        {
            //처음접속
            ShopItemsChange();//상점 아이템 교환          
        }
        //ShopItemSlotUI();//상점 아이템 UI

    }



    /// <summary> 인터렉션 이벤트 </summary>
    public override void InteractionEvent()
    {
        //여기서 타이머 확인해주기
        if (busShopUI == null) return;

        ShopItemSlotUI();//상점 아이템 UI
        busShopUI.SetActive(true);
        ShopTimer();

        StartCoroutine(TimerText());

    }

    private void ShopTimer()
    {
        //현재시간 - 마지막저장시간 <최대시간 => 타이머시작
        //현재시간 - 마지막저장시간 >최대시간 => 리셋처리

        DateTime currentTime = DateTime.Now;

        //string dateString = PlayerPrefs.GetString("busShopTimerStartTime");
        string jsonStartTime = JsonMG.instance.jsonData.busShopTimerStartTime;

        if (jsonStartTime != "")
        {
            startTime = DateTime.ParseExact(jsonStartTime, "yyyy-MM-dd HH:mm:ss", null);
        }
        else
        {
            //타이머 처음 시작
            startTime = DateTime.Now;
            string Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //PlayerPrefs.SetString("busShopTimerStartTime", Time);//test
            JsonMG.instance.jsonData.busShopTimerStartTime = Time;
        }


        TimeSpan timeDif = currentTime - startTime;
        if (timeDif.Days == 0)
        {
            if (timeDif.Hours < 2)
            {
                timeLeft = maxTime - (float)timeDif.TotalSeconds;

                if (timerCoroutine != null) StopCoroutine(timerCoroutine);

                timerCoroutine = StartCoroutine(timer.TimerCo(timeLeft, returnValue =>
                {
                    if (returnValue)
                    {
                        TimerMaxEvent();
                    }
                }));
            }
            else
            {
                TimerMaxEvent();
            }
        }
        else//접속 1일초과
        {
            //Debug.Log("BusShop => Day >= 1 => Timer_Max");
            TimerMaxEvent();
        }
    }

    /// <summary> 타이머 최대값 이벤트 </summary>
    public void TimerMaxEvent()
    {
        Debug.Log("BusShop => Timer_Max");
        timeLeft = maxTime;
        string Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        //PlayerPrefs.SetString("busShopTimerStartTime", Time);//test
        JsonMG.instance.jsonData.busShopTimerStartTime = Time;

        ShopItemsChange();//상점_아이템_교환
        ShopItemSlotUI();//상점_아이템 UI
        ShopTimer();//타이머시작

    }

    /// <summary> 남은시간 텍스트 UI(Co) </summary>
    IEnumerator TimerText()
    {
        isBusShop = true;
        while (isBusShop)
        {
            //남은시간 텍스트
            int hour = Mathf.FloorToInt(timer.leftTime / 3600);
            int minutes = Mathf.FloorToInt((timer.leftTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(timer.leftTime % 60);
            timeLeftText.text = string.Format("{0:00} : {1:00} : {2:00}", hour, minutes, seconds);

            yield return null;
        }
    }

    public void ShopUIExitButtonEvent()
    {
        isBusShop = false;
        busShopUI.SetActive(false);
    }

    /// <summary> 상점_슬롯_UI </summary>
    private void ShopItemSlotUI()
    {
        for (int i = 0; i < capacity; i++)
        {
            /*
            if (shopItems[i].itemData == null) return;
            Transform itemImagePosition = slotsParent.GetChild(i).GetChild(0);//아이템이미지UI
            Transform itemNamePosition = slotsParent.GetChild(i).GetChild(1).GetChild(0);//아이템이름UI
            Transform itemCostPosition = slotsParent.GetChild(i).GetChild(2).GetChild(0);//코스트비용UI

            itemImagePosition.GetComponent<Image>().sprite = shopItems[i].itemData.itemSprite;
            itemNamePosition.GetComponent<TMP_Text>().text = $"{shopItems[i].itemData.itemName}x{shopItems[i].itemCount}";
            if (shopItems[i].itemData is CountableItemData countAble)
            {
                itemNamePosition.GetComponent<TMP_Text>().color = Inventory.Instance._inventoryUI.ItemGradeFrame((int)countAble.itemGrade);
            }
            itemCostPosition.GetComponent<TMP_Text>().text = (shopItems[i].buyCost).ToString();
            */

            if (shopItems[i].itemData == null) return;
            Image itemImageImage = slotsParent.GetChild(i).GetChild(0).GetComponent<Image>();//아이템이미지UI
            TMP_Text itemNameText = slotsParent.GetChild(i).GetChild(1).GetChild(0).GetComponent<TMP_Text>();//아이템이름UI
            TMP_Text itemCostText = slotsParent.GetChild(i).GetChild(2).GetChild(0).GetComponent<TMP_Text>();//코스트비용UI

            itemImageImage.sprite = shopItems[i].itemData.itemSprite;
            itemNameText.text = $"{shopItems[i].itemData.itemName}x{shopItems[i].itemCount}";
            if (shopItems[i].itemData is CountableItemData countAble)
            {
                itemNameText.color = Inventory.Instance._inventoryUI.ItemGradeFrame((int)countAble.itemGrade);
            }
            itemCostText.text = (shopItems[i].buyCost).ToString();

        }
    }

    /// <summary> 상점 아이템 교환 </summary>
    private void ShopItemsChange()
    {
        for (int i = 0; i < shopItems.Length; i++)
        {
            shopItems[i] = GetRandomItem();
        }

        //Json에 아이템목록 저장
        JsonMG.instance.jsonData.busShopItemList = shopItems;
    }

    /// <summary> 랜덤(+가중치) 아이템 획득 </summary>
    private ShopItem GetRandomItem()
    {
        double chance = UnityEngine.Random.Range(0.0f, 1.0f);
        chance *= sumOfWeights;

        return GetRandomItem(chance);
    }


    private ShopItem GetRandomItem(double randomValue)
    {
        //Debug.Log(randomValue + " => randomValue");
        double current = 0.0f;
        foreach (var pair in itemDict)
        {
            current += pair.weight;

            if (randomValue < current)
            {
                //Debug.Log(pair.itemData + "=>pair.ItemData");
                return pair;
            }
        }

        throw new Exception($"Unreachable - [Random Value : {randomValue}, Current Value : {current}]");

    }

    /// <summary> 모든 아이템의 가중치 합 계산해놓기 </summary>
    private void CalculateSumIfDirty()
    {
        sumOfWeights = 0.0;
        foreach (var pair in itemDict)
        {
            sumOfWeights += pair.weight;
        }
    }

    /// <summary> 상점 전체 아이템추가 </summary>
    private void AddItemList()
    {
        //나중에 아이템 리셋할때 연출추가해서 로딩시간 대기해주기
        //C등급
        int weightGradC = 40;
        int itemCostGradC = 100;
        //재료아이템(5)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("나무"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("돌"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("고철"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("식물섬유"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("밧줄"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        //소비아이템(5)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("열매"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("버섯"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("날고기"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("당근"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("양배추"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));

        //B등급
        int weightGradB = 40;
        int itemCostGradB = 200;
        //재료(4)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("목재"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("석탄"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("철"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("천조각"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        //소비아이템(4)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("붕대"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("통조림"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("연어"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("열매차"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));

        //A등급
        int weightGradA = 20;
        int itemCostGradA = 400;
        //재료(3)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("나무판자"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("금"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("은괴"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
        //소비아이템(3)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("구급상자"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("장어"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("랍스타"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
    }
}