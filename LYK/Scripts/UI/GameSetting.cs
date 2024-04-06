using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameSetting : MonoBehaviour
{

    [SerializeField] private Button cameraShakeBt;//카메라흔들림 버튼
    [SerializeField] private Slider backGroundMusic;//배경음_BGM
    [SerializeField] private Slider soundEffect;//효과음
    public TMP_Text playerIDText;//플레이어ID

    private void Start()
    {
        //카메라 흔들림 설정
        cameraShakeBt.onClick.AddListener(() =>
        {
            if (cameraShakeBt.transform.GetChild(0).gameObject.activeSelf)
            {
                //OFF -> ON
                cameraShakeBt.transform.GetChild(1).gameObject.SetActive(true);
                cameraShakeBt.transform.GetChild(0).gameObject.SetActive(false);
                //카메라쉐이크
                ECCameraShake.Instance.isShake = true;
            }
            else
            {
                //ON -> OFF
                cameraShakeBt.transform.GetChild(0).gameObject.SetActive(true);
                cameraShakeBt.transform.GetChild(1).gameObject.SetActive(false);
                //카메라쉐이크
                ECCameraShake.Instance.isShake = false;
            }
        }

        );
        //배경음악 소리 설정
        backGroundMusic.onValueChanged.AddListener((value) => SoundManager.Instance.audioSourceBgm.volume = value);
        //이펙트 소리 설정
        soundEffect.onValueChanged.AddListener((value) => SoundManager.Instance.SetEffectSoundsVolume(value));
    }

}
