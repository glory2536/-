using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameSetting : MonoBehaviour
{

    [SerializeField] private Button cameraShakeBt;//ī�޶���鸲 ��ư
    [SerializeField] private Slider backGroundMusic;//�����_BGM
    [SerializeField] private Slider soundEffect;//ȿ����
    public TMP_Text playerIDText;//�÷��̾�ID

    private void Start()
    {
        //ī�޶� ��鸲 ����
        cameraShakeBt.onClick.AddListener(() =>
        {
            if (cameraShakeBt.transform.GetChild(0).gameObject.activeSelf)
            {
                //OFF -> ON
                cameraShakeBt.transform.GetChild(1).gameObject.SetActive(true);
                cameraShakeBt.transform.GetChild(0).gameObject.SetActive(false);
                //ī�޶���ũ
                ECCameraShake.Instance.isShake = true;
            }
            else
            {
                //ON -> OFF
                cameraShakeBt.transform.GetChild(0).gameObject.SetActive(true);
                cameraShakeBt.transform.GetChild(1).gameObject.SetActive(false);
                //ī�޶���ũ
                ECCameraShake.Instance.isShake = false;
            }
        }

        );
        //������� �Ҹ� ����
        backGroundMusic.onValueChanged.AddListener((value) => SoundManager.Instance.audioSourceBgm.volume = value);
        //����Ʈ �Ҹ� ����
        soundEffect.onValueChanged.AddListener((value) => SoundManager.Instance.SetEffectSoundsVolume(value));
    }

}
