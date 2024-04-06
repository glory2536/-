using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//¹«±â Àåºñ °³ÀÎ Á¤º¸
public class WeaponInfo : MonoBehaviour
{
    public ParticleSystem muzzleFlashEffect;//ÃÑ±¸ È­¿° È¿°ú
    public GameObject bulletPrefab;//ÃÑ¾Ë
    public Transform muzzlePosition;//ÃÑ±¸À§Ä¡

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