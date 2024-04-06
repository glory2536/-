using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Glory.ObjectPool;

public class DamagePopUp : PoolObject
{
    [SerializeField] private TextMeshPro damageTMP;

    [SerializeField] private float lifeTime = 0.6f;

    [SerializeField] private float moveYSpeed = 1f;
    [SerializeField] private float fadeOutSpeed = 0.1f;//Color alpha -> fade out speed

    private void Start()
    {
        key = "DamagePopUp";
    }

    private void OnEnable()
    {
        if(damageTMP == null && TryGetComponent<TextMeshPro>(out TextMeshPro text))
        {
            damageTMP = text;
        }
    }

    IEnumerator DamagePopUpEvent()
    {
        Color textColor = damageTMP.color;
        lifeTime = 1f;

        while (true)
        {
            transform.LookAt(2 * transform.position - Camera.main.transform.position);
            transform.position += new Vector3(0f, moveYSpeed * Time.deltaTime, 0f);//move upwards

            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0)
            {
                ObjectPoolManager.Instance.Despawn(this);//반환처리

                /*=>사라지는 연출(점점 흐리게)
                while (true)
                {
                    //textColor.a -= fadeOutSpeed * Time.deltaTime;
                    textColor.a -= 0.01f;

                    Debug.Log(textColor.a);

                    if(textColor.a <= 0)
                    {
                        //반환처리
                        Destroy(this.gameObject);//테스트용
                        break;
                    }
                    yield return null;
                }
                */
                break;
            }
            yield return null;
        }
    }


    public void SetDamageText(int _damage)
    {      
        damageTMP.text = _damage.ToString();
        //StartCoroutine(DamagePopUpEvent());
    }

    public void DamagePopUpEvent(Vector3 _popUpPosition, int _damage)
    {
        transform.position = _popUpPosition;
        SetDamageText(_damage);
        StartCoroutine(DamagePopUpEvent());
    }

}
