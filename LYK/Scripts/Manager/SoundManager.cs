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
    public AudioSource[] audioSourceEffects;//����Ʈ ������ҽ�
    public AudioSource audioSourceBgm;//������ ������ҽ�

    public Sound[] effectSounds;//����� ����Ʈ ����
    public Sound[] bgmSounds;//����� ��� ����

    public string[] playSoundName;

    private void Start()
    {
        audioSourceBgm.clip = bgmSounds[0].soundClip;
        audioSourceBgm.Play();
    }

    /// <summary> ����Ʈ ��ü ���� ���� </summary>
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
                Debug.Log("��� ���� AudioSource�� ������Դϴ�");
            }
        }
        Debug.Log(_name + "=> ���尡 SounManger�� ��ϵ��� �ʾҽ��ϴ�.");
    }

    /// <summary> ���ϴ� ���� ���� </summary>
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
        Debug.Log("��� ����" + _name + "���尡 �����ϴ�.");
    }

    /// <summary> ��� ���� �Ҹ����� </summary>
    public void StopAllSE()
    {
        for(int i = 0; i< audioSourceEffects.Length; i++)
        {
            audioSourceEffects[i].Stop();
        }
        audioSourceBgm.Stop();
    }
}
/* �ʿ��ϸ� ��������

*/