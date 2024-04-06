using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSC : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float followSpeed;

    private void Start()
    {
        if (!targetObject)
        {
            targetObject = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void Update()
    {
        //transform.position = targetObject.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetObject.position + offset, followSpeed * Time.deltaTime);
    }

}
