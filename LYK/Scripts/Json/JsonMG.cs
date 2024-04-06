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
    public string WaterCollectorTimerStartTime;//물_보관함_타이머_시작시간

    public ShopItem[] busShopItemList;//버스상점_아이템들_목록
    public string busShopTimerStartTime;//버스상점_타이머시작시간

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
    //보관함 아이템
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
                //수량있는 아이템
                Debug.Log("SaveJsonCountableItemData");
                jsonCountableItem.Add(slotIndex, new JsonCountableItem(countableItem.Data.itemID, countableItem.CurrentAmount));
            }
            else if (_items[slotIndex] is EquipmentItem equipItem)
            {
                //수량있는 아이템
                Debug.Log("SaveJsonEquipmentItemItemData");
                jsonEquipItem.Add(slotIndex, new JsonEquipItemData(equipItem.Data.itemID, equipItem.currentUpgradeValue));
            }
            else
            {
                //아무것도없음 Null
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
        public int itemID;//아이템 아이디
        public int currentAmount;//아이템개수

        public JsonCountableItem(int _itemID, int _currentAmount)
        {
            itemID = _itemID;
            currentAmount = _currentAmount;
        }

    }

    public class JsonEquipItemData
    {
        public int itemID;//아이템 아이디
        public int currentUpgradeValue;//강화수치
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
        //파일이름
        string dataFileName = "jsonData.json";
        string chestItemsFileName = "jsonChestItemsData.json";
        string inventoryItemsFileName = "jsonInventoryItemsData.json";

        //파일경로
#if UNITY_EDITOR || UNITY_STANDALONE
        //경로 =>E:\MZS_Portfolio\Assets\StreamingAssets
        //Assets ->StreamingAssets 폴더
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

        //json파일 존재여부
        FileInfo fileInfo = new FileInfo(dataFilePath);
        if (fileInfo.Exists)//파일존재
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
        
        //기본데이터들
        string data = JsonUtility.ToJson(jsonData); 
        File.WriteAllText(dataFilePath, data);

        //보관함 아이템들
        string chestItemsData = JsonConvert.SerializeObject(jsonChestItemData, setting);
        File.WriteAllText(chestItemsFilePath, chestItemsData);

        //인벤토리 아이템들
        string inventoryItemsData = JsonConvert.SerializeObject(jsonInventoryItemData, setting);
        File.WriteAllText(inventoryItemsFilePath, inventoryItemsData);

    }

    private void LoadDataFromJson()//Json Load
    {
        //기본데이터들
        string data = File.ReadAllText(dataFilePath);
        jsonData = JsonUtility.FromJson<JsonData>(data);

        //보관함 아이템들
        string chestItemData = File.ReadAllText(chestItemsFilePath);
        jsonChestItemData = JsonConvert.DeserializeObject<JsonItemData>(chestItemData);

        //인벤토리 아이템들
        string inventoryItemData = File.ReadAllText(inventoryItemsFilePath);
        jsonInventoryItemData = JsonConvert.DeserializeObject<JsonItemData>(inventoryItemData);
    }

    void OnApplicationQuit()
    {
        SaveDataToJson();
        Debug.Log("OnApplicationQuit");      
    }
}