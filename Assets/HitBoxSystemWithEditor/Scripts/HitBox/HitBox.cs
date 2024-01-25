using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public bool CanBeTriggered = true;
    public float CustomValue = 0;
    void OnEnable() => CanBeTriggered = true;
    void OnDisable() => CanBeTriggered = false;
    public virtual Collider2D[] Cast(LayerMask hitableLayer){ return null;}

#if UNITY_EDITOR
    public virtual void DrawGizmos(Color color, bool isHit = false){}
#endif
}
