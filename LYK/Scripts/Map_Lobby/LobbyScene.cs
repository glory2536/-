using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LobbyScene : MonoBehaviour
{
    //���̾�̽� ���� => �α��� => ������ ���� �� �ҷ����� => �� �ѱ�

    //���̾�̽� ����
    [SerializeField] FirebaseGoogleAuth firebaseAuth;//���̾�̽� ����
    [SerializeField] RectTransform firebaseAuthUI;//���̾�̽� ���� UI

    [SerializeField] AddressableAsset addressable;//��巹���� �Ŵ���

    [SerializeField] private GameSetting gameSetting;//���� UI�� ���� ���̵�ѱ�

    public void Init()
    {
        //���̾� ���̽� ���� �� �α��� => �α��� �Ϸ�Ǹ� �ڵ����� �����ܰ�� �Ѿ
        firebaseAuth.gameObject.SetActive(true);
        firebaseAuthUI.gameObject.SetActive(true);
    }

    /// <summary>�α��� ���� </summary>
    public void LogginSuccess()
    {
        //���� �α��ν� DB�ʱ�ȭ //�̹� �α��λ��¸� ���
        DatabaseManager.Instance.GetData(firebaseAuth.GetUserID());

        firebaseAuthUI.gameObject.SetActive(false);//�α���UI
        addressable.gameObject.SetActive(true);//��巹���� �Ŵ���

        gameSetting.playerIDText.text = $"ID:{firebaseAuth.GetUserID()}";

    }

}
