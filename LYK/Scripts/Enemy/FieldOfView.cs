using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask, obstacleMask;

    //Target mask에 rayhit된 transform을 보관하는 리스트
    public List<Transform> visibleTargets = new List<Transform>();

    private void Start()
    {
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(delay);

        while (true)
        {
            yield return waitForSeconds;
            FindVisibleTargets();
        }

    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();

        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for(int i =0; i<targetsInRadius.Length; i++)
        {
            Transform target = targetsInRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            //플레이어와 forward와 target이 이루는 각이 설장한 각도 내라면
            if(Vector3.Angle(transform.forward,dirToTarget) < viewAngle * 0.5)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

                //타겟으로 가는 레이캐스트에 obstarcleMask가 걸리지 않으면 visibleTargets에 추가
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    public Vector3 DirFromAngle(float angleDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Cos((-angleDegrees + 90) * Mathf.Deg2Rad), 0, Mathf.Sin((-angleDegrees + 90) * Mathf.Deg2Rad));
    }

}
