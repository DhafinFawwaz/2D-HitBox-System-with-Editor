using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class RectangleBox : HitBox
{
    public Vector2 Size = new Vector2(2,2);
    public override Collider2D[] Cast(LayerMask hitableLayer)
    {
        return Physics2D.OverlapBoxAll(transform.position, Size, 0, hitableLayer);
    }



#if UNITY_EDITOR
    public override void DrawGizmos(Color color, bool isHit)
    {
        Handles.color = color;
        Rect rect = new Rect(transform.position - (Vector3)Size/2, Size);
        color.a = 0.04f;
        if(isHit) color.a = 0.1f;
        Handles.DrawSolidRectangleWithOutline(rect, color, Handles.color);
    }
#endif
}
