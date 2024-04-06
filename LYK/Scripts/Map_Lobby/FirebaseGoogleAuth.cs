using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
//using static System.Net.Mime.MediaTypeNames;
using System;

public class FirebaseGoogleAuth : MonoBehaviour
{
    public TMP_Text textText;//UserId 표시용

    FirebaseAuth auth;
    FirebaseUser user;//인증이 완료된 유저 정보

    [SerializeField] TMP_InputField email;
    [SerializeField] TMP_InputField passWord;

    [SerializeField] LobbyScene lobbyScene;

    public String GetUserID() => user.UserId;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        //※주의 : 구글로그인(GPGS v10.14 사용해야함)
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .RequestIdToken()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        auth = FirebaseAuth.DefaultInstance; // auth => FirebaseAuth 클래스는 모든 API 호출을 위한 게이트웨이

        auth.StateChanged += OnChanged;
    }

    private void OnChanged(object sender, EventArgs e)
    {
        if (auth.CurrentUser != user)
        {
            bool signed = auth.CurrentUser != null;

            if (!signed && user != null)
            {
                Debug.Log("로그아웃");
                textText.text = "ID : ";
            }

            user = auth.CurrentUser;
            if (signed)
            {
                Debug.Log("로그인/아이디교체");
                textText.text = "ID : " + user.UserId;

                lobbyScene.LogginSuccess();//인증 성공 후 처리
            }
        }
    }



    /// <summary> 이메일 - 신규 이용자 생성 </summary>
    public void CreateNewUserEmail()
    {
        auth.CreateUserWithEmailAndPasswordAsync(email.text, passWord.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("IsCanceled =>CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("IsFaulted => CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }

    /// <summary> 이메일 - 로그인 </summary>
    public void LogInEmail()
    {
        auth.SignInWithEmailAndPasswordAsync(email.text, passWord.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }

    /// <summary> 로그아웃 </summary>
    public void LogOut()
    {
        Debug.Log("LogOut");
        user = null;
        auth.SignOut();
        lobbyScene.Init();
    }

    /// <summary> 구글 로그인 체크 </summary>
    public void TryGoogleLogin()
    {
        if (!Social.localUser.authenticated) //인증여부확인
        {
            Social.localUser.Authenticate(success => // 로그인 시도
            {
                if (success)
                {
                    Debug.Log("Login=>Success");

                    CreateUserGoogle(); // Firebase Login 시도
                }
                else
                {
                    Debug.Log("Login=>Fail");
                }
            });
        }
    }

    /// <summary> 구글 로그인 계정 생성 </summary>
    private void CreateUserGoogle()
    {
        Firebase.Auth.Credential credential =
    Firebase.Auth.GoogleAuthProvider.GetCredential(GetGoogleToken(), null);
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }

    /// <summary> 구글 토큰 받아오기</summary>
    string GetGoogleToken()
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            return ((PlayGamesLocalUser)Social.localUser).GetIdToken();
        }
        Debug.Log("not Login");
        return null;
    }

    /// <summary> 익명 로그인 </summary>
    public void LogInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;

            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }
}