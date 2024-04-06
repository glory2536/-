using GooglePlayGames.BasicApi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseCampMg : MonoBehaviour
{
    [SerializeField, Range(0.01f,10f)]
    private float fadeTime;//fadeSpeed ���� 10�̸� 1��(���� Ŭ���� ����)
    [SerializeField]
    private Image fadeOutImage;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        //�÷��̾� ó��
        GameObject player = PlayerStatLYK.Instance.gameObject;
        if(player == null)
        {
            Debug.Log("PlaeyrNull");
            return;
        }
        player.SetActive(true);
        player.GetComponent<Transform>().position = Vector3.zero;
        player.GetComponent<Player>().enabled = true;
        //ī�޶�
        Camera.main.transform.parent.gameObject.transform.position = new Vector3(0, 10, -5.5f);
        //���̵�ƿ�
        StartCoroutine(Fade(1, 0));
    }

    /// <summary> ���̵� �ƿ� ȿ�� </summary>
    private IEnumerator Fade(float start, float end)
    {
        float currentTime = 0.0f;
        float percent = 0.0f;

        while(percent < 1)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / fadeTime;

            Color color = fadeOutImage.color;
            color.a = Mathf.Lerp(start, end, percent);
            fadeOutImage.color = color;

            yield return null;
        }
        fadeOutImage.gameObject.SetActive(false);
    }

}
