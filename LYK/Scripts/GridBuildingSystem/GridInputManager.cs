using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridInputManager : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;

    private Vector3 lastPosition;//터치(클릭)한 위치

    [SerializeField]
    private LayerMask placementLayermask;

    public event Action OnClicked, OnExit;

    private void Start()
    {
        if (sceneCamera == null) sceneCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
            OnClicked?.Invoke();
        if (Input.GetKeyDown(KeyCode.Escape))
            OnExit?.Invoke();
    }

    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();//★포인터가 UI에 있는 경우 True, 아닌경우 false반환

    /// <summary>터치한 위치 반환</summary>
    public Vector3 GetSelecteMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 300, placementLayermask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }


}
