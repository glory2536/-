using Glory.ObjectPool;
using GooglePlayGames.BasicApi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : InteractionObject
{
    public DropItemInfo dropItemInfo;

    Quaternion originRotation;

    [SerializeField] private float maxHealth = 100f;
    private float CurrentHealth { get; set; }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        Dead = false;
        CurrentHealth = maxHealth;
        originRotation = transform.rotation;
        obType = InteractionObType.Tree;
    }

    /// <summary> ������� �Դ� ���(�ǰ�) </summary>
    public void OnDamage(float damage)
    {
        if (Dead) return;

        CurrentHealth -= damage;
        SoundManager.Instance.PlaySE("TreeChopSound");

        //ü�� && �������� üũ
        if (CurrentHealth <= 0 && !Dead)
        {
            Die();
        }
        else
        {
            ShakeObject();//������Ʈ ����
        }
    }

    /// <summary> ��� ó�� </summary>
    public void Die()
    {
        if (Dead) return;
        if (dropItemInfo.itemKey < 0) return;

        dropItemInfo.dropItemData = ItemDataMG.Instance.GetItemData(dropItemInfo.itemKey);
        StartCoroutine(DieCo());
    }

    /// <summary> ������Ʈ ���� </summary>
    public void ShakeObject()
    {
        if (Dead) return;
        StartCoroutine(ShakeObjectCo());
    }

    IEnumerator ShakeObjectCo()
    {
        //Quaternion originRotation = transform.rotation;
        float time = 0;
        float shakeIntensity = 0.01f;

        while (true)
        {
            if (Dead) break;

            time += Time.deltaTime;
            if (time > 0.15f) break;

            transform.rotation = new Quaternion(originRotation.x + UnityEngine.Random.Range(-shakeIntensity, shakeIntensity) * 2f, originRotation.y, originRotation.z, originRotation.w);
            shakeIntensity -= 0.00005f;

            yield return new WaitForSeconds(0.02f);
        }

        //transform.rotation = originRotation; //�ʱⰪ���� ����
        yield return null;
    }


    /// <summary> ������Ʈ ���� ó�� </summary>
    IEnumerator DieCo()
    {
        Dead = true;
        transform.GetComponent<CapsuleCollider>().enabled = false;
        SoundManager.Instance.PlaySE("TreeFallingSound");

        while (true)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, -90)), 0.007f);

            if (transform.eulerAngles.z < 300 && transform.eulerAngles.z > 250)//transform.eulerAngles => 0~360�� 
            {
                ObjectPoolManager.Instance.GetDropItemPool(transform, dropItemInfo);
                break;
            }
            yield return null;
        }

        //=> �ڿ� ����� �ڵ�
        yield return new WaitForSeconds(5.0f);
        this.gameObject.SetActive(true);
        Dead = false;
        CurrentHealth = maxHealth;
        transform.GetComponent<CapsuleCollider>().enabled = true;
        transform.rotation = originRotation;

        yield return null;
    }

    /// <summary> ���ͷ��� �̺�Ʈ </summary>
    public override void InteractionEvent()
    {
        float woodDamage = PlayerStatLYK.Instance.LumberingDamage.GetValue;
        OnDamage(woodDamage);
    }
}