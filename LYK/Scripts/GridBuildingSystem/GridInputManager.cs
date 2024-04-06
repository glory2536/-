using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridInputManager : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;

    private Vector3 lastPosition;//��ġ(Ŭ��)�� ��ġ

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
        => EventSystem.current.IsPointerOverGameObject();//�������Ͱ� UI�� �ִ� ��� True, �ƴѰ�� false��ȯ

    /// <summary>��ġ�� ��ġ ��ȯ</summary>
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
