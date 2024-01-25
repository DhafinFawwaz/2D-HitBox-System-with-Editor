using System;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class HitBoxCaster : MonoBehaviour
{
    [Tooltip("Can be used to ignore self rigidbody getting hit by its own hitbox")]
    [SerializeField] Rigidbody2D _rigidbodyToIgnore;
    [Tooltip("LayerMask to cast hitbox on")]
    public LayerMask HitableLayer = ~0;
    public HitBox[] HitBoxes;
    public Action<HitBoxData> OnHitCast;
    private HashSet<Rigidbody2D> _castedRigidbodies;
    void OnDisable()
    {
        _castedRigidbodies.Clear();
        _castedRigidbodies.Add(_rigidbodyToIgnore);
    }

    void FixedUpdate()
    {
        CastHitBox(HitBoxes);
    }
    void CastHitBox(HitBox[] hitBoxes)
    {
        foreach(HitBox hb in hitBoxes)
        {
            if(hb.CanBeTriggered)
            {
                Collider2D[] cols = hb.Cast(HitableLayer);
                if(cols.Length > 0)
                {
                    foreach(Collider2D col in cols)
                    {
                        Rigidbody2D rb = col.attachedRigidbody;
                        if(rb != null && _castedRigidbodies.Add(rb))
                        {
                            OnCasted(hb, col);
                        }
                    }
                }
            }
        }
    }
    void OnCasted(HitBox hbCaster, Collider2D col)
    {
        HitBoxReceiver hbr = col.attachedRigidbody.GetComponent<HitBoxReceiver>();
        if(hbr != null)
        {
            HitBoxData hbData = new HitBoxData(hbCaster, col);
            OnHitCast?.Invoke(hbData);
            hbr.OnHitReceive?.Invoke(hbData);
        }
    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        foreach(HitBox hb in HitBoxes)
        {
            hb.DrawGizmos(Color.red, false);
            if(hb.CanBeTriggered)
            {
                Collider2D[] cols = hb.Cast(HitableLayer);
                if(cols.Length > 0)
                {
                    hb.DrawGizmos(Color.red, true);
                    foreach(Collider2D col in cols)
                    {
                        Rigidbody2D rb = col.attachedRigidbody;
                        if(rb != null && _rigidbodyToIgnore != rb)
                        {
                            DrawGizmosOnReceiver(col);
                        }
                    }
                }
            }
        }
    }
    void DrawGizmosOnReceiver(Collider2D col)
    {
        if(col is CircleCollider2D)
        {
            CircleCollider2D circleCol = (CircleCollider2D)col;
            DrawCircleOnReceiver(circleCol.transform.position, circleCol.radius, Color.green);
            return;
        }

        if(col is BoxCollider2D)
        {
            BoxCollider2D boxCol = (BoxCollider2D)col;
            DrawBoxOnReceiver(boxCol.transform.position, boxCol.size, Color.green);
            return;
        }
    }
    void DrawCircleOnReceiver(Vector3 position, float radius, Color color)
    {
        Handles.color = color;
        Handles.DrawWireDisc(position, Vector3.forward, radius);
        
        color.a = 0.1f;
        
        Handles.color = color;

        Handles.DrawSolidDisc(position, Vector3.forward, radius);
    }
    void DrawBoxOnReceiver(Vector2 position, Vector2 size, Color color)
    {
        Handles.color = color;
        color.a = 0.1f;
        Rect rect = new Rect(position.x - size.x / 2, position.y - size.y / 2, size.x, size.y);
        Handles.DrawSolidRectangleWithOutline(rect, color, Handles.color);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(HitBoxCaster))]
public class HitBoxCasterInspector: Editor
{
    Animator animator;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        HitBoxCaster hbc = (HitBoxCaster)target;
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Add Circle"))
        {
            GameObject go = new GameObject("[Circle] " + hbc.HitBoxes.Length);
            go.transform.SetParent(hbc.transform);
            go.transform.localPosition = Vector3.one*2;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            CircleBox cd = go.AddComponent<CircleBox>();
            HitBox[] newHitBoxes = new HitBox[hbc.HitBoxes.Length + 1];
            for(int i = 0; i < hbc.HitBoxes.Length; i++)
            {
                newHitBoxes[i] = hbc.HitBoxes[i];
            }
            newHitBoxes[newHitBoxes.Length - 1] = cd;
            hbc.HitBoxes = newHitBoxes;
        }
        if(GUILayout.Button("Add Rectangle"))
        {
            GameObject go = new GameObject("[Rectangle] " + hbc.HitBoxes.Length);
            go.transform.SetParent(hbc.transform);
            go.transform.localPosition = Vector3.one*2;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            RectangleBox cd = go.AddComponent<RectangleBox>();
            HitBox[] newHitBoxes = new HitBox[hbc.HitBoxes.Length + 1];
            for(int i = 0; i < hbc.HitBoxes.Length; i++)
            {
                newHitBoxes[i] = hbc.HitBoxes[i];
            }
            newHitBoxes[newHitBoxes.Length - 1] = cd;
            hbc.HitBoxes = newHitBoxes;
        }
        GUILayout.EndHorizontal();


        // warning message
        if(animator == null) animator = hbc.GetComponent<Animator>();
        if(animator != null && animator.updateMode != AnimatorUpdateMode.AnimatePhysics)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Animator attached to HitBoxCaster needs to be set to Animate Physics", MessageType.Warning);
        }
    }
    void OnSceneGUI()
    {
        HitBoxCaster hbc = (HitBoxCaster)target;
        foreach(HitBox hb in hbc.HitBoxes)
        {
            if(hb is CircleBox)
            {
                CircleBox box = (CircleBox)hb;
                Vector3 pos = box.transform.position;
                float radius = box.Radius;
                float handleRadius = HandleUtility.GetHandleSize(Vector3.zero) * .2f;
                Vector3 handleSize = Vector3.one * 0.1f;

                EditorGUI.BeginChangeCheck();

                Vector3 center = Handles.FreeMoveHandle(pos, Quaternion.identity, handleRadius/2, handleSize/2, Handles.DotHandleCap);
                float snap = 0.1f;

                Vector3 topPrev = pos + new Vector3(0, radius, 0);
                Vector3 top = Handles.Slider(topPrev, Vector3.up, handleRadius, Handles.SphereHandleCap, snap);

                Vector3 bottomPrev = pos + new Vector3(0, -radius, 0);
                Vector3 bottom = Handles.Slider(bottomPrev, Vector3.down, handleRadius, Handles.SphereHandleCap, snap);

                Vector3 leftPrev = pos + new Vector3(-radius, 0, 0);
                Vector3 left = Handles.Slider(leftPrev, Vector3.left, handleRadius, Handles.SphereHandleCap, snap);

                Vector3 rightPrev = pos + new Vector3(radius, 0, 0);
                Vector3 right = Handles.Slider(rightPrev, Vector3.right, handleRadius, Handles.SphereHandleCap, snap);

                if(EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(box, "Change Circle Box");
                    Undo.RecordObject(box.transform, "Change Circle Box");
                    box.transform.position = center;
                    if(top != topPrev)
                    {
                        Vector3 newCenter = (top + bottom) / 2;
                        float newRadius = Mathf.Abs(top.y - bottom.y) / 2;
                        box.transform.position = newCenter;
                        box.Radius = newRadius;
                    }

                    if(bottom != bottomPrev)
                    {
                        Vector3 newCenter = (top + bottom) / 2;
                        float newRadius = Mathf.Abs(top.y - bottom.y) / 2;
                        box.transform.position = newCenter;
                        box.Radius = newRadius;
                    }

                    if(left != leftPrev)
                    {
                        Vector3 newCenter = (left + right) / 2;
                        float newRadius = Mathf.Abs(left.x - right.x) / 2;
                        box.transform.position = newCenter;
                        box.Radius = newRadius;
                    }

                    if(right != rightPrev)
                    {
                        Vector3 newCenter = (left + right) / 2;
                        float newRadius = Mathf.Abs(left.x - right.x) / 2;
                        box.transform.position = newCenter;
                        box.Radius = newRadius;
                    }
                }

            }
            else if(hb is RectangleBox)
            {
                RectangleBox box = (RectangleBox)hb;
                Vector3 pos = box.transform.position;
                Vector3 size = box.Size;
                float handleRadius = HandleUtility.GetHandleSize(Vector3.zero) * .2f;
                Vector3 handleSize = Vector3.one * 0.1f;

                EditorGUI.BeginChangeCheck();

                Vector3 center = Handles.FreeMoveHandle(pos, Quaternion.identity, handleRadius/2, handleSize/2, Handles.DotHandleCap);

                Vector3 topLeftPrev = pos + new Vector3(-size.x/2, size.y/2, 0);
                Vector3 topLeft = Handles.FreeMoveHandle(topLeftPrev, Quaternion.identity, handleRadius, handleSize, Handles.SphereHandleCap);

                Vector3 topRightPrev = pos + new Vector3(size.x/2, size.y/2, 0);
                Vector3 topRight = Handles.FreeMoveHandle(topRightPrev, Quaternion.identity, handleRadius, handleSize, Handles.SphereHandleCap);

                Vector3 bottomLeftPrev = pos + new Vector3(-size.x/2, -size.y/2, 0);
                Vector3 bottomLeft = Handles.FreeMoveHandle(bottomLeftPrev, Quaternion.identity, handleRadius, handleSize, Handles.SphereHandleCap);

                Vector3 bottomRightPrev = pos + new Vector3(size.x/2, -size.y/2, 0);
                Vector3 bottomRight = Handles.FreeMoveHandle(bottomRightPrev, Quaternion.identity, handleRadius, handleSize, Handles.SphereHandleCap);

                if(EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(box, "Change Rectangle Box");
                    Undo.RecordObject(box.transform, "Change Rectangle Box");
                    box.transform.position = center;
                    if(topLeft != topLeftPrev)
                    {
                        Vector3 newCenter = (topLeft + bottomRight) / 2;
                        Vector3 newSize = new Vector3(Mathf.Abs(topLeft.x - bottomRight.x), Mathf.Abs(topLeft.y - bottomRight.y), 0);
                        box.transform.position = newCenter;
                        box.Size = newSize;
                    }

                    if(topRight != topRightPrev)
                    {
                        Vector3 newCenter = (topRight + bottomLeft) / 2;
                        Vector3 newSize = new Vector3(Mathf.Abs(topRight.x - bottomLeft.x), Mathf.Abs(topRight.y - bottomLeft.y), 0);
                        box.transform.position = newCenter;
                        box.Size = newSize;
                    }

                    if(bottomLeft != bottomLeftPrev)
                    {
                        Vector3 newCenter = (topRight + bottomLeft) / 2;
                        Vector3 newSize = new Vector3(Mathf.Abs(topRight.x - bottomLeft.x), Mathf.Abs(topRight.y - bottomLeft.y), 0);
                        box.transform.position = newCenter;
                        box.Size = newSize;
                    }

                    if(bottomRight != bottomRightPrev)
                    {
                        Vector3 newCenter = (topLeft + bottomRight) / 2;
                        Vector3 newSize = new Vector3(Mathf.Abs(topLeft.x - bottomRight.x), Mathf.Abs(topLeft.y - bottomRight.y), 0);
                        box.transform.position = newCenter;
                        box.Size = newSize;
                    }
                }

            }
        }
    }
}

#endif
