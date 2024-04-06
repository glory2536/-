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
    const string stat_URL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=1186688167";//����_���۽�Ʈ

    //���� ������
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

        for (int k = 1; k < line.Length; k++)//������ �����ϴ� ĭ�̶� 1���ͽ���
        {
            string[] row = line[k].Split('\t');

            playerStat[k - 1] = new Stat(int.Parse(row[0]), row[1], float.Parse(row[2]));
        }
    }

    //��� ������Ƽ
    public Stat Damage { get { return playerStat[0]; } }//00_���ݷ�
    public Stat CriticalChance { get { return playerStat[1]; } }//01_ġ��ŸȮ��
    public Stat CriticalHit { get { return playerStat[2]; } }//02_ġ��Ÿ������
    public Stat AttackSpeed { get { return playerStat[3]; } }//03_���ݼӵ�
    public Stat AttackRange { get { return playerStat[4]; } }//04_���ݹ���
    public Stat AttackAccuracy { get { return playerStat[5]; } }//05_���߷�
    //06_ü��
    public Stat Health
    {
        get
        {
            healthSlider.value = CurrentHealth / playerStat[6].GetValue;
            healthText.text = string.Format("{0}/{1}", (int)CurrentHealth, (int)playerStat[6].GetValue);
            return playerStat[6];
        }
    }
    public Stat Defense { get { return playerStat[7]; } }//07_����
    public Stat Evasion { get { return playerStat[8]; } }//08_ȸ����
    public Stat Speed { get { return playerStat[9]; } }//09_�̵��ӵ�
    public Stat LumberingDamage { get { return playerStat[10]; } }//10_������ݷ�
    public Stat MiningDamage { get { return playerStat[11]; } }//11_ä�����ݷ�
    public Stat WeaponType { get { return playerStat[12]; } }//12_����Ÿ��
    public Stat MaxLevel { get { return playerStat[13]; } }//13_�ִ뷹��
    //14_���緹��
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
    //15_�������ġ
    public float CurrentExp
    {
        get
        {
            return playerStat[15].GetValue;
        }
        set
        {
            if (value >= UseLevelUpExp)//������ó��
            {
                if (CurrentLevel + 1 <= MaxLevel.GetValue)//�ִ뷹�� ��
                {
                    Debug.Log("PlayerLevelUp!!");
                    playerStat[15].baseValue = value - UseLevelUpExp;
                    CurrentLevel += 1;

                    //������ �̺�Ʈ
                    Instantiate(levelUpParticle, this.gameObject.transform);
                }
                else
                {
                    Debug.Log("PlayerMaxLevel!!");
                    CurrentLevel = (int)MaxLevel.GetValue;//�ִ뷹��
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

    //16_���� ü��
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
                //����ó��
                playerStat[16].baseValue = 0;

                //���� �̺�Ʈ
                playerDeadEvent();
            }
            else if (value >= Health.GetValue)
            {
                //�ִ�ü�³����� ü�� = �ִ�ü�� or ����
                playerStat[16].baseValue = Health.GetValue;
            }
            else
            {
                //������ó��
                playerStat[16].baseValue = value;
            }
            //Json
            JsonMG.instance.jsonData.playerHp = CurrentHealth;


            //ü��UIó
            healthSlider.value = CurrentHealth / Health.GetValue;
            healthText.text = string.Format("{0}/{1}", (int)CurrentHealth, (int)Health.GetValue);
        }
    }
    public Stat SearchRange { get { return playerStat[17]; } }//17_�÷��̾�_��Ī����(�ֺ� ���ͷ��� ������Ʈ ��Ī)
    public Stat InteractionRange { get { return playerStat[18]; } }//18_���ͷ��� ����
    //19_�����
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
                //����� ��ġ 0�̸� ��ü ü���� 10%�� ����
                CurrentHealth -= (Health.GetValue * 0.1f);
            }
            else
            {
                playerStat[19].baseValue = value;
            }

            JsonMG.instance.jsonData.playerHungry = Hungry;
            hungry.fillAmount = (playerStat[19].baseValue * 0.01f);//UIó��
        }

    }
    //20_�񸶸�
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
                //�񸶸� ��ġ 0�̸� ��ü ü���� 10%�� ����
                CurrentHealth -= (Health.GetValue * 0.1f);
            }
            else
            {
                playerStat[20].baseValue = value;
            }

            JsonMG.instance.jsonData.playerThirsty = Thirsty;
            thirsty.fillAmount = (playerStat[20].baseValue * 0.01f);//UIó��
        }
    }

    //21_����Ÿ��
    public Stat WeaponAttackType { get { return playerStat[21]; } }


    //�������� �ʿ��� ����ġ
    public int UseLevelUpExp
    {
        get
        {
            //UseLevelUpExp = (int)CurrentLevel * 100;
            return (int)CurrentLevel * 50;
        }
    }
}