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
    //스텟
    public int userLevel;
    public float playerExp;
    public float lastHP;
    public float lastHungryValue;
    public float lastThirstyValue;

    /*
    //계정정보
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
        // 파이어베이스의 메인 참조 얻기
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }


    /// <summary> 데이터베이스 데이터 초기화 </summary>
    public void InitDatabaseData(string _userId)
    {
        databaseData = new();
        string json = JsonUtility.ToJson(databaseData);
        databaseReference.Child(_userId).SetRawJsonValueAsync(json);
        LoadAllData(_userId);
    }


    /// <summary> 계정에 최초 접속하는지 확인 </summary>
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
                Debug.Log("Snapshot_Exists => 이미 데이터존재");
                LoadAllData(_userId);
                userId = _userId;
            }
            else
            {
                Debug.Log("Snapshot_not_Exists => 최초접속");
                InitDatabaseData(_userId);
                userId = _userId;
            }
        }
    });
    }

    /// <summary> 게임데이터 DB에 전체 저장, 종료시 실행 </summary>
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

    /// <summary> DB에서 데이터 전체 불러오기 </summary>
    public void LoadAllData(string _userId)
    {
        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.GetReference(_userId);
        //데이터 조회
        databaseReference.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)//데이터 로딩 실패
            {
                Debug.Log("Failed load data!!");
            }
            else if (task.IsCompleted)//데이터 로딩 성공
            {
                //스냅샷 생성 : 조회한 데이터(레코드)를 저장하는 단위
                DataSnapshot snapshot = task.Result;

                //데이터 건수 출력
                Debug.Log($"데이터 레코드 갯수 : {snapshot.ChildrenCount}");

                IDictionary test = (IDictionary)snapshot.Value;
                Debug.Log("lastHP : " + test["lastHP"] + " lastHungryValue : " + test["lastHungryValue"] + " lastThirstyValue : " + test["lastThirstyValue"] + " userLevel : " + test["userLevel"]);

                databaseData.lastHP = (int.Parse)(test["lastHP"].ToString());
                databaseData.lastHungryValue = (int.Parse)(test["lastHungryValue"].ToString());
                databaseData.lastThirstyValue = (int.Parse)(test["lastThirstyValue"].ToString());
                databaseData.userLevel = (int.Parse)(test["userLevel"].ToString());
            }
        });
    }

    /// <summary> 키 값으로 검색 </summary>
    public void SelectData(string _userId, string _key)
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(_userId);
        Query test = reference.OrderByChild("필요한 key ex)userLevel 등").EqualTo(_key);
    }

    private void OnApplicationQuit()
    {
        if (userId == null) return;
        SaveDatabaseData(userId);
    }
}