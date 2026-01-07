using UnityEditor;
using UnityEngine;

/// <summary> ReadOnlyAttribute 用の Inspector 描画制御 </summary>
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false; // ここで編集不可に設定
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;　// ここで再び編集可能に設定
    }
}
