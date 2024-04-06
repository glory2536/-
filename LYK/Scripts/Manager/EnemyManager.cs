using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class EnemyData
{
    public int enemyIndex;
    public string enemyName;
    public int maxHealth;
    public int defense;
    public int damage;
    public float speed;
    public int exp;
    public int dropItemIndex;

    public float attackRadius;
    public float attackDistance;
    public float attackAngle;
    public float attackSpeed;
    //프리팹경로

    public EnemyData(int _enemyIndex, string _enemyName, int _maxHealth, int _defense, int _damage, float _speed, int _exp, int _dropItemIndex, float _attackRadius, float _attackDistance, float _attackAngle, float _attackSpeed)
    {
        enemyIndex = _enemyIndex;
        enemyName = _enemyName;
        maxHealth = _maxHealth;
        defense = _defense;
        damage = _damage;
        speed = _speed;
        exp = _exp;

        dropItemIndex = _dropItemIndex;
        attackRadius = _attackRadius;
        attackDistance = _attackDistance;
        attackAngle = _attackAngle;
        attackSpeed = _attackSpeed;
    }
}

//[ExecuteInEditMode]
public class EnemyManager : MonoBehaviour
{
    [SerializeField] EnemyData[] enemyData;

    GameObject enemyObject;//리스트로 바꿔주기

    [SerializeField] private Transform[] EnemySpawnPoint;

    private void Awake()
    {
        StartCoroutine(GetEnemyDataFromGoogleSheets());
    }

    IEnumerator GetEnemyDataFromGoogleSheets()
    {
        const string enemyDataURL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=892522095";

        UnityWebRequest www = UnityWebRequest.Get(enemyDataURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        string[] line = data.Substring(0, data.Length).Split('\n');
        enemyData = new EnemyData[line.Length - 1];

        for (int k = 0; k < enemyData.Length; k++)
        {
            string[] row = line[k + 1].Split('\t');
            enemyData[k] = new EnemyData(int.Parse(row[0]), row[1], int.Parse(row[2]), int.Parse(row[3]), int.Parse(row[4]), float.Parse(row[5]), int.Parse(row[6]), int.Parse(row[7]), float.Parse(row[8]), float.Parse(row[9]), float.Parse(row[10]), float.Parse(row[11]));
        }

        //적생성
        EnemySpawn(EnemySpawnPoint[0].position, 0);
        EnemySpawn(EnemySpawnPoint[1].position, 1);
        EnemySpawn(EnemySpawnPoint[2].position, 4);
        EnemySpawn(EnemySpawnPoint[3].position, 3);
        EnemySpawn(EnemySpawnPoint[4].position, 2);
    }

    /// <summary> 적 스폰 </summary>
    public void EnemySpawn(Vector3 _EnemyPosition,int _EnemyIndex)
    {
        for (int i=0; i < enemyData.Length; i++)
        {
            if(enemyData[i].enemyIndex == _EnemyIndex)
            {               
                Addressables.InstantiateAsync(enemyData[i].enemyName, _EnemyPosition, Quaternion.identity).Completed +=
                    (AsyncOperationHandle<GameObject> obj) =>
                    {
                        if (obj.Result.TryGetComponent<Zombie>(out Zombie _enemy))
                        {
                            enemyObject = obj.Result;
                            _enemy.SetupStat(enemyData[i]);
                        }
                    };

                break;
            }
        }
    }

    private void ReleaseDestroy()
    {
        Debug.Log("ReleaseDestroy");

        Destroy(enemyObject);
        Addressables.Release(enemyObject);
    }

    private void OnDestroy()
    {

        Debug.Log($"OnDestroy");
        Destroy(enemyObject);
        Addressables.Release(enemyObject);
    }
}