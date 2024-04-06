using Glory.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class BuildingDataMG : MonoBehaviour
{
    //�ν����Ϳ�
    [SerializeReference] private List<BuildingObjectData> buildingData_Dictionary_values = new List<BuildingObjectData>();

    public List<BuildingObjectData> buildingData = new();

    [SerializeField] GameObject[] buildingModelPrefab;
    [SerializeField] Sprite[] buildingModelImage;

    public GameObject limitLevelSlotPrefab;//������ ����
    public Transform limitLevelSlotParent;
    public GameObject buildingSlotPrefab;//�ǹ� ����

    public Transform buildingUI;
    public Transform buildingUIToolTip;

    int limitMaxLevel = 4;
    int touchSlotIndex;
    [SerializeField] GridPlacementSystem gridPlacementSystem;


    private void Awake() => Init();


    #region ��ũ��Ʈ ó�����۽� ó���ϴ� �̺�Ʈ
    public void Init()
    {
        Debug.Log(JsonMG.instance.jsonData.buildingData.Count);
        if (JsonMG.instance.jsonData.buildingData.Count <= 0)//ó�� ����
        {
            Debug.Log("=>New_JsonBuildingData");
            StartCoroutine(GetBuildingData());
        }
        else//���� ������ ����
        {
            Debug.Log("=>Exist_JsonBuildingData");
            BuildingDataFromJson();
            for (int i = 0; i < buildingData.Count; i++)
            {
                buildingData[i].buildingPrefab = buildingModelPrefab[i];
                buildingData[i].buildingImage = buildingModelImage[i];
            }

            //������ �ִ� �ǹ� ����
            foreach (var _building in buildingData)
            {
                if (!_building.isActive) continue;

                //Instantiate(_building.buildingPrefab, _building.lastBuildingPosition, Quaternion.identity);

                Transform newObject = _building.buildingPrefab.transform;
                newObject.position = _building.lastBuildingPosition;

                if (newObject.GetChild(0).TryGetComponent(out InteractionObject ob))
                {
                    ob.enabled = true;
                }
            }

            SetBuildingUI();
        }

        //�Ǽ���ư�̺�Ʈ �߰�
        buildingUIToolTip.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(CraftButton);
    }
    #endregion

    #region ���۽�Ʈ���� �ǹ����� �ҷ�����
    IEnumerator GetBuildingData()
    {
        const string BuildingDataURL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=698500121";

        UnityWebRequest www = UnityWebRequest.Get(BuildingDataURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        string[] line = data.Substring(0, data.Length).Split('\n');

        int buildingIndex;
        string buildingName;
        Vector2Int buildingSize;
        int limitPlayerLevel;
        int craftItem1Index;
        int craftItemAmount1;
        int craftItem2Index;
        int craftItemAmount2;

        for (int i = 1; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');

            buildingIndex = int.Parse(row[0]);
            buildingName = row[1];
            buildingSize = new Vector2Int(int.Parse(row[2]), int.Parse(row[3]));
            limitPlayerLevel = int.Parse(row[4]);

            craftItem1Index = int.Parse(row[5]);
            craftItemAmount1 = int.Parse(row[6]);

            craftItem2Index = int.Parse(row[7]);
            craftItemAmount2 = int.Parse(row[8]);

            buildingData.Add(new BuildingObjectData(buildingIndex, buildingName, buildingSize, limitPlayerLevel, craftItem1Index, craftItemAmount1, craftItem2Index, craftItemAmount2));
        }

        //�ǹ�������,�ǹ��̹���
        for (int i = 0; i < buildingData.Count; i++)
        {
            buildingData[i].buildingPrefab = buildingModelPrefab[i];
            buildingData[i].buildingImage = buildingModelImage[i];
        }

        SetBuildingUI();
        BuildingDataToJson();
    }
    #endregion

    #region �ǹ� UI ó��
    /// <summary> �ǹ����� UI </summary>
    public void SetBuildingUI()
    {
        for (int i = 0; i < limitMaxLevel; i++)
        {
            GameObject levelSlot = Instantiate(limitLevelSlotPrefab, limitLevelSlotParent);
            levelSlot.name = $"Level_{i + 1}";
            levelSlot.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString();//�����ؽ�Ʈ

            List<BuildingObjectData> levelBuilding = new();
            for (int k = 0; k < buildingData.Count; k++)
            {
                if (buildingData[k].limitPlayerLevel.Equals(i + 1))
                {
                    levelBuilding.Add(buildingData[k]);
                }
            }

            for (int b = 0; b < levelBuilding.Count; b++)
            {
                GameObject buildingSlot = Instantiate(buildingSlotPrefab, levelSlot.transform.GetChild(2));
                buildingSlot.name = $"Slot_{b}";

                int index = levelBuilding[b].ID;
                //���� ��ư �̺�Ʈ �߰�
                buildingSlot.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => SlotOnClickEvent(index));

                //�ǹ��̹���
                buildingSlot.transform.GetChild(1).GetComponent<Image>().sprite = levelBuilding[b].buildingImage;

                //�ǹ��̸�
                buildingSlot.transform.GetChild(2).GetComponent<TMP_Text>().text = levelBuilding[b].Name;
            }
        }
    }

    /// <summary>�ǹ� ���� Ŭ�� �̺�Ʈ </summary>
    private void SlotOnClickEvent(int _slotIndex)
    {
        BuildingObjectData touchBuildingSlot = buildingData[_slotIndex];
        if (touchBuildingSlot.isActive == true) return; //=>�̹� �Ǽ����¸� ����ó�����ֱ�

        ItemData craftResourceItem1 = ItemDataMG.Instance.GetItemData(touchBuildingSlot.craftItem_Index);
        ItemData craftResourceItem2 = ItemDataMG.Instance.GetItemData(touchBuildingSlot.craftItem2_Index);

        //���������ֱ�
        buildingUIToolTip.gameObject.SetActive(true);
        buildingUIToolTip.transform.GetChild(0).GetComponent<Image>().sprite = touchBuildingSlot.buildingImage;
        buildingUIToolTip.transform.GetChild(1).GetComponent<TMP_Text>().text = touchBuildingSlot.Name;

        if (craftResourceItem1 != null)
        {
            buildingUIToolTip.GetChild(2).GetChild(0).GetComponent<Image>().sprite = craftResourceItem1.itemSprite;
            buildingUIToolTip.GetChild(2).GetChild(1).GetComponent<TMP_Text>().text =
                $"{Inventory.Instance.GetCurrentAmount(craftResourceItem1.itemName)} / {touchBuildingSlot.craftItemAmount}";
        }

        if (craftResourceItem2 != null)
        {
            buildingUIToolTip.GetChild(3).gameObject.SetActive(true);
            buildingUIToolTip.GetChild(3).GetChild(0).GetComponent<Image>().sprite = craftResourceItem2.itemSprite;
            buildingUIToolTip.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text =
                $"{Inventory.Instance.GetCurrentAmount(craftResourceItem2.itemName)} / {touchBuildingSlot.craftItemAmount2}";
        }
        else
        {
            buildingUIToolTip.GetChild(3).gameObject.SetActive(false);
        }

        touchSlotIndex = _slotIndex;
    }

    /// <summary> �ǹ��Ǽ� ��ư �̺�Ʈ </summary>
    void CraftButton()
    {
        Debug.Log(touchSlotIndex + " =>CraftBuliding_ToutchSlotIndex");
        BuildingObjectData touchBuildingSlot = buildingData[touchSlotIndex];
        ItemData craftResourceItem1 = ItemDataMG.Instance.GetItemData(touchBuildingSlot.craftItem_Index);
        ItemData craftResourceItem2 = ItemDataMG.Instance.GetItemData(touchBuildingSlot.craftItem2_Index);

        if (craftResourceItem1 == null) return;

        if (Inventory.Instance.GetCurrentAmount(craftResourceItem1.itemName) >= touchBuildingSlot.craftItemAmount)
        {
            Debug.Log(craftResourceItem1.itemName);
            if (craftResourceItem2 != null)
            {
                if (Inventory.Instance.GetCurrentAmount(craftResourceItem2.itemName) > touchBuildingSlot.craftItemAmount2)
                {
                    Inventory.Instance.CountAbleItemSubtract(craftResourceItem1.itemName, touchBuildingSlot.craftItemAmount);
                    Inventory.Instance.CountAbleItemSubtract(craftResourceItem2.itemName, touchBuildingSlot.craftItemAmount2);

                    buildingUIToolTip.gameObject.SetActive(false);
                    buildingUI.gameObject.SetActive(false);
                    gridPlacementSystem.StartPlacement((touchBuildingSlot.ID));

                    return;
                }
            }
            else
            {
                Inventory.Instance.CountAbleItemSubtract(craftResourceItem1.itemName, touchBuildingSlot.craftItemAmount);

                buildingUIToolTip.gameObject.SetActive(false);
                buildingUI.gameObject.SetActive(false);
                gridPlacementSystem.StartPlacement((touchBuildingSlot.ID));

                return;
            }
        }

        Debug.Log("false => �������ϴٰ� �˾�â����");
    }
    #endregion

    #region Jsonó��
    public void BuildingDataToJson()
    {
        JsonMG.instance.jsonData.buildingData = buildingData;
        JsonMG.instance.SaveDataToJson();
    }

    public void BuildingDataFromJson()
    {
        if (JsonMG.instance.jsonData.buildingData.Count <= 0)
        {
            BuildingDataToJson();
            Debug.Log("=>BuildingDataToJson");
            return;
        }
        Debug.Log("=>BuildingDataFromJson");
        buildingData = JsonMG.instance.jsonData.buildingData;
    }
    #endregion
}
