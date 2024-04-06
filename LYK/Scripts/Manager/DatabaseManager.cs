using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;
using Google.MiniJSON;
using Unity.VisualScripting;
using System.Runtime.InteropServices;
using Firebase.Extensions;
using System;
using GooglePlayGames.BasicApi;


[Serializable]
public class DatabaseData
{
    //����
    public int userLevel;
    public float playerExp;
    public float lastHP;
    public float lastHungryValue;
    public float lastThirstyValue;

    /*
    //��������
    //public string userID;
    public DatabaseData(string _userID)
    {
        userID = _userID;
    }
    */
    public DatabaseData()
    {
        userLevel = 1;
        playerExp = 0;
        lastHP = 100;
        lastHungryValue = 100;
        lastThirstyValue = 100;
    }
}


public class DatabaseManager : Singleton<DatabaseManager>
{
    private DatabaseReference databaseReference;
    public DatabaseData databaseData = new();

    string userId;

    private void Awake()
    {
        // ���̾�̽��� ���� ���� ���
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }


    /// <summary> �����ͺ��̽� ������ �ʱ�ȭ </summary>
    public void InitDatabaseData(string _userId)
    {
        databaseData = new();
        string json = JsonUtility.ToJson(databaseData);
        databaseReference.Child(_userId).SetRawJsonValueAsync(json);
        LoadAllData(_userId);
    }


    /// <summary> ������ ���� �����ϴ��� Ȯ�� </summary>
    public void GetData(string _userId)
    {
        FirebaseDatabase.DefaultInstance
    .GetReference(_userId)
    .GetValueAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsFaulted)
        {
            // Handle the error...
        }
        else if (task.IsCompleted)
        {
            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                Debug.Log("Snapshot_Exists => �̹� ����������");
                LoadAllData(_userId);
                userId = _userId;
            }
            else
            {
                Debug.Log("Snapshot_not_Exists => ��������");
                InitDatabaseData(_userId);
                userId = _userId;
            }
        }
    });
    }

    /// <summary> ���ӵ����� DB�� ��ü ����, ����� ���� </summary>
    public void SaveDatabaseData(string _userId)
    {
        databaseData.userLevel = PlayerStatLYK.Instance.CurrentLevel;
        databaseData.playerExp = PlayerStatLYK.Instance.CurrentExp;
        databaseData.lastHP = PlayerStatLYK.Instance.CurrentHealth;
        databaseData.lastHungryValue = PlayerStatLYK.Instance.Hungry;
        databaseData.lastThirstyValue = PlayerStatLYK.Instance.Thirsty;

        string json = JsonUtility.ToJson(databaseData);
        databaseReference.Child(_userId).SetRawJsonValueAsync(json);
    }

    /// <summary> DB���� ������ ��ü �ҷ����� </summary>
    public void LoadAllData(string _userId)
    {
        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.GetReference(_userId);
        //������ ��ȸ
        databaseReference.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)//������ �ε� ����
            {
                Debug.Log("Failed load data!!");
            }
            else if (task.IsCompleted)//������ �ε� ����
            {
                //������ ���� : ��ȸ�� ������(���ڵ�)�� �����ϴ� ����
                DataSnapshot snapshot = task.Result;

                //������ �Ǽ� ���
                Debug.Log($"������ ���ڵ� ���� : {snapshot.ChildrenCount}");

                IDictionary test = (IDictionary)snapshot.Value;
                Debug.Log("lastHP : " + test["lastHP"] + " lastHungryValue : " + test["lastHungryValue"] + " lastThirstyValue : " + test["lastThirstyValue"] + " userLevel : " + test["userLevel"]);

                databaseData.lastHP = (int.Parse)(test["lastHP"].ToString());
                databaseData.lastHungryValue = (int.Parse)(test["lastHungryValue"].ToString());
                databaseData.lastThirstyValue = (int.Parse)(test["lastThirstyValue"].ToString());
                databaseData.userLevel = (int.Parse)(test["userLevel"].ToString());
            }
        });
    }

    /// <summary> Ű ������ �˻� </summary>
    public void SelectData(string _userId, string _key)
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(_userId);
        Query test = reference.OrderByChild("�ʿ��� key ex)userLevel ��").EqualTo(_key);
    }

    private void OnApplicationQuit()
    {
        if (userId == null) return;
        SaveDatabaseData(userId);
    }
}