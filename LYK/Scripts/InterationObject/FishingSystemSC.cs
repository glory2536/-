using Glory.InventorySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> ����� ���� </summary>
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
    [SerializeField] float timerMultiplicator = 1f;//=> �������� �� ���� ����Ⱑ ��ġ�� �ٲ�

    float fishSpeed;//����Ǵ°� �׳� �̴�� �α�
    [SerializeField] float smoothMotion = 1f;//��ǥ���� �����ӵ�(�����ӵ�)

    [SerializeField] Slider hook;
    float hookPosition;
    [SerializeField] float hookSize = 0.1f;
    [SerializeField] float hookPower = 0.05f;
    float hookProgress;
    float hookPullVelocity; //pull velocity => ���� �ӵ�
    [SerializeField] float hookPullPower = 0.01f;
    [SerializeField] float hookGravityPower = 0.005f;
    [SerializeField] float hookProgressDegradationPower = 0.01f;

    [SerializeField] Slider progressBar;

    [SerializeField] bool pause = false;
    [SerializeField] float failTimer = 10f;
    [SerializeField] TextMeshProUGUI timerText;

    //������
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public LineRenderer linerenderer;
    public float vertexCount = 12;

    //�����
    [SerializeField] private Fish[] fishs;
    Fish currentFish;


    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        fishs = new Fish[3];
        fishs[0] = new Fish(0, "���", Grade.C, 100, 4, 106, 5);
        fishs[1] = new Fish(1, "�׾�", Grade.B, 150, 2, 107, 7);
        fishs[2] = new Fish(2, "���", Grade.A, 200, 1, 109, 10);
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
            Debug.Log("���� ����");
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

            /* =>�̰� 0�λ��¿��� �����Ǹ� �ڵ����� ���еǰ�(��������϶� ����)
            if (hookProgress <= 0)
            {
                //pause = true;
                Debug.Log("���� ����");
            }
            */
        }

        if (hookProgress >= 1f)//����� ���� ����
        {
            //Win();
            pause = true;
            Debug.Log("���� ����");
            //�����̺�Ʈ ó��
            FishingSuccessEvent();
            return;
        }

        progressBar.value = Mathf.Clamp(hookProgress, 0f, 1f);

    }

    /// <summary> ���� ���� �̺�Ʈ </summary>
    void FishingSuccessEvent()
    {
        //����� �־��ֱ�
        Debug.Log("=> FishingSuccess");
        progressBar.value = Mathf.Clamp(hookProgress, 0f, 1f);
        Inventory.Instance.ItemAdd(ItemDataMG.Instance.GetItemData(currentFish.dropItemIndex));
        this.gameObject.SetActive(false);
    }

    /// <summary> ���� ���� �̺�Ʈ </summary>
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
            hookPullVelocity += hookPullPower * Time.deltaTime;//������ �Ŀ���ŭ ������
        }
        hookPullVelocity -= hookGravityPower * Time.deltaTime;//�߷°���ŭ ����

        hookPosition += hookPullVelocity;
        hookPosition = Mathf.Clamp(hookPosition, 0, 1);//Value���� �ּ�/�ִ� ���̷� ����

        hook.value = hookPosition;
    }


    /// <summary> ����� ��ġ ���� </summary>
    void Fish()
    {
        fishTimer -= 10 * Time.deltaTime;
        if (fishTimer < 0f)
        {
            //Random.value => 0.0���� 1.0 ������ ������ �ε� �Ҽ��� ���� ����
            fishTimer = UnityEngine.Random.value * timerMultiplicator;//=>�� �������� ����� ��ġ �̵����� ����

            fishDestination = UnityEngine.Random.value;
        }

        fishMovePosition = Mathf.SmoothDamp(fishPosition.value, fishDestination, ref fishSpeed, smoothMotion);
        fishPosition.value = fishMovePosition;
    }

    /// <summary> ������ �ڿ������� ���� </summary>
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
