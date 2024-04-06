using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LobbyScene : MonoBehaviour
{
    //파이어베이스 인증 => 로그인 => 데이터 저장 및 불러오기 => 씬 넘김

    //파이어베이스 인증
    [SerializeField] FirebaseGoogleAuth firebaseAuth;//파이어베이스 인증
    [SerializeField] RectTransform firebaseAuthUI;//파이어베이스 인증 UI

    [SerializeField] AddressableAsset addressable;//어드레서블 매니저

    [SerializeField] private GameSetting gameSetting;//세팅 UI에 유저 아이디넘김

    public void Init()
    {
        //파이어 베이스 인증 및 로그인 => 로그인 완료되면 자동으로 다음단계로 넘어감
        firebaseAuth.gameObject.SetActive(true);
        firebaseAuthUI.gameObject.SetActive(true);
    }

    /// <summary>로그인 성공 </summary>
    public void LogginSuccess()
    {
        //최초 로그인시 DB초기화 //이미 로그인상태면 통과
        DatabaseManager.Instance.GetData(firebaseAuth.GetUserID());

        firebaseAuthUI.gameObject.SetActive(false);//로그인UI
        addressable.gameObject.SetActive(true);//어드레서블 매니저

        gameSetting.playerIDText.text = $"ID:{firebaseAuth.GetUserID()}";

    }

}
