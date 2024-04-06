using Glory.InventorySystem;
using Glory.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> DropItem 오브젝트가 가지고있는 스크립트 </summary>
public class ObejctPoolDropItem : PoolObject
{
   // string key = "DropItem";
    private Vector3 startPos, endPos;//포물선 시작,끝 위치
    public ItemData itemInfo;//아이템정보

    private void Start()
    {
        key = "DropItem";
    }

    public void DropItem(Transform _originPosition, DropItemInfo _dropItemInfo, Vector3? _endPos)
    {
        StartCoroutine(DropItemCo(_originPosition, _dropItemInfo, _endPos));
    }

    IEnumerator DropItemCo(Transform _originPosition, DropItemInfo _dropItemInfo, Vector3? _endPos)
    {
        Vector3 originPosition = _originPosition.position;

        //ObejctPoolDropItem poolOb = (ObejctPoolDropItem)ObjectPoolManager.Instance.Spawn(key);//오브젝트 풀에서 가져오기
        //poolOb.GetComponent<BoxCollider>().enabled = true;
        GetComponent<SpriteRenderer>().sprite = _dropItemInfo.dropItemData.itemSprite;//드랍 아이템 이미지 변경
        itemInfo = _dropItemInfo.dropItemData;//아이템정보넘기기

        Vector3 startPosition = originPosition + new Vector3(0, 0.3f, 0);
        Vector3 endPos;
        if(_endPos == null)
        {
            endPos = startPosition + new Vector3(UnityEngine.Random.Range(-1, 1), 0, UnityEngine.Random.Range(-1, 1));
        }
        else
        {
            endPos = startPosition + (Vector3)_endPos;

        }

        ItemDrop(startPosition, endPos);//드랍아이템 위치+포물선 처리                                      

        yield return null;
    }

    /// <summary> 게임오브젝트 활성화 </summary>
    #region 오브젝트 활성화
    public override void Activate()
    {
        base.Activate();
        GetComponent<BoxCollider>().enabled = false;
        //DropItem();
        Invoke("AddItem", 3);
    }
    #endregion

    /// /// <summary> 게임오브젝트 비활성화 </summary>
    #region 게임오브젝트 비활성화
    public override void Deactivate()
    {
        itemInfo = null;
        GetComponent<BoxCollider>().enabled = false;
        base.Deactivate();
    }
    #endregion

    /// <summary> 포물선 </summary>
    protected static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }

    protected IEnumerator ItemDropCo()
    {
        float timer = 0;
        //Debug.Log(startPos +"=> startPos");
        //Debug.Log(endPos + "=> endPos");
        transform.position = startPos;
        while (transform.position.y >= startPos.y)
        {
            timer += Time.deltaTime;
            Vector3 tempPos = Parabola(startPos, endPos, 2, timer);
            transform.position = tempPos;
            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary> 아이템 드랍 </summary>
    [ContextMenu("ItemDrop")]
    public void ItemDrop(Vector3 startPosition, Vector3 endPosition)
    {
        startPos = startPosition;

        //endPos = startPos + new Vector3(UnityEngine.Random.Range(-1, 1), 0, UnityEngine.Random.Range(-1, 1));
        endPos = endPosition;

        StartCoroutine(ItemDropCo());
    }

    /// <summary> 아이템 추가 =>인벤토리 </summary>
    public void AddItem()
    {
        int itemAmount;
        if (itemInfo == null) return;

        itemAmount = Inventory.Instance.ItemAdd(itemInfo);//인벤토리에 아이템추가
        if (itemAmount != 0)
        {
            //Debug.Log("인벤토리에 자리없다고 UI로 표시해주기");
            GetComponent<BoxCollider>().enabled = true;//직접 획득 방식으로 변경
        }
        else
        {
            GetComponent<BoxCollider>().enabled = false;
            ObjectPoolManager.Instance.Despawn(this);//풀반환
            Deactivate();
        }

    }

    /// <summary> 캐릭터랑 충돌 체크 </summary>
    private void OnTriggerEnter(Collider other)
    {
        //인벤토리에 자리없으면 충돌형식으로 처리
        if (other.tag == "Player")
        {
            AddItem();
        }
    }
}