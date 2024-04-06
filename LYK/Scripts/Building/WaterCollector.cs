using Glory.InventorySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WaterCollector : InteractionObject
{
    int Building_Index;
    string BUilding_Name;

    Timer timer = new();
    DateTime startTime;//Ÿ�̸�_����_�ð�
    float timeLeft;//Ÿ�̸�_�����ð�
    [SerializeField] private int maxTime = 10;//���۽ð�
    [SerializeField] private int currentWaterCount;//���� �� ���� ����
    [SerializeField] private int maxWaterCount;//�� �ִ뺸�� ���� 

    public GameObject Popup_WaterUI;
    public Sprite waterImage;
    public Image imagePoint;
    public TMP_Text timeLeftText;//�����ð� �ؽ�Ʈ
    public Slider timeLeftSlider;//�����ð� �����̴�
    public Button waterButton;//UI��ư

    [SerializeField] LayerMask playerLayer;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        string timerStartTime = JsonMG.instance.jsonData.WaterCollectorTimerStartTime;

        if (timerStartTime != "")
        {
            //Json����������
            startTime = DateTime.ParseExact(timerStartTime, "yyyy-MM-dd HH:mm:ss", null);
        }
        else
        {
            //Ÿ�̸� ó�� ���� => ����ð�Json������
            startTime = DateTime.Now;
            string Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            JsonMG.instance.jsonData.WaterCollectorTimerStartTime = Time;
        }

        //StartCoroutine(WaterTimerCo());
        WaterTimer();
        StartCoroutine(SearchPlayerCo());
    }

    #region �������� �ǹ� Ÿ�̸�
    /// <summary> �������� �� Ÿ�̸� </summary>
    public void WaterTimer()
    {
        DateTime currentTime = DateTime.Now;

        TimeSpan timeDif = currentTime - startTime;

        if (timeDif.Days >= 1)
        {
            currentWaterCount = maxWaterCount;
            return;
        }

        double totalSeconds = timeDif.TotalSeconds;
        while (totalSeconds > maxTime)
        {
            if (currentWaterCount == maxWaterCount) break;

            totalSeconds -= maxTime;
            currentWaterCount += 1;
            if (currentWaterCount == maxWaterCount)
            {
                Debug.Log("��_����_�ִ�");
                break;
            }
        }

        timeLeft = maxTime - (float)totalSeconds;
        StartCoroutine(WaterTimerCo());
    }


    IEnumerator WaterTimerCo()
    {
        if (currentWaterCount < maxWaterCount)
        {
            StartCoroutine(timer.TimerCo(timeLeft, returnValue =>
            {
                if (returnValue)
                {
                    currentWaterCount += 1;
                    if (currentWaterCount >= maxWaterCount)
                    {
                        Debug.Log("MaxWaterCount!");
                        return;
                    }

                    timeLeft = maxTime;
                    StartCoroutine(WaterTimerCo());

                }
            }));
        }
        yield return null;
    }
    #endregion

    /// <summary> �������� �ǹ� ��ư �̺�Ʈ </summary>
    private void WaterButtonOnClick()
    {
        bool fullWatercheck = false;
        for (int i = 0; i < currentWaterCount; i++)
        {
            Inventory.Instance.ItemAdd(ItemDataMG.Instance.GetItemData("��"));
        }

        if (currentWaterCount == maxWaterCount) fullWatercheck = true;
        currentWaterCount = 0;
        if (fullWatercheck)
        {
            timeLeft = maxTime;
            StartCoroutine(WaterTimerCo());
        }

        TimeSpan leftTimeSpan = new TimeSpan(0, 0, maxTime - (int)timeLeft);
        DateTime resultTime = DateTime.Now.Subtract(leftTimeSpan);//����ð� - Ÿ�̸ӳ����ð�
        string Time = resultTime.ToString("yyyy-MM-dd HH:mm:ss");
        JsonMG.instance.jsonData.WaterCollectorTimerStartTime = Time;
    }


    /// <summary> ��������_�ǹ�_UI </summary>
    private void WaterCollectorUI()
    {
        //�˾�UI
        Popup_WaterUI.SetActive(true);
        Popup_WaterUI.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 1f, 0));

        imagePoint.sprite = waterImage;//�� �̹���
        if (imagePoint.transform.GetChild(0).TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI text))
        {
            text.text = string.Format("{0}/{1}", currentWaterCount, maxWaterCount);//�������� �� ����
        }

        //�����ð� �ؽ�Ʈ
        int minutes = Mathf.FloorToInt(timer.leftTime / 60);
        int seconds = Mathf.FloorToInt(timer.leftTime % 60);
        timeLeftText.text = string.Format("{0:00} : {1:00}", minutes, seconds);

        //�����ð� �����̴�
        timeLeftSlider.value = (float)(maxTime - timer.leftTime) / maxTime;

        waterButton.onClick.AddListener(WaterButtonOnClick);
    }

    IEnumerator SearchPlayerCo()
    {
        float searchRadius = 3;
        //WaitForSeconds waitTime = new WaitForSeconds(0.2f);

        while (true)
        {
            Collider[] serachPlayer = Physics.OverlapSphere(transform.position, searchRadius, playerLayer);

            if (serachPlayer.Length > 0)
            {
                WaterCollectorUI();
            }
            else
            {
                Popup_WaterUI.SetActive(false);//�˾�UI
            }
            //yield return waitTime;
            yield return null;
        }
    }


    public override void InteractionEvent()
    {

    }

}
