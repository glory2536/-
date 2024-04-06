using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldEvent : InteractionObject
{
    private void Start()
    {
        obType = InteractionObType.Field;
    }

    public override void InteractionEvent()
    {
        if (Dead) return;

        Dead = true;
        if (TryGetComponent(out Animator anim))
        {
            anim.enabled = true;
        }
        if (TryGetComponent(out BoxCollider collider))
        {
            collider.enabled = false;
        }
    }
}
