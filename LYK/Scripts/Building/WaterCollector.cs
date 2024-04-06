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
    DateTime startTime;//타이머_시작_시간
    float timeLeft;//타이머_남은시간
    [SerializeField] private int maxTime = 10;//제작시간
    [SerializeField] private int currentWaterCount;//현재 물 보관 개수
    [SerializeField] private int maxWaterCount;//물 최대보관 개수 

    public GameObject Popup_WaterUI;
    public Sprite waterImage;
    public Image imagePoint;
    public TMP_Text timeLeftText;//남은시간 텍스트
    public Slider timeLeftSlider;//남은시간 슬라이더
    public Button waterButton;//UI버튼

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
            //Json데이터있음
            startTime = DateTime.ParseExact(timerStartTime, "yyyy-MM-dd HH:mm:ss", null);
        }
        else
        {
            //타이머 처음 시작 => 현재시간Json에저장
            startTime = DateTime.Now;
            string Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            JsonMG.instance.jsonData.WaterCollectorTimerStartTime = Time;
        }

        //StartCoroutine(WaterTimerCo());
        WaterTimer();
        StartCoroutine(SearchPlayerCo());
    }

    #region 빗물받이 건물 타이머
    /// <summary> 빗물받이 물 타이머 </summary>
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
                Debug.Log("물_보관_최대");
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

    /// <summary> 빗물받이 건물 버튼 이벤트 </summary>
    private void WaterButtonOnClick()
    {
        bool fullWatercheck = false;
        for (int i = 0; i < currentWaterCount; i++)
        {
            Inventory.Instance.ItemAdd(ItemDataMG.Instance.GetItemData("물"));
        }

        if (currentWaterCount == maxWaterCount) fullWatercheck = true;
        currentWaterCount = 0;
        if (fullWatercheck)
        {
            timeLeft = maxTime;
            StartCoroutine(WaterTimerCo());
        }

        TimeSpan leftTimeSpan = new TimeSpan(0, 0, maxTime - (int)timeLeft);
        DateTime resultTime = DateTime.Now.Subtract(leftTimeSpan);//현재시간 - 타이머남은시간
        string Time = resultTime.ToString("yyyy-MM-dd HH:mm:ss");
        JsonMG.instance.jsonData.WaterCollectorTimerStartTime = Time;
    }


    /// <summary> 빗물받이_건물_UI </summary>
    private void WaterCollectorUI()
    {
        //팝업UI
        Popup_WaterUI.SetActive(true);
        Popup_WaterUI.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 1f, 0));

        imagePoint.sprite = waterImage;//물 이미지
        if (imagePoint.transform.GetChild(0).TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI text))
        {
            text.text = string.Format("{0}/{1}", currentWaterCount, maxWaterCount);//보관중인 물 개수
        }

        //남은시간 텍스트
        int minutes = Mathf.FloorToInt(timer.leftTime / 60);
        int seconds = Mathf.FloorToInt(timer.leftTime % 60);
        timeLeftText.text = string.Format("{0:00} : {1:00}", minutes, seconds);

        //남은시간 슬라이더
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
                Popup_WaterUI.SetActive(false);//팝업UI
            }
            //yield return waitTime;
            yield return null;
        }
    }


    public override void InteractionEvent()
    {

    }

}
