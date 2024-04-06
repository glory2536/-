using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSC : MonoBehaviour
{
    public Transform target;

    public float projectileSpeed;
    public float projectileSpeedMultiplier;

    float timer = 0;
    public float deathtimer = 3;

    [SerializeField] private LayerMask targetLayer;

    private void FixedUpdate()
    {
        if (target == null) return;

        timer += Time.deltaTime;

        if (timer >= deathtimer|| target.GetComponent<LivingEntity>().Dead == true)
        {
            Destroy(gameObject);
        }
        else
        {
            Vector3 test = new Vector3(target.position.x, this.transform.position.y, target.position.z);
            transform.LookAt(test);
            transform.Translate(Vector3.forward * 1f);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Destroy(gameObject);
        }

    }
}
