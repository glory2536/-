using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildingObjectData
{
    [field: SerializeField]
    public int ID { get; private set; }

    [field: SerializeField]
    public string Name { get; private set; }

    [field: SerializeField]
    public Vector2Int Size { get; private set; } = Vector2Int.one;//πÃ¡§

    public int limitPlayerLevel;

    public GameObject buildingPrefab;
    public Sprite buildingImage;

    //public ItemData craftItem;
    public int craftItem_Index = -1;
    public int craftItemAmount;

    //public ItemData craftItem2;
    public int craftItem2_Index = -1;
    public int craftItemAmount2;

    public Vector3 lastBuildingPosition;
    public bool isActive = false;


    //public BuildingObjectData(int _ID, string _Name, Vector2Int _size, int _limitPlayerLevel,ItemData _craftItem, int _craftItemAmount, ItemData _craftItem2, int _craftItemAmount2)
    public BuildingObjectData(int _ID, string _Name, Vector2Int _size, int _limitPlayerLevel, int _craftItemIndex, int _craftItemAmount, int _craftItem2Index, int _craftItemAmount2)
    {
        ID = _ID;
        Name = _Name;
        Size = _size;
        limitPlayerLevel = _limitPlayerLevel;
        craftItem_Index = _craftItemIndex;
        craftItemAmount = _craftItemAmount;
        craftItem2_Index = _craftItem2Index;
        craftItemAmount2 = _craftItemAmount2;
    }

}
