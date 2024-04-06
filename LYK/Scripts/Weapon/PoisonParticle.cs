using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonParticle : MonoBehaviour
{
    public Transform target;

    public float projectileSpeed;
    public float projectileSpeedMultiplier;

    float timer = 0;
    public float deathtimer = 3;

    [SerializeField] private LayerMask targetLayer;

    bool isCheck = false;
    Vector3 targetPosition;
    float damage;

    public void SetUp(float _damage)
    {
        targetPosition = target.position;
        isCheck = true;
        damage = _damage;
    }

    private void FixedUpdate()
    {
        if (target == null) return;
        if (!isCheck) return;

        timer += Time.deltaTime;

        if (timer >= deathtimer || target.GetComponent<LivingEntity>().Dead == true)
        {
            Destroy(gameObject);
        }
        else
        {
            Vector3 test = new Vector3(targetPosition.x, 0.1f, targetPosition.z);
            transform.LookAt(test);
            projectileSpeed = projectileSpeed + projectileSpeedMultiplier;
            transform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") || other.gameObject.layer == LayerMask.NameToLayer("Plane") || other.gameObject.layer == LayerMask.NameToLayer("Interaction"))
        {
            if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                target.GetComponent<LivingEntity>().OnDamage(damage);
            }
            
            Destroy(gameObject);
        }

    }
}
