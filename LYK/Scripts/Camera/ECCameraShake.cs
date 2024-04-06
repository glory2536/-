using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class ECCameraShake : Singleton<ECCameraShake>
{
    public Transform camTransform;

    public float shakeDuration = 0f;
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;

    public bool isShake = true;
    Vector3 originalPos;

    void Awake()
    {
        if (camTransform == null)
        {
            camTransform = GetComponent(typeof(Transform)) as Transform;
        }
    }

    void OnEnable()
    {
        originalPos = camTransform.localPosition;
    }

    /// <summary> 카메라 흔들림 </summary>
    public void  CameraShaking(float _shakeDuration, float _shakeAmount)
    {
        if (!isShake) return;
        StartCoroutine(CameraShakingCo(_shakeDuration, _shakeAmount));
    }

    IEnumerator CameraShakingCo(float _shakeDuration, float _shakeAmount)
    {
        shakeDuration = _shakeDuration;
        shakeAmount = _shakeAmount;

        while (true)
        {
            if (shakeDuration > 0)
            {
                camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
                shakeDuration -= Time.deltaTime * decreaseFactor;
            }

            if (shakeAmount > 0)
            {
                shakeAmount -= Time.deltaTime * decreaseFactor;
            }
            else
            {
                shakeAmount = 0f;
                shakeDuration = 0f;
                camTransform.localPosition = originalPos;
                break;
            }
            yield return null;
        }
    }
}