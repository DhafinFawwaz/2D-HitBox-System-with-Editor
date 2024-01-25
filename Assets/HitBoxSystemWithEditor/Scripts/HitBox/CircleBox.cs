using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif
public class CircleBox : HitBox
{
    public float Radius = 2;
    public override Collider2D[] Cast(LayerMask hitableLayer)
    {
        return Physics2D.OverlapCircleAll(transform.position, Radius, hitableLayer);
    }



#if UNITY_EDITOR
    public override void DrawGizmos(Color color, bool isHit)
    {
        float radius = Radius;

        Handles.color = color;
        Handles.DrawWireDisc(transform.position, Vector3.forward, radius);
        
        color.a = 0.04f;
        if(isHit) color.a = 0.1f;
        
        Handles.color = color;

        Handles.DrawSolidDisc(transform.position, Vector3.forward, radius);
    }
#endif
}
