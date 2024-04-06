using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string soundName;
    public AudioClip soundClip;
}


public class SoundManager : Singleton<SoundManager>
{
    public AudioSource[] audioSourceEffects;//이펙트 오디오소스
    public AudioSource audioSourceBgm;//배경사운드 오디오소스

    public Sound[] effectSounds;//사용할 이펙트 사운드
    public Sound[] bgmSounds;//사용할 배경 사운드

    public string[] playSoundName;

    private void Start()
    {
        audioSourceBgm.clip = bgmSounds[0].soundClip;
        audioSourceBgm.Play();
    }

    /// <summary> 이펙트 전체 사운드 변경 </summary>
    public void SetEffectSoundsVolume(float _value)
    {
        foreach(var effectAudioSource in audioSourceEffects)
        {
            effectAudioSource.volume = _value;
        }
    }


    public void PlaySE(string _name)
    {
        for(int i=0; i<effectSounds.Length; i++)
        {
            if (_name == effectSounds[i].soundName)
            {
                for(int j =0; j< audioSourceEffects.Length; j++)
                {
                    if (!audioSourceEffects[j].isPlaying)
                    {
                        audioSourceEffects[j].clip = effectSounds[i].soundClip;
                        audioSourceEffects[j].Play();
                        return;
                    }
                }
                Debug.Log("모든 가용 AudioSource가 사용중입니다");
            }
        }
        Debug.Log(_name + "=> 사운드가 SounManger에 등록되지 않았습니다.");
    }

    /// <summary> 원하는 사운드 끄기 </summary>
    public void StopSE(string _name)
    {
        for (int i = 0; i < audioSourceEffects.Length; i++)
        {
            if (playSoundName[i] == _name)
            {
                audioSourceEffects[i].Stop();
                return;
            }
        }
        Debug.Log("재생 중인" + _name + "사운드가 없습니다.");
    }

    /// <summary> 모든 사운드 소리끄기 </summary>
    public void StopAllSE()
    {
        for(int i = 0; i< audioSourceEffects.Length; i++)
        {
            audioSourceEffects[i].Stop();
        }
        audioSourceBgm.Stop();
    }
}
/* 필요하면 가져오기

*/