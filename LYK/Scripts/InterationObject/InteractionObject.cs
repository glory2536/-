using Glory.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractionObType { Tree, Mining, PickUp, Fishing, Building, Field }
// DestructibleObject,
public abstract class InteractionObject : MonoBehaviour
{
    public InteractionObType obType;
    public bool Dead { get; protected set; }

    public abstract void InteractionEvent();

}