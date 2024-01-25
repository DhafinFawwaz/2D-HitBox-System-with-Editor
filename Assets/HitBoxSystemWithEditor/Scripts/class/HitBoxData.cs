using UnityEngine;
public class HitBoxData
{
    public HitBox HitBoxCaster;
    public Collider2D HitColliderReceiver; // attachedRigidbody never null
    public HitBoxData(HitBox hb, Collider2D col)
    {
        HitBoxCaster = hb;
        HitColliderReceiver = col;
    }
}