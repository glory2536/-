using Glory.InventorySystem;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
//using static UnityEditor.Progress;
//using Newtonsoft.Json;

[Serializable]
public class JsonData
{
    //public int level;
    public string WaterCollectorTimerStartTime;//��_������_Ÿ�̸�_���۽ð�

    public ShopItem[] busShopItemList;//��������_�����۵�_���
    public string busShopTimerStartTime;//��������_Ÿ�̸ӽ��۽ð�

    public List<BuildingObjectData> buildingData = new();

    public int playerLevel;
    public float playerExp;
    public float playerHp;
    public float playerHungry;
    public float playerThirsty;

    public JsonData()
    {
        playerLevel = 1;
        playerExp = 0;
        playerHp = 100;
        playerHungry = 100;
        playerThirsty = 100;
    }
}

[Serializable]
public class JsonItemData
{
    //������ ������
    public int capacity;
    public int maxCapacity;

    public Dictionary<int, Item> jsonItem = new();
    public Dictionary<int, JsonCountableItem> jsonCountableItem = new();
    public Dictionary<int, JsonEquipItemData> jsonEquipItem = new();

    public void SaveItemDataAll(Item[] _items)
    {
        jsonItem.Clear();
        jsonCountableItem.Clear();
        jsonEquipItem.Clear();

        for(int slotIndex=0; slotIndex < _items.Length; slotIndex++)
        {
            if (_items[slotIndex] is CountableItem countableItem)
            {
                //�����ִ� ������
                Debug.Log("SaveJsonCountableItemData");
                jsonCountableItem.Add(slotIndex, new JsonCountableItem(countableItem.Data.itemID, countableItem.CurrentAmount));
            }
            else if (_items[slotIndex] is EquipmentItem equipItem)
            {
                //�����ִ� ������
                Debug.Log("SaveJsonEquipmentItemItemData");
                jsonEquipItem.Add(slotIndex, new JsonEquipItemData(equipItem.Data.itemID, equipItem.currentUpgradeValue));
            }
            else
            {
                //�ƹ��͵����� Null
                jsonItem.Add(slotIndex, _items[slotIndex]);
            }
        }
    }

    //public Item[] LoadJsonAllChestItem()
    public Item[] LoadItemDataAll()
    {
        Item[] chestLoadItems = new Item[maxCapacity];

        for (int i=0; i < chestLoadItems.Length; i++)
        {
            chestLoadItems[i] = null;
        }

        for (int i = 0; i < chestLoadItems.Length; i++)
        {
            foreach(var item in jsonItem)
            {
                chestLoadItems[item.Key] = item.Value;
            }
            foreach (var item in jsonCountableItem)
            {
                if(ItemDataMG.Instance.GetItemData(item.Value.itemID) is PotionItemData potionData)
                {
                    chestLoadItems[item.Key] = new PotionItem(potionData, item.Value.currentAmount);
                }
                else if(ItemDataMG.Instance.GetItemData(item.Value.itemID) is ResourcesData resourceItem)
                {
                    chestLoadItems[item.Key] = new ResourceItem(resourceItem, item.Value.currentAmount);
                }
            }
            foreach (var item in jsonEquipItem)
            {
                chestLoadItems[item.Key] = new EquipmentItem((EquipmentData)ItemDataMG.Instance.GetItemData(item.Value.itemID), item.Value.currentUpgradeValue);
            }
        }
        return chestLoadItems;

    }

    public class JsonCountableItem
    {
        public int itemID;//������ ���̵�
        public int currentAmount;//�����۰���

        public JsonCountableItem(int _itemID, int _currentAmount)
        {
            itemID = _itemID;
            currentAmount = _currentAmount;
        }

    }

    public class JsonEquipItemData
    {
        public int itemID;//������ ���̵�
        public int currentUpgradeValue;//��ȭ��ġ
        public JsonEquipItemData(int _itemID, int _currentUpgradeValue)
        {
            itemID = _itemID;
            currentUpgradeValue = _currentUpgradeValue;
        }

    }
}

public class JsonMG : MonoBehaviour
{
    public static JsonMG instance;

    public JsonData jsonData = new();
    public JsonItemData jsonChestItemData = new();
    public JsonItemData jsonInventoryItemData = new();

    string dataFilePath;
    string chestItemsFilePath;
    string inventoryItemsFilePath;


    private void Awake()
    {
        instance = this;
        Init();
    }

    void Init()
    {
        //�����̸�
        string dataFileName = "jsonData.json";
        string chestItemsFileName = "jsonChestItemsData.json";
        string inventoryItemsFileName = "jsonInventoryItemsData.json";

        //���ϰ��
#if UNITY_EDITOR || UNITY_STANDALONE
        //��� =>E:\MZS_Portfolio\Assets\StreamingAssets
        //Assets ->StreamingAssets ����
        dataFilePath = Path.Combine(Application.streamingAssetsPath, dataFileName);
        chestItemsFilePath = Path.Combine(Application.streamingAssetsPath, chestItemsFileName);
        inventoryItemsFilePath = Path.Combine(Application.streamingAssetsPath, inventoryItemsFileName);
#elif UNITY_ANDROID
        dataFilePath = Path.Combine(Application.persistentDataPath, dataFileName);
        chestItemsFilePath = Path.Combine(Application.persistentDataPath, chestItemsFileName);
        inventoryItemsFilePath = Path.Combine(Application.persistentDataPath, inventoryItemsFileName);
#elif UNITY_IOS
        dataFilePath = Path.Combine(Application.streamingAssetsPath, dataFileName);
        chestItemsFilePath = Path.Combine(Application.streamingAssetsPath, chestItemsFileName);
        inventoryItemsFilePath = Path.Combine(Application.streamingAssetsPath, inventoryItemsFileName);
#else
    //dataFilePath = null;
#endif

        //json���� ���翩��
        FileInfo fileInfo = new FileInfo(dataFilePath);
        if (fileInfo.Exists)//��������
        {
            Debug.Log("File => Exists");
            LoadDataFromJson();
        }
        else
        {
            Debug.Log("File => Fail");
            SaveDataToJson();
        }

    }


    public void SaveDataToJson()
    {
        JsonSerializerSettings setting = new JsonSerializerSettings();
        setting.Formatting = Formatting.Indented;
        setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        
        //�⺻�����͵�
        string data = JsonUtility.ToJson(jsonData); 
        File.WriteAllText(dataFilePath, data);

        //������ �����۵�
        string chestItemsData = JsonConvert.SerializeObject(jsonChestItemData, setting);
        File.WriteAllText(chestItemsFilePath, chestItemsData);

        //�κ��丮 �����۵�
        string inventoryItemsData = JsonConvert.SerializeObject(jsonInventoryItemData, setting);
        File.WriteAllText(inventoryItemsFilePath, inventoryItemsData);

    }

    private void LoadDataFromJson()//Json Load
    {
        //�⺻�����͵�
        string data = File.ReadAllText(dataFilePath);
        jsonData = JsonUtility.FromJson<JsonData>(data);

        //������ �����۵�
        string chestItemData = File.ReadAllText(chestItemsFilePath);
        jsonChestItemData = JsonConvert.DeserializeObject<JsonItemData>(chestItemData);

        //�κ��丮 �����۵�
        string inventoryItemData = File.ReadAllText(inventoryItemsFilePath);
        jsonInventoryItemData = JsonConvert.DeserializeObject<JsonItemData>(inventoryItemData);
    }

    void OnApplicationQuit()
    {
        SaveDataToJson();
        Debug.Log("OnApplicationQuit");      
    }
}