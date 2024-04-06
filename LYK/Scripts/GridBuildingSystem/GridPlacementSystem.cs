using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

public class GridPlacementSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject cellIndicator;//셀 표시기 //mouseIndicator =>마우스지표
    [SerializeField]
    private GridInputManager inputManager;
    [SerializeField]
    private Grid grid;

    [SerializeField]
    private BuildingDataMG buildingDatabase;
    private int selectedObjectIndex = -1;//-1이면 null처리
    [SerializeField]
    private GameObject gridVisualization;//그리드시각화


    private GridData buildingGridData;//건물그리드데이터(=> 추후에 가구,바닥으로 나눌꺼면 분류해주기)

    //미리보기 건물 오브젝트 //cellIndicator(셀표시기)오브젝트 자식에 배치
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

    /// <summary> 건설모드 시작 </summary>
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

    /// <summary> 건물 배치 </summary>
    public void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI()) return;

        //마우스위치,그리드위치
        Vector3 mousePosition = inputManager.GetSelecteMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        //자리체크해서 없으면 return
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (placementValidity == false)
            return;

        //자리있으면 프리팹생성 or 씬에있는 건물 위치 변경 형식중에 선택
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

        //Json => 건물 위치 활성화
        buildingDatabase.buildingData[selectedObjectIndex].lastBuildingPosition = newObject.transform.position;//위치값
        buildingDatabase.buildingData[selectedObjectIndex].isActive = true;//Active
        buildingDatabase.BuildingDataToJson();

        StopPlacement();
    }

    //Validity = 타당성, Placement = 놓기
    /// <summary>건설 공간 체크(bool) </summary>
    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = buildingGridData;
        return selectedData.CanPlaceObjectAt(gridPosition, buildingDatabase.buildingData[selectedObjectIndex].Size);
    }

    /// <summary> 건설모드 종료 </summary>
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
        //Vector3Int  => 정수를 사용하여 3D 벡터 및 점을 표현
        //LocalToCell => 로컬 포지션으로 셀 포지션을 얻음
        //WorldToCell => World포지션으로 셀 포지션을 얻음
        //CellToWorld => 셀 위치를 월드 위치 공간으로 변환
        //피벗 포인트 왼쪽하단

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
