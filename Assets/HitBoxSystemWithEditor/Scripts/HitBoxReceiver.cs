using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class HitBoxReceiver : MonoBehaviour
{
    [Tooltip("The layer that the hitbox allowed to hit. Ignore the warning.")]
    [SerializeField, Layer] public LayerMask ReceiverLayer;
    public Action<HitBoxData> OnHitReceive;
    public Collider2D[] Collider;
#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        foreach(Collider2D col in Collider)
        {
            CircleCollider2D circleCol = col as CircleCollider2D;
            if(circleCol != null)
            {
                DrawCircleGizmos(col.transform.position, col.bounds.extents.x, Color.green);
                continue;
            }

            BoxCollider2D boxCol = col as BoxCollider2D;
            if(boxCol != null)
            {
                DrawBoxGizmos(boxCol.transform.position, boxCol.size, Color.green);
                continue;
            }
        }
    }
    void DrawCircleGizmos(Vector3 position, float radius, Color color)
    {
        Handles.color = color;
        Handles.DrawWireDisc(position, Vector3.forward, radius);
        
        color.a = 0.04f;
        
        Handles.color = color;

        Handles.DrawSolidDisc(position, Vector3.forward, radius);
    }
    void DrawBoxGizmos(Vector3 position, Vector2 size, Color color)
    {
        Handles.color = color;
        color.a = 0.04f;
        Rect rect = new Rect(position.x - size.x / 2, position.y - size.y / 2, size.x, size.y);
        Handles.DrawSolidRectangleWithOutline(rect, color, Handles.color);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(HitBoxReceiver))]
public class HitBoxReceiverInspector: Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        HitBoxReceiver hbr = (HitBoxReceiver)target;


        // Apply Layer Button
        // draw on top of the label of the layer field
        Rect layerRect = GUILayoutUtility.GetLastRect();
        layerRect.width -= EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 20;
        layerRect.y -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        layerRect.height = EditorGUIUtility.singleLineHeight;

        if(GUI.Button(layerRect, ""))
        {
            hbr.gameObject.layer = hbr.ReceiverLayer;
            foreach(Collider2D col in hbr.Collider)
            {
                col.gameObject.layer = hbr.ReceiverLayer;
            }
        }
        // get label rect by string
        string labelStr = "Apply Layer";
        float labelWidth = EditorStyles.label.CalcSize(new GUIContent(labelStr)).x;
        float leftPadding = (layerRect.width - labelWidth) / 2;
        layerRect.x += leftPadding;
        layerRect.y -= 1;
        GUI.Label(layerRect, labelStr);


        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Add Circle"))
        {
            GameObject go = new GameObject("[Circle]");
            go.transform.SetParent(hbr.transform);
            go.transform.localPosition = Vector3.one*2;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            CircleCollider2D cd = go.AddComponent<CircleCollider2D>();
            Collider2D[] newHitBoxes = new Collider2D[hbr.Collider.Length + 1];
            for(int i = 0; i < hbr.Collider.Length; i++)
            {
                newHitBoxes[i] = hbr.Collider[i];
            }
            newHitBoxes[newHitBoxes.Length - 1] = cd;
            hbr.Collider = newHitBoxes;
        }
        if(GUILayout.Button("Add Rectangle"))
        {
            GameObject go = new GameObject("[Rectangle]");
            go.transform.SetParent(hbr.transform);
            go.transform.localPosition = Vector3.one*2;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            BoxCollider2D cd = go.AddComponent<BoxCollider2D>();
            Collider2D[] newHitBoxes = new Collider2D[hbr.Collider.Length + 1];
            for(int i = 0; i < hbr.Collider.Length; i++)
            {
                newHitBoxes[i] = hbr.Collider[i];
            }
            newHitBoxes[newHitBoxes.Length - 1] = cd;
            hbr.Collider = newHitBoxes;
        }
        GUILayout.EndHorizontal();


        // warning message
        if(hbr.Collider[0] != null && hbr.Collider[0].attachedRigidbody == null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("HitBoxReceiver need to have kinematic Rigidbody2D attached or as parent to work properly", MessageType.Warning);
        }
        if(hbr.Collider[0] != null && hbr.Collider[0].attachedRigidbody != null && hbr.Collider[0].attachedRigidbody.bodyType != RigidbodyType2D.Kinematic)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Please set the Rigidbody2D attached to HitBoxReceiver or parent of HitBoxReceiver to kinematic", MessageType.Warning);
        }
    }

    void OnSceneGUI()
    {
        HitBoxReceiver hbr = (HitBoxReceiver)target;
        foreach(Collider2D col in hbr.Collider)
        {
            if(col is CircleCollider2D)
            {
                CircleCollider2D box = (CircleCollider2D)col;
                Vector3 pos = box.transform.position;
                float radius = box.radius;
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
                        box.radius = newRadius;
                    }

                    if(bottom != bottomPrev)
                    {
                        Vector3 newCenter = (top + bottom) / 2;
                        float newRadius = Mathf.Abs(top.y - bottom.y) / 2;
                        box.transform.position = newCenter;
                        box.radius = newRadius;
                    }

                    if(left != leftPrev)
                    {
                        Vector3 newCenter = (left + right) / 2;
                        float newRadius = Mathf.Abs(left.x - right.x) / 2;
                        box.transform.position = newCenter;
                        box.radius = newRadius;
                    }

                    if(right != rightPrev)
                    {
                        Vector3 newCenter = (left + right) / 2;
                        float newRadius = Mathf.Abs(left.x - right.x) / 2;
                        box.transform.position = newCenter;
                        box.radius = newRadius;
                    }
                }



            }
            else if(col is BoxCollider2D)
            {
                BoxCollider2D box = (BoxCollider2D)col;
                Vector3 pos = box.transform.position;
                Vector3 size = box.size;
                float handleRadius = HandleUtility.GetHandleSize(Vector3.zero) * .2f;
                Vector3 handleSize = Vector3.one * 0.1f;

                EditorGUI.BeginChangeCheck();

                Vector3 center = Handles.FreeMoveHandle(pos, Quaternion.identity, handleRadius/2, handleSize/2, Handles.DotHandleCap);
                box.transform.position = center;

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
                        box.size = newSize;
                    }

                    if(topRight != topRightPrev)
                    {
                        Vector3 newCenter = (topRight + bottomLeft) / 2;
                        Vector3 newSize = new Vector3(Mathf.Abs(topRight.x - bottomLeft.x), Mathf.Abs(topRight.y - bottomLeft.y), 0);
                        box.transform.position = newCenter;
                        box.size = newSize;
                    }

                    if(bottomLeft != bottomLeftPrev)
                    {
                        Vector3 newCenter = (topRight + bottomLeft) / 2;
                        Vector3 newSize = new Vector3(Mathf.Abs(topRight.x - bottomLeft.x), Mathf.Abs(topRight.y - bottomLeft.y), 0);
                        box.transform.position = newCenter;
                        box.size = newSize;
                    }

                    if(bottomRight != bottomRightPrev)
                    {
                        Vector3 newCenter = (topLeft + bottomRight) / 2;
                        Vector3 newSize = new Vector3(Mathf.Abs(topLeft.x - bottomRight.x), Mathf.Abs(topLeft.y - bottomRight.y), 0);
                        box.transform.position = newCenter;
                        box.size = newSize;
                    }
                }


                
            }
        }
    }
}

#endif
