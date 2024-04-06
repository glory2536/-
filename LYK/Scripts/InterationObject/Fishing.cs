using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Fishing : InteractionObject
{
    [SerializeField] FishingSystemSC fishingSystem;

    public override void InteractionEvent()
    {
        fishingSystem.gameObject.SetActive(true);
    }

}
