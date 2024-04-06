using Glory.InventorySystem;
using Glory.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> DropItem ������Ʈ�� �������ִ� ��ũ��Ʈ </summary>
public class ObejctPoolDropItem : PoolObject
{
   // string key = "DropItem";
    private Vector3 startPos, endPos;//������ ����,�� ��ġ
    public ItemData itemInfo;//����������

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

        //ObejctPoolDropItem poolOb = (ObejctPoolDropItem)ObjectPoolManager.Instance.Spawn(key);//������Ʈ Ǯ���� ��������
        //poolOb.GetComponent<BoxCollider>().enabled = true;
        GetComponent<SpriteRenderer>().sprite = _dropItemInfo.dropItemData.itemSprite;//��� ������ �̹��� ����
        itemInfo = _dropItemInfo.dropItemData;//�����������ѱ��

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

        ItemDrop(startPosition, endPos);//��������� ��ġ+������ ó��                                      

        yield return null;
    }

    /// <summary> ���ӿ�����Ʈ Ȱ��ȭ </summary>
    #region ������Ʈ Ȱ��ȭ
    public override void Activate()
    {
        base.Activate();
        GetComponent<BoxCollider>().enabled = false;
        //DropItem();
        Invoke("AddItem", 3);
    }
    #endregion

    /// /// <summary> ���ӿ�����Ʈ ��Ȱ��ȭ </summary>
    #region ���ӿ�����Ʈ ��Ȱ��ȭ
    public override void Deactivate()
    {
        itemInfo = null;
        GetComponent<BoxCollider>().enabled = false;
        base.Deactivate();
    }
    #endregion

    /// <summary> ������ </summary>
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

    /// <summary> ������ ��� </summary>
    [ContextMenu("ItemDrop")]
    public void ItemDrop(Vector3 startPosition, Vector3 endPosition)
    {
        startPos = startPosition;

        //endPos = startPos + new Vector3(UnityEngine.Random.Range(-1, 1), 0, UnityEngine.Random.Range(-1, 1));
        endPos = endPosition;

        StartCoroutine(ItemDropCo());
    }

    /// <summary> ������ �߰� =>�κ��丮 </summary>
    public void AddItem()
    {
        int itemAmount;
        if (itemInfo == null) return;

        itemAmount = Inventory.Instance.ItemAdd(itemInfo);//�κ��丮�� �������߰�
        if (itemAmount != 0)
        {
            //Debug.Log("�κ��丮�� �ڸ����ٰ� UI�� ǥ�����ֱ�");
            GetComponent<BoxCollider>().enabled = true;//���� ȹ�� ������� ����
        }
        else
        {
            GetComponent<BoxCollider>().enabled = false;
            ObjectPoolManager.Instance.Despawn(this);//Ǯ��ȯ
            Deactivate();
        }

    }

    /// <summary> ĳ���Ͷ� �浹 üũ </summary>
    private void OnTriggerEnter(Collider other)
    {
        //�κ��丮�� �ڸ������� �浹�������� ó��
        if (other.tag == "Player")
        {
            AddItem();
        }
    }
}