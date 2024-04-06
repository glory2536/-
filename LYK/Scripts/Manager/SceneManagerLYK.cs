using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Scene
{
    LobbyScene,
    BaseCampScene,
    LoadingScene,
    Map_GasStation,
    Map_Neighborhood
}

public class SceneManagerLYK : MonoBehaviour
{
    public static string nextScene;

    [SerializeField] Image progressBar;
    [SerializeField] TMP_Text gameTipTMP;

    List<string> gameTip = new();

    private void Start()
    {
        StartCoroutine(GetGameTipString());
    }

    /// <summary> 씬 로드 </summary>
    public static void LoadScene(Scene _nextScene)
    {
        JsonMG.instance.SaveDataToJson();//=>씬이동시 Json저장

        nextScene = System.Enum.GetName(typeof(Scene), _nextScene);
        SceneManager.LoadScene("LoadingScene");
    }

    IEnumerator LoadSceneCo()
    {
        yield return null;
        int randomNumber = Random.Range(0, gameTip.Count);

        if(gameTipTMP != null) gameTipTMP.text = string.Format("GameTip : {0}", gameTip[randomNumber]);

        AsyncOperation asyncOper = SceneManager.LoadSceneAsync(nextScene);
        asyncOper.allowSceneActivation = false;//true면 로딩다되면 바로 넘어감 false면 95%까지만 불러옴

        float timer = 0.0f;
        while (!asyncOper.isDone)
        {
            yield return null;
            
            if(asyncOper.progress < 0.9f)
            {
                progressBar.fillAmount = asyncOper.progress;
            }
            else
            {
                timer += Time.deltaTime;
                progressBar.fillAmount = Mathf.Lerp(0.9f, 1f, timer);
                if (progressBar.fillAmount >= 1f)
                {
                    asyncOper.allowSceneActivation = true;
                    
                    yield break;
                }
            }
        }
    }

    /// <summary> 게임 툴팁 구글시트에서 불러오기 </summary>
    IEnumerator GetGameTipString()
    {
        const string tipURL = "https://docs.google.com/spreadsheets/d/10zqJB85sNmSy00893CEFLeVYAwLodsKC7B6R4_0UNQI/export?format=tsv&gid=788496907";
        UnityWebRequest www = UnityWebRequest.Get(tipURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        string[] line = data.Substring(0, data.Length).Split('\n');
        
        for (int i = 1; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');
            gameTip.Add(row[0]);
        }

        StartCoroutine(LoadSceneCo());
    }
}