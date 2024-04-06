using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���� ��� ���� ����
public class WeaponInfo : MonoBehaviour
{
    public ParticleSystem muzzleFlashEffect;//�ѱ� ȭ�� ȿ��
    public GameObject bulletPrefab;//�Ѿ�
    public Transform muzzlePosition;//�ѱ���ġ

    public void Shot(Transform Target)
    {
        if (muzzleFlashEffect != null)
        {

            muzzleFlashEffect.Play();
        }

        if (bulletPrefab != null && muzzlePosition != null)
        {

            GameObject bulletObject = Instantiate(bulletPrefab, muzzlePosition.position, Quaternion.Euler(new Vector3(0, -90, 0)));

            if (bulletObject.TryGetComponent<BulletSC>(out BulletSC bulletSC))
            {
                bulletSC.target = Target;
                SoundManager.Instance.PlaySE("GunShotSound");
            }
        }
    }

    public void MuzzleFlashStop()
    {
        if (muzzleFlashEffect != null)
        {
            Debug.Log("MuzzleStop");
            muzzleFlashEffect.Stop();
        }
    }
}