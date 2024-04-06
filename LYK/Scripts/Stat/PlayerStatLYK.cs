using Glory.InventorySystem;
using GooglePlayGames.BasicApi;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[ExecuteInEditMode]
public class PlayerStatLYK : Singleton<PlayerStatLYK>
{
    const string stat_URL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=1186688167";//스탯_구글시트

    //스탯 원본값
    [SerializeField] Stat[] playerStat;
    public Stat GetPlayerStat(int statIndex) => playerStat[statIndex];

    [Header("PlayerStatUI")]
    [SerializeField] TMP_Text levelText;
    [SerializeField] Slider healthSlider;
    [SerializeField] TMP_Text healthText;
    [SerializeField] Slider expSlider;
    [SerializeField] TMP_Text expText;
    [SerializeField] Image hungry;
    [SerializeField] Image thirsty;

    [SerializeField] ParticleSystem levelUpParticle;

    public Action playerDeadEvent;

    private void Awake()
    {
        StartCoroutine(GetFile());
    }

    IEnumerator GetFile()
    {
        UnityWebRequest www = UnityWebRequest.Get(stat_URL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;

        string[] line = data.Substring(0, data.Length).Split('\n');

        playerStat = new Stat[line.Length - 1];

        for (int k = 1; k < line.Length; k++)//맨위는 설명하는 칸이라 1부터시작
        {
            string[] row = line[k].Split('\t');

            playerStat[k - 1] = new Stat(int.Parse(row[0]), row[1], float.Parse(row[2]));
        }
    }

    //멤버 프로퍼티
    public Stat Damage { get { return playerStat[0]; } }//00_공격력
    public Stat CriticalChance { get { return playerStat[1]; } }//01_치명타확률
    public Stat CriticalHit { get { return playerStat[2]; } }//02_치명타데미지
    public Stat AttackSpeed { get { return playerStat[3]; } }//03_공격속도
    public Stat AttackRange { get { return playerStat[4]; } }//04_공격범위
    public Stat AttackAccuracy { get { return playerStat[5]; } }//05_명중률
    //06_체력
    public Stat Health
    {
        get
        {
            healthSlider.value = CurrentHealth / playerStat[6].GetValue;
            healthText.text = string.Format("{0}/{1}", (int)CurrentHealth, (int)playerStat[6].GetValue);
            return playerStat[6];
        }
    }
    public Stat Defense { get { return playerStat[7]; } }//07_방어력
    public Stat Evasion { get { return playerStat[8]; } }//08_회피율
    public Stat Speed { get { return playerStat[9]; } }//09_이동속도
    public Stat LumberingDamage { get { return playerStat[10]; } }//10_벌목공격력
    public Stat MiningDamage { get { return playerStat[11]; } }//11_채굴공격력
    public Stat WeaponType { get { return playerStat[12]; } }//12_무기타입
    public Stat MaxLevel { get { return playerStat[13]; } }//13_최대레벨
    //14_현재레벨
    public int CurrentLevel
    {
        get
        {
            return (int)playerStat[14].GetValue;
        }
        set
        {
            playerStat[14].baseValue = value;
            JsonMG.instance.jsonData.playerLevel = CurrentLevel;
            levelText.text = playerStat[14].GetValue.ToString();
        }
    }
    //15_현재경험치
    public float CurrentExp
    {
        get
        {
            return playerStat[15].GetValue;
        }
        set
        {
            if (value >= UseLevelUpExp)//레벨업처리
            {
                if (CurrentLevel + 1 <= MaxLevel.GetValue)//최대레벨 비교
                {
                    Debug.Log("PlayerLevelUp!!");
                    playerStat[15].baseValue = value - UseLevelUpExp;
                    CurrentLevel += 1;

                    //레벨업 이벤트
                    Instantiate(levelUpParticle, this.gameObject.transform);
                }
                else
                {
                    Debug.Log("PlayerMaxLevel!!");
                    CurrentLevel = (int)MaxLevel.GetValue;//최대레벨
                    playerStat[15].baseValue = UseLevelUpExp;
                }
            }
            else
            {

                playerStat[15].baseValue = value;
            }

            JsonMG.instance.jsonData.playerExp = CurrentExp;
            expSlider.value = CurrentExp / UseLevelUpExp;
            expText.text = ((int)(CurrentExp / UseLevelUpExp * 100)).ToString() + "%";
        }
    }

    //16_현재 체력
    public float CurrentHealth
    {
        get
        {
            return playerStat[16].GetValue;
        }
        set
        {
            if (value <= 0)
            {
                //죽음처리
                playerStat[16].baseValue = 0;

                //죽음 이벤트
                playerDeadEvent();
            }
            else if (value >= Health.GetValue)
            {
                //최대체력넘으면 체력 = 최대체력 or 리턴
                playerStat[16].baseValue = Health.GetValue;
            }
            else
            {
                //데미지처리
                playerStat[16].baseValue = value;
            }
            //Json
            JsonMG.instance.jsonData.playerHp = CurrentHealth;


            //체력UI처
            healthSlider.value = CurrentHealth / Health.GetValue;
            healthText.text = string.Format("{0}/{1}", (int)CurrentHealth, (int)Health.GetValue);
        }
    }
    public Stat SearchRange { get { return playerStat[17]; } }//17_플레이어_서칭범위(주변 인터렉션 오브젝트 서칭)
    public Stat InteractionRange { get { return playerStat[18]; } }//18_인터렉션 범위
    //19_배고픔
    public float Hungry
    {
        get
        {
            return playerStat[19].GetValue;
        }
        set
        {
            if (value < 0)
            {
                playerStat[19].baseValue = 0;
                //배고픔 수치 0이면 전체 체력의 10%씩 감소
                CurrentHealth -= (Health.GetValue * 0.1f);
            }
            else
            {
                playerStat[19].baseValue = value;
            }

            JsonMG.instance.jsonData.playerHungry = Hungry;
            hungry.fillAmount = (playerStat[19].baseValue * 0.01f);//UI처리
        }

    }
    //20_목마름
    public float Thirsty
    {
        get
        {
            return playerStat[20].GetValue;
        }
        set
        {
            if (value < 0)
            {
                playerStat[20].baseValue = 0;
                //목마름 수치 0이면 전체 체력의 10%씩 감소
                CurrentHealth -= (Health.GetValue * 0.1f);
            }
            else
            {
                playerStat[20].baseValue = value;
            }

            JsonMG.instance.jsonData.playerThirsty = Thirsty;
            thirsty.fillAmount = (playerStat[20].baseValue * 0.01f);//UI처리
        }
    }

    //21_공격타입
    public Stat WeaponAttackType { get { return playerStat[21]; } }


    //레벨업에 필요한 경험치
    public int UseLevelUpExp
    {
        get
        {
            //UseLevelUpExp = (int)CurrentLevel * 100;
            return (int)CurrentLevel * 50;
        }
    }
}