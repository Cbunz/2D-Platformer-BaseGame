using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(TileControl))]
public class TileControlEditor : Editor
{
    static BoxBoundsHandle boxBoundsHandle = new BoxBoundsHandle();
    static Color enabledColor = Color.green + Color.grey;

    SerializedProperty dimensionsProperty;
    //SerializedProperty centerProperty;

    private void OnEnable()
    {
        dimensionsProperty = serializedObject.FindProperty("dimensions");
        //centerProperty = serializedObject.FindProperty("center");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(dimensionsProperty);
        //EditorGUILayout.PropertyField(centerProperty);

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        TileControl tileControl = (TileControl)target;

        if (!tileControl.enabled)
            return;

        Matrix4x4 handleMatrix = tileControl.transform.localToWorldMatrix;
        handleMatrix.SetRow(0, Vector4.Scale(handleMatrix.GetRow(0), new Vector4(1f, 1f, 0f, 1f)));
        handleMatrix.SetRow(1, Vector4.Scale(handleMatrix.GetRow(1), new Vector4(1f, 1f, 0f, 1f)));
        handleMatrix.SetRow(2, new Vector4(0f, 0f, 1f, tileControl.transform.position.z));

        using (new Handles.DrawingScope(handleMatrix))
        {
            boxBoundsHandle.center = tileControl.center;
            boxBoundsHandle.size = tileControl.dimensions;

            boxBoundsHandle.SetColor(enabledColor);
            EditorGUI.BeginChangeCheck();
            boxBoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tileControl, "Modify Tile Control");

                tileControl.dimensions = new Vector2((int)boxBoundsHandle.size.x, (int)boxBoundsHandle.size.y);
            }
        }
    }
}
