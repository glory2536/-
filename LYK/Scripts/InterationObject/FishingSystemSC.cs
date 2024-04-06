using Glory.InventorySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 물고기 정보 </summary>
[Serializable]
public class Fish
{
    public int fishID;
    public string fishName;
    public Grade itemGrade;
    public float fishHP;
    public float fishSpeed;
    public float fishLifeTimer;

    public int dropItemIndex;

    public Fish(int _id, string _name, Grade _itemGrade, float _hp, float _speed, int _dropItemIndex, float _fishLifeTimer)
    {
        fishID = _id;
        fishName = _name;
        itemGrade = _itemGrade;
        fishHP = _hp;
        fishSpeed = _speed;
        dropItemIndex = _dropItemIndex;
        fishLifeTimer = _fishLifeTimer;
    }

}

public class FishingSystemSC : MonoBehaviour
{
    [SerializeField] Slider fishPosition;

    float fishMovePosition;
    float fishDestination;

    float fishTimer;
    [SerializeField] float timerMultiplicator = 1f;//=> 낮을수록 더 빨리 물고기가 위치를 바꿈

    float fishSpeed;//변경되는값 그냥 이대로 두기
    [SerializeField] float smoothMotion = 1f;//목표까지 도착속도(물고기속도)

    [SerializeField] Slider hook;
    float hookPosition;
    [SerializeField] float hookSize = 0.1f;
    [SerializeField] float hookPower = 0.05f;
    float hookProgress;
    float hookPullVelocity; //pull velocity => 당기는 속도
    [SerializeField] float hookPullPower = 0.01f;
    [SerializeField] float hookGravityPower = 0.005f;
    [SerializeField] float hookProgressDegradationPower = 0.01f;

    [SerializeField] Slider progressBar;

    [SerializeField] bool pause = false;
    [SerializeField] float failTimer = 10f;
    [SerializeField] TextMeshProUGUI timerText;

    //낚시줄
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public LineRenderer linerenderer;
    public float vertexCount = 12;

    //물고기
    [SerializeField] private Fish[] fishs;
    Fish currentFish;


    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        fishs = new Fish[3];
        fishs[0] = new Fish(0, "농어", Grade.C, 100, 4, 106, 5);
        fishs[1] = new Fish(1, "잉어", Grade.B, 150, 2, 107, 7);
        fishs[2] = new Fish(2, "장어", Grade.A, 200, 1, 109, 10);
    }

    private void OnEnable()
    {
        currentFish = fishs[UnityEngine.Random.Range(0, fishs.Length)];

        fishTimer = 0;
        failTimer = currentFish.fishLifeTimer;
        smoothMotion = currentFish.fishSpeed;
        hookPosition = 0;
        fishPosition.value = 0;
        hookProgress = 0;
        hook.value = 0;
        progressBar.value = 0;
        pause = false;
    }

    private void Update()
    {
        if (pause) return;

        Fish();
        Hook();
        ProgressCheck();
        //FishLine();
    }

    void ProgressCheck()
    {
        float min = hookPosition - 0.05f;
        float max = hookPosition + 0.05f;

        failTimer -= Time.deltaTime;
        timerText.text = ((int)failTimer).ToString();
        if (failTimer < 0f)
        {
            pause = true;
            Debug.Log("낚시 실패");
            FishingFailEvent();
            return;
        }

        if (min < fishPosition.value && fishPosition.value < max)
        {
            Debug.Log("Progress_Attack");
            hookProgress += hookPower * Time.deltaTime;
        }
        else
        {
            Debug.Log("Progress_Degradation");
            hookProgress -= hookProgressDegradationPower * Time.deltaTime;

            /* =>이거 0인상태에서 오래되면 자동으로 실패되게(잠수상태일때 방지)
            if (hookProgress <= 0)
            {
                //pause = true;
                Debug.Log("낚시 실패");
            }
            */
        }

        if (hookProgress >= 1f)//물고기 낚시 성공
        {
            //Win();
            pause = true;
            Debug.Log("낚시 성공");
            //성공이벤트 처리
            FishingSuccessEvent();
            return;
        }

        progressBar.value = Mathf.Clamp(hookProgress, 0f, 1f);

    }

    /// <summary> 낚시 성공 이벤트 </summary>
    void FishingSuccessEvent()
    {
        //물고기 넣어주기
        Debug.Log("=> FishingSuccess");
        progressBar.value = Mathf.Clamp(hookProgress, 0f, 1f);
        Inventory.Instance.ItemAdd(ItemDataMG.Instance.GetItemData(currentFish.dropItemIndex));
        this.gameObject.SetActive(false);
    }

    /// <summary> 낚시 실패 이벤트 </summary>
    void FishingFailEvent()
    {
        Debug.Log("=> FishingFail");
        this.gameObject.SetActive(false);
    }


    void Hook()
    {
        if (Input.GetMouseButton(0))
        {
            hookPullVelocity = Mathf.Clamp(hookPullVelocity, 0, 1);
            hookPullVelocity += hookPullPower * Time.deltaTime;//누르면 파워만큼 높아짐
        }
        hookPullVelocity -= hookGravityPower * Time.deltaTime;//중력값만큼 감소

        hookPosition += hookPullVelocity;
        hookPosition = Mathf.Clamp(hookPosition, 0, 1);//Value값이 최소/최대 사이로 고정

        hook.value = hookPosition;
    }


    /// <summary> 물고기 위치 변경 </summary>
    void Fish()
    {
        fishTimer -= 10 * Time.deltaTime;
        if (fishTimer < 0f)
        {
            //Random.value => 0.0에서 1.0 사이의 임의의 부동 소수점 수를 제공
            fishTimer = UnityEngine.Random.value * timerMultiplicator;//=>이 값에따라 물고기 위치 이동할지 정함

            fishDestination = UnityEngine.Random.value;
        }

        fishMovePosition = Mathf.SmoothDamp(fishPosition.value, fishDestination, ref fishSpeed, smoothMotion);
        fishPosition.value = fishMovePosition;
    }

    /// <summary> 낚시줄 자연스러운 연출 </summary>
    void FishLine()
    {
        var pointList = new List<Vector3>();

        for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
        {
            var tangent1 = Vector3.Lerp(point1.position, point2.position, ratio);
            var tangent2 = Vector3.Lerp(point2.position, point3.position, ratio);
            var curve = Vector3.Lerp(tangent1, tangent2, ratio);

            pointList.Add(curve);
        }

        linerenderer.positionCount = pointList.Count;
        linerenderer.SetPositions(pointList.ToArray());
    }
}
