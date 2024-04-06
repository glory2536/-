using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using System;


[ExecuteInEditMode]
public class ItemDataMG : MonoBehaviour, ISerializationCallbackReceiver
{
    #region Singleton
    private static ItemDataMG instance;
    public static ItemDataMG Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<ItemDataMG>();
                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    var newObj = new GameObject().AddComponent<ItemDataMG>();
                    instance = newObj;
                }
            }
            return instance;
        }
    }
    #endregion

    //딕셔너리_인스펙터용(나중에 제거)
    public List<int> EquipmentDataDictionary_keys = new List<int>();
    [SerializeReference] private List<EquipmentData> equipItem_Dictionary_values = new List<EquipmentData>();
    [SerializeReference] private List<CountableItemData> countAble_Dictionary_values = new List<CountableItemData>();

    //장비 아이템 딕셔너리
    public Dictionary<int, EquipmentData> equipItem = new Dictionary<int, EquipmentData>();
    //수량있는 아이템 딕셔너리
    public Dictionary<int, CountableItemData> countAbleItem = new Dictionary<int, CountableItemData>();

    //장비 아이템 프리팹 => 추후에 경로값으로 저장
    [Header("- EquipItemPrefabs")]
    [SerializeField] private GameObject[] weaponPrefabs;
    [SerializeField] private GameObject[] headPrefabs;
    [SerializeField] private GameObject[] bodyPrefabs;
    [SerializeField] private GameObject[] handPrefabs;
    [SerializeField] private GameObject[] shoesPrefabs;
    [SerializeField] private GameObject[] resourceWeaponPrefabs;

    //장비 아이템 이미지 => 추후에 경로값으로 저장
    [Header("- EquipItemSprite")]
    [SerializeField] private Sprite[] wepaonSprite;
    [SerializeField] private Sprite[] headSprite;
    [SerializeField] private Sprite[] bodySprite;
    [SerializeField] private Sprite[] handSprite;
    [SerializeField] private Sprite[] shoesSprite;
    [SerializeField] private Sprite[] resourceWeaponSprite;

    //자원 장비 아이템 이미지 => 추후에 경로값으로 저장
    [Header("- CountAbleSprite")]
    [SerializeField] private Sprite[] resourcesSprite;
    [SerializeField] private Sprite[] potionSprite;

    private void Awake()
    {
        StartCoroutine(GetWeaponData());
        StartCoroutine(GetHeadData());
        StartCoroutine(GetBodyData());
        StartCoroutine(GetHandData());
        StartCoroutine(GetShoesData());
        StartCoroutine(GetResourceWeaponData());
        StartCoroutine(GetResourcesData());
        StartCoroutine(GetPotionData());
    }

    public ItemData GetItemData(int _key)
    {
        if (_key == -1) return null;

        if(_key >= 200)//장비
        {
            return equipItem[_key];
        }
        return countAbleItem[_key];

    }

    public ItemData GetItemData(string _itemName)
    {
        foreach(var k in countAbleItem)
        {
            if(k.Value.itemName.Equals(_itemName)) return k.Value;
        }
        return null ;
    }


    #region 아이템 정보 받아오기
    IEnumerator GetWeaponData()
    {
        const string weaponURL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=1082493372";

        UnityWebRequest www = UnityWebRequest.Get(weaponURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        string[] line = data.Substring(0, data.Length).Split('\n');

        for (int i = 1; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');

            equipItem.Add(int.Parse(row[0]), new WeaponData(int.Parse(row[0]), (WeaponType)int.Parse(row[1]), (Grade)int.Parse(row[2]), row[3], int.Parse(row[4]), float.Parse(row[5]), float.Parse(row[6]), weaponPrefabs[i - 1], wepaonSprite[i - 1],
                int.Parse(row[7]), int.Parse(row[8]), int.Parse(row[9]), int.Parse(row[10])));
        }
    }

    IEnumerator GetHeadData()
    {
        const string headURL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=141329754";

        UnityWebRequest www = UnityWebRequest.Get(headURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        string[] line = data.Substring(0, data.Length).Split('\n');

        for (int i = 1; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');

            equipItem.Add(int.Parse(row[0]), new HeadData(int.Parse(row[0]), (Grade)int.Parse(row[1]), row[2], int.Parse(row[3]), int.Parse(row[4]), int.Parse(row[5]), int.Parse(row[6]), headPrefabs[i - 1], headSprite[i - 1]));
        }
    }

    IEnumerator GetBodyData()
    {
        const string bodyURL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=2071770501";

        UnityWebRequest www = UnityWebRequest.Get(bodyURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        string[] line = data.Substring(0, data.Length).Split('\n');

        for (int i = 1; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');

            equipItem.Add(int.Parse(row[0]), new BodyData(int.Parse(row[0]), (Grade)int.Parse(row[1]), row[2], int.Parse(row[3]), int.Parse(row[4]), int.Parse(row[5]), int.Parse(row[6]), bodyPrefabs[i - 1], bodySprite[i - 1]));
        }
    }

    IEnumerator GetHandData()
    {
        const string handURL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=808428932";

        UnityWebRequest www = UnityWebRequest.Get(handURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        string[] line = data.Substring(0, data.Length).Split('\n');

        for (int i = 1; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');

            equipItem.Add(int.Parse(row[0]), new HandData(int.Parse(row[0]), (Grade)int.Parse(row[1]), row[2], int.Parse(row[3]), int.Parse(row[4]), int.Parse(row[5]), int.Parse(row[6]), int.Parse(row[7]), handPrefabs[i - 1], handSprite[i - 1]));
        }
    }

    IEnumerator GetShoesData()
    {
        const string handURL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=73332079";

        UnityWebRequest www = UnityWebRequest.Get(handURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        string[] line = data.Substring(0, data.Length).Split('\n');

        for (int i = 1; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');

            equipItem.Add(int.Parse(row[0]), new ShoesData(int.Parse(row[0]), (Grade)int.Parse(row[1]), row[2], int.Parse(row[3]), int.Parse(row[4]), int.Parse(row[5]), int.Parse(row[6]), int.Parse(row[7]), shoesPrefabs[i - 1], shoesSprite[i - 1]));
        }
    }

    IEnumerator GetResourceWeaponData()
    {
        const string resourceWeaponURL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=269730368";

        UnityWebRequest www = UnityWebRequest.Get(resourceWeaponURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        string[] line = data.Substring(0, data.Length).Split('\n');

        for (int i = 1; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');

            equipItem.Add(int.Parse(row[0]), new ResourceWeaponData(int.Parse(row[0]), (Grade)int.Parse(row[1]), row[2], int.Parse(row[3]), int.Parse(row[4]), resourceWeaponSprite[i - 1], resourceWeaponPrefabs[i - 1]));
        }
    }

    IEnumerator GetResourcesData()
    {
        const string resourceURL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=157714216&range=A1:E35";

        UnityWebRequest www = UnityWebRequest.Get(resourceURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        string[] line = data.Substring(0, data.Length).Split('\n');

        for (int i = 1; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');

            countAbleItem.Add(int.Parse(row[0]), new ResourcesData(int.Parse(row[0]), row[1], int.Parse(row[2]), row[3], resourcesSprite[i - 1], int.Parse(row[4])));
        }
    }
    IEnumerator GetPotionData()
    {
        const string potionURL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=2105663906";//포션_구글시트

        UnityWebRequest www = UnityWebRequest.Get(potionURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        string[] line = data.Substring(0, data.Length).Split('\n');

        for (int i = 1; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');

            countAbleItem.Add(int.Parse(row[0]), new PotionItemData(int.Parse(row[0]), row[1], int.Parse(row[2]), int.Parse(row[3]), int.Parse(row[4]), int.Parse(row[5]), potionSprite[i - 1], int.Parse(row[6])));
        }
    }
    #endregion


    public void OnBeforeSerialize()
    {
        //무기 딕셔너리 리스트화
        EquipmentDataDictionary_keys.Clear();
        equipItem_Dictionary_values.Clear();
        countAble_Dictionary_values.Clear();

        foreach (var kvp in equipItem)
        {
            EquipmentDataDictionary_keys.Add(kvp.Key);
            equipItem_Dictionary_values.Add(kvp.Value);
        }

        foreach(var tt in countAbleItem)
        {
            countAble_Dictionary_values.Add(tt.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        /*
        equipItem = new Dictionary<int, EquipmentData>();

        for (int i = 0; i < Dictionary_values.Count; i++)
            //equipItem.Add(Dictionary_keys[i], Dictionary_values[i]);

        Debug.Log(equipItem.Count);
        */
    }

}
