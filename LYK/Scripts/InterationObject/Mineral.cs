using Glory.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Mineral : InteractionObject
{
    public DropItemInfo dropItemInfo;

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
        obType = InteractionObType.Mining;
    }

    /// <summary> ������� �Դ� ���(�ǰ�) </summary>
    public void OnDamage(float damage)
    {
        if (Dead) return;

        CurrentHealth -= damage;
        SoundManager.Instance.PlaySE("MiningSound");

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
        Quaternion originRotation = transform.rotation;
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
        Dead = true;//��� ���� ����
        if (dropItemInfo.itemKey >= 0) dropItemInfo.dropItemData = ItemDataMG.Instance.GetItemData(dropItemInfo.itemKey);

        transform.GetComponent<CapsuleCollider>().enabled = false;
        SoundManager.Instance.PlaySE("RockDebris");

        if (TryGetComponent<Animator>(out Animator animator))
        {
            animator.SetBool("Die", true);
            ObjectPoolManager.Instance.GetDropItemPool(transform, dropItemInfo);

            //=> �ڿ������
            WaitForSeconds resourceResetTime = new WaitForSeconds(5.0f);
            yield return resourceResetTime;

            Dead = false;
            CurrentHealth = maxHealth;
            transform.GetComponent<CapsuleCollider>().enabled = true;
            animator.SetBool("Die", false);
        }

        yield return null;
    }

    /// <summary> ���ͷ��� ��ư </summary>
    public override void InteractionEvent()
    {
        float miningDamage = PlayerStatLYK.Instance.MiningDamage.GetValue;
        OnDamage(miningDamage);
    }
}