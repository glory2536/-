using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

public class GridPlacementSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject cellIndicator;//�� ǥ�ñ� //mouseIndicator =>���콺��ǥ
    [SerializeField]
    private GridInputManager inputManager;
    [SerializeField]
    private Grid grid;

    [SerializeField]
    private BuildingDataMG buildingDatabase;
    private int selectedObjectIndex = -1;//-1�̸� nulló��
    [SerializeField]
    private GameObject gridVisualization;//�׸���ð�ȭ


    private GridData buildingGridData;//�ǹ��׸��嵥����(=> ���Ŀ� ����,�ٴ����� �������� �з����ֱ�)

    //�̸����� �ǹ� ������Ʈ //cellIndicator(��ǥ�ñ�)������Ʈ �ڽĿ� ��ġ
    [SerializeField] private Renderer previewRenderer;

    private GameObject previewGameObject;

    private List<GameObject> placedGameObjects = new();

    private void Start()
    {
        buildingGridData = new();
        //cellIndicator = cellIndicator.transform.GetChild(0).gameObject;
        //previewRenderer = cellIndicator.GetComponentInChildren<Renderer>();
        //previewRenderer = cellIndicator.transform.GetChild(0).GetChild(0).GetComponent<Renderer>();

    }

    /// <summary> �Ǽ���� ���� </summary>
    public void StartPlacement(int _ID)
    {
        Debug.Log($"BuildingID =>{_ID}");
        BuildingObjectData outkey;

        foreach (var _tryGetValue in buildingDatabase.buildingData)
        {
            if (_tryGetValue.ID.Equals(_ID))
            {
                Debug.Log(_ID + "=> selectedObjectIndex");

                selectedObjectIndex = _ID;
                gridVisualization.SetActive(true);
                cellIndicator.transform.GetComponent<Transform>().localScale = new Vector3(_tryGetValue.Size.x, _tryGetValue.Size.y, 1);
                //previewRenderer.GetComponent<Transform>().localScale = new Vector3(previewRenderer.GetComponent<Transform>().localScale.x/ outkey.Size.x, previewRenderer.GetComponent<Transform>().localScale.x / outkey.Size.y, 1);

                previewGameObject = _tryGetValue.buildingPrefab;
                cellIndicator.SetActive(true);
                inputManager.OnClicked += PlaceStructure;
                inputManager.OnExit += StopPlacement;
            }
        }
    }

    /// <summary> �ǹ� ��ġ </summary>
    public void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI()) return;

        //���콺��ġ,�׸�����ġ
        Vector3 mousePosition = inputManager.GetSelecteMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        //�ڸ�üũ�ؼ� ������ return
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (placementValidity == false)
            return;

        //�ڸ������� �����ջ��� or �����ִ� �ǹ� ��ġ ���� �����߿� ����
        //GameObject newObject = Instantiate(buildingDatabase.buildingData[selectedObjectIndex].buildingPrefab);
        GameObject newObject = buildingDatabase.buildingData[selectedObjectIndex].buildingPrefab;
        newObject.transform.position = grid.CellToWorld(gridPosition) + new Vector3(-0.5f, 0.01f, -0.5f);

        placedGameObjects.Add(newObject);

        GridData selectedData = buildingGridData;
        selectedData.AddObjectAt(gridPosition,
            buildingDatabase.buildingData[selectedObjectIndex].Size,
            buildingDatabase.buildingData[selectedObjectIndex].ID,
            placedGameObjects.Count - 1
            );

        if (buildingDatabase.buildingData[selectedObjectIndex].buildingPrefab.transform.GetChild(0).TryGetComponent(out InteractionObject ob))
        {
            ob.enabled = true;
        }

        //Json => �ǹ� ��ġ Ȱ��ȭ
        buildingDatabase.buildingData[selectedObjectIndex].lastBuildingPosition = newObject.transform.position;//��ġ��
        buildingDatabase.buildingData[selectedObjectIndex].isActive = true;//Active
        buildingDatabase.BuildingDataToJson();

        StopPlacement();
    }

    //Validity = Ÿ�缺, Placement = ����
    /// <summary>�Ǽ� ���� üũ(bool) </summary>
    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = buildingGridData;
        return selectedData.CanPlaceObjectAt(gridPosition, buildingDatabase.buildingData[selectedObjectIndex].Size);
    }

    /// <summary> �Ǽ���� ���� </summary>
    public void StopPlacement()
    {
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
        gridVisualization.SetActive(false);
        cellIndicator.SetActive(false);
        selectedObjectIndex = -1;
    }

    private void Update()
    {
        //Vector3Int  => ������ ����Ͽ� 3D ���� �� ���� ǥ��
        //LocalToCell => ���� ���������� �� �������� ����
        //WorldToCell => World���������� �� �������� ����
        //CellToWorld => �� ��ġ�� ���� ��ġ �������� ��ȯ
        //�ǹ� ����Ʈ �����ϴ�

        if (selectedObjectIndex < 0)
            return;

        Vector3 mousePosition = inputManager.GetSelecteMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        // previewRenderer.material.color = placementValidity ? Color.white : Color.red;
        cellIndicator.transform.GetChild(0).GetComponent<Renderer>().material.color = placementValidity ? Color.white : Color.red;

        //mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.CellToWorld(gridPosition) + new Vector3(-0.5f, 0.1f, -0.5f);

        previewGameObject.transform.position = grid.CellToWorld(gridPosition) + new Vector3(-0.5f, 0.1f, -0.5f);
    }



}
