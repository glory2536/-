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
    public TMP_Text textText;//UserId ǥ�ÿ�

    FirebaseAuth auth;
    FirebaseUser user;//������ �Ϸ�� ���� ����

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
        //������ : ���۷α���(GPGS v10.14 ����ؾ���)
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .RequestIdToken()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        auth = FirebaseAuth.DefaultInstance; // auth => FirebaseAuth Ŭ������ ��� API ȣ���� ���� ����Ʈ����

        auth.StateChanged += OnChanged;
    }

    private void OnChanged(object sender, EventArgs e)
    {
        if (auth.CurrentUser != user)
        {
            bool signed = auth.CurrentUser != null;

            if (!signed && user != null)
            {
                Debug.Log("�α׾ƿ�");
                textText.text = "ID : ";
            }

            user = auth.CurrentUser;
            if (signed)
            {
                Debug.Log("�α���/���̵�ü");
                textText.text = "ID : " + user.UserId;

                lobbyScene.LogginSuccess();//���� ���� �� ó��
            }
        }
    }



    /// <summary> �̸��� - �ű� �̿��� ���� </summary>
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

    /// <summary> �̸��� - �α��� </summary>
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

    /// <summary> �α׾ƿ� </summary>
    public void LogOut()
    {
        Debug.Log("LogOut");
        user = null;
        auth.SignOut();
        lobbyScene.Init();
    }

    /// <summary> ���� �α��� üũ </summary>
    public void TryGoogleLogin()
    {
        if (!Social.localUser.authenticated) //��������Ȯ��
        {
            Social.localUser.Authenticate(success => // �α��� �õ�
            {
                if (success)
                {
                    Debug.Log("Login=>Success");

                    CreateUserGoogle(); // Firebase Login �õ�
                }
                else
                {
                    Debug.Log("Login=>Fail");
                }
            });
        }
    }

    /// <summary> ���� �α��� ���� ���� </summary>
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

    /// <summary> ���� ��ū �޾ƿ���</summary>
    string GetGoogleToken()
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            return ((PlayGamesLocalUser)Social.localUser).GetIdToken();
        }
        Debug.Log("not Login");
        return null;
    }

    /// <summary> �͸� �α��� </summary>
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