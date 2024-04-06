using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpBar : PoolObject
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider hpBackSlider;

    WaitForSeconds corutinWaitTime = new WaitForSeconds(0.1f);
    WaitForSeconds corutinWaitTime2 = new WaitForSeconds(0.5f);

    private void Start()
    {
        key = "HPBar";
    }

    private void OnEnable()
    {
        if(hpSlider == null || hpBackSlider == null)
        {
            hpSlider = transform.GetChild(0).GetChild(0).GetComponent<Slider>();
            hpBackSlider = transform.GetChild(0).GetChild(1).GetComponent<Slider>();
        }
    }

    public void HpBar(Vector3 _enemyPosition, float _currentHp, float _maxHp)
    {
        ShowSldier();
        StopAllCoroutines();
        StartCoroutine(HpBarUpdateCo(_enemyPosition, _currentHp, _maxHp));
        StartCoroutine(HpBarBackUpdateCo());
    }


    IEnumerator HpBarUpdateCo(Vector3 _enemyPosition,float _currentHp ,float _maxHp)
    {
        float speed = 3f;//선형보간속도

        while (true)
        {
            transform.position = _enemyPosition;
            hpSlider.value = Mathf.Lerp(hpSlider.value,_currentHp/_maxHp,Time.deltaTime * speed);//선형보간
            if(hpSlider.value - 0.01f <= _currentHp / _maxHp)
            {
                hpSlider.value = _currentHp / _maxHp;
                break;
            }
            yield return null;
        }
    }

    IEnumerator HpBarBackUpdateCo()
    {
        yield return corutinWaitTime;
        float speed = 4f;//선형보간속도

        while (true)
        {
            hpBackSlider.value = Mathf.Lerp(hpBackSlider.value, hpSlider.value, Time.deltaTime * speed);
            if(hpSlider.value >= hpBackSlider.value - 0.01f)
            {
                hpBackSlider.value = hpSlider.value;
                yield return corutinWaitTime2;
                HideSldier();
                break;
                
            }
            yield return null;
        }
    }

    public void HideSldier()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void ShowSldier()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }
}
