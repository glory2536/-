using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    //Key=>����ϰ��ִ� �׸���(1ĭ��), Value => PlacementData
    Dictionary<Vector3Int, PlacementData> placedObjects = new();

    /// <summary> �ǹ���ġ �߰� </summary>
    public void AddObjectAt(Vector3Int _gridPosition, Vector2Int _objectSize, int _ID, int _placeObjectIndex)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(_gridPosition, _objectSize);
        PlacementData data = new PlacementData(positionToOccupy, _ID, _placeObjectIndex);
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                throw new Exception($"Dictionary already contains this cell position{pos}");
            placedObjects[pos] = data;
        }
    }


    /// <summary> ����ϴ� ��ġ ����ؼ� ����Ʈ�� �־��� </summary>
    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> returnVal = new();
        //Debug.Log(gridPosition + "=>GridPosition " + objectSize + "=>ObjectSize");

        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                returnVal.Add(gridPosition + new Vector3Int(x, 0, y));
            }
        }

        /*
        foreach(var t in returnVal)
        {
            Debug.Log(t + "=>returnVal");
        }
        */

        return returnVal;
    }


    /// <summary> ��ġ�˻�(�ߺ� ��ġ�ƴϸ� true��ȯ �ƴϸ� false��ȯ) </summary>
    public bool CanPlaceObjectAt(Vector3Int _gridPosition, Vector2Int _objectSize)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(_gridPosition, _objectSize);
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                return false;
        }
        return true;
    }
}

public class PlacementData
{
    public List<Vector3Int> occupiedPositions;
    public int ID { get; private set; }
    public int PlacedObjectIndex { get; private set; }

    public PlacementData(List<Vector3Int> _occupiedPositions, int _ID, int _PlacedObjectIndex)
    {
        this.occupiedPositions = _occupiedPositions;
        ID = _ID;
        PlacedObjectIndex = _PlacedObjectIndex;
    }

}