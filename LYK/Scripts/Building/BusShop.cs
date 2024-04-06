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
    [SerializeReference] public ItemData itemData;//������_����
    public int itemCount;//������_����
    public int weight;//������_����ġ
    public int buyCost;//������_���

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
    //����
    [SerializeField, Range(8, 20)]
    private int capacity = 8;
    [SerializeField] private GameObject busShopUI;
    [SerializeField] private RectTransform slotsParent;
    [SerializeField] private GameObject shopSlotPrefab;

    //������
    [SerializeField] private ShopItem[] shopItems;//����_��ü�����۵�
    [SerializeField] private List<ShopItem> itemDict = new();//����_����_�����۵�
    private double sumOfWeights;//����ġ

    //Ÿ�̸�
    Timer timer = new();
    DateTime startTime;//Ÿ�̸�_����_�ð�
    [SerializeField] private int maxTime = 7200;//����_����_�ð�
    [SerializeField] private float timeLeft;//�����ð�
    [SerializeField] private TMP_Text timeLeftText;//�����ð�_�ؽ�Ʈ

    //��Ÿ
    bool isBusShop = true;
    Coroutine timerCoroutine = null;

    /// <summary> ��ü �������� ����ġ �� </summary>
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
            Instantiate(shopSlotPrefab, slotsParent);//���Ի���
        }

        AddItemList();//�����۵�ųʸ� �߰�
        CalculateSumIfDirty();//���� ����ġ ��

        if (JsonMG.instance.jsonData.busShopItemList.Length > 0)
        {
            for (int i = 0; i < JsonMG.instance.jsonData.busShopItemList.Length; i++)
            {
                shopItems[i] = JsonMG.instance.jsonData.busShopItemList[i];
            }
        }
        else
        {
            //ó������
            ShopItemsChange();//���� ������ ��ȯ          
        }
        //ShopItemSlotUI();//���� ������ UI

    }



    /// <summary> ���ͷ��� �̺�Ʈ </summary>
    public override void InteractionEvent()
    {
        //���⼭ Ÿ�̸� Ȯ�����ֱ�
        if (busShopUI == null) return;

        ShopItemSlotUI();//���� ������ UI
        busShopUI.SetActive(true);
        ShopTimer();

        StartCoroutine(TimerText());

    }

    private void ShopTimer()
    {
        //����ð� - ����������ð� <�ִ�ð� => Ÿ�̸ӽ���
        //����ð� - ����������ð� >�ִ�ð� => ����ó��

        DateTime currentTime = DateTime.Now;

        //string dateString = PlayerPrefs.GetString("busShopTimerStartTime");
        string jsonStartTime = JsonMG.instance.jsonData.busShopTimerStartTime;

        if (jsonStartTime != "")
        {
            startTime = DateTime.ParseExact(jsonStartTime, "yyyy-MM-dd HH:mm:ss", null);
        }
        else
        {
            //Ÿ�̸� ó�� ����
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
        else//���� 1���ʰ�
        {
            //Debug.Log("BusShop => Day >= 1 => Timer_Max");
            TimerMaxEvent();
        }
    }

    /// <summary> Ÿ�̸� �ִ밪 �̺�Ʈ </summary>
    public void TimerMaxEvent()
    {
        Debug.Log("BusShop => Timer_Max");
        timeLeft = maxTime;
        string Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        //PlayerPrefs.SetString("busShopTimerStartTime", Time);//test
        JsonMG.instance.jsonData.busShopTimerStartTime = Time;

        ShopItemsChange();//����_������_��ȯ
        ShopItemSlotUI();//����_������ UI
        ShopTimer();//Ÿ�̸ӽ���

    }

    /// <summary> �����ð� �ؽ�Ʈ UI(Co) </summary>
    IEnumerator TimerText()
    {
        isBusShop = true;
        while (isBusShop)
        {
            //�����ð� �ؽ�Ʈ
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

    /// <summary> ����_����_UI </summary>
    private void ShopItemSlotUI()
    {
        for (int i = 0; i < capacity; i++)
        {
            /*
            if (shopItems[i].itemData == null) return;
            Transform itemImagePosition = slotsParent.GetChild(i).GetChild(0);//�������̹���UI
            Transform itemNamePosition = slotsParent.GetChild(i).GetChild(1).GetChild(0);//�������̸�UI
            Transform itemCostPosition = slotsParent.GetChild(i).GetChild(2).GetChild(0);//�ڽ�Ʈ���UI

            itemImagePosition.GetComponent<Image>().sprite = shopItems[i].itemData.itemSprite;
            itemNamePosition.GetComponent<TMP_Text>().text = $"{shopItems[i].itemData.itemName}x{shopItems[i].itemCount}";
            if (shopItems[i].itemData is CountableItemData countAble)
            {
                itemNamePosition.GetComponent<TMP_Text>().color = Inventory.Instance._inventoryUI.ItemGradeFrame((int)countAble.itemGrade);
            }
            itemCostPosition.GetComponent<TMP_Text>().text = (shopItems[i].buyCost).ToString();
            */

            if (shopItems[i].itemData == null) return;
            Image itemImageImage = slotsParent.GetChild(i).GetChild(0).GetComponent<Image>();//�������̹���UI
            TMP_Text itemNameText = slotsParent.GetChild(i).GetChild(1).GetChild(0).GetComponent<TMP_Text>();//�������̸�UI
            TMP_Text itemCostText = slotsParent.GetChild(i).GetChild(2).GetChild(0).GetComponent<TMP_Text>();//�ڽ�Ʈ���UI

            itemImageImage.sprite = shopItems[i].itemData.itemSprite;
            itemNameText.text = $"{shopItems[i].itemData.itemName}x{shopItems[i].itemCount}";
            if (shopItems[i].itemData is CountableItemData countAble)
            {
                itemNameText.color = Inventory.Instance._inventoryUI.ItemGradeFrame((int)countAble.itemGrade);
            }
            itemCostText.text = (shopItems[i].buyCost).ToString();

        }
    }

    /// <summary> ���� ������ ��ȯ </summary>
    private void ShopItemsChange()
    {
        for (int i = 0; i < shopItems.Length; i++)
        {
            shopItems[i] = GetRandomItem();
        }

        //Json�� �����۸�� ����
        JsonMG.instance.jsonData.busShopItemList = shopItems;
    }

    /// <summary> ����(+����ġ) ������ ȹ�� </summary>
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

    /// <summary> ��� �������� ����ġ �� ����س��� </summary>
    private void CalculateSumIfDirty()
    {
        sumOfWeights = 0.0;
        foreach (var pair in itemDict)
        {
            sumOfWeights += pair.weight;
        }
    }

    /// <summary> ���� ��ü �������߰� </summary>
    private void AddItemList()
    {
        //���߿� ������ �����Ҷ� �����߰��ؼ� �ε��ð� ������ֱ�
        //C���
        int weightGradC = 40;
        int itemCostGradC = 100;
        //��������(5)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("����"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("��"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("��ö"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("�Ĺ�����"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("����"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        //�Һ������(5)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("����"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("����"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("�����"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("���"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("�����"), UnityEngine.Random.Range(1, 3), weightGradC, itemCostGradC));

        //B���
        int weightGradB = 40;
        int itemCostGradB = 200;
        //���(4)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("����"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("��ź"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("ö"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("õ����"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        //�Һ������(4)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("�ش�"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("������"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("����"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("������"), UnityEngine.Random.Range(1, 3), weightGradB, itemCostGradB));

        //A���
        int weightGradA = 20;
        int itemCostGradA = 400;
        //���(3)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("��������"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("��"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("����"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
        //�Һ������(3)
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("���޻���"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("���"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
        itemDict.Add(new ShopItem(ItemDataMG.Instance.GetItemData("����Ÿ"), UnityEngine.Random.Range(1, 3), weightGradA, itemCostGradA));
    }
}