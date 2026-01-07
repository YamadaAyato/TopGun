using UnityEditor;
using UnityEngine;

/// <summary> ReadOnlyAttribute 用の Inspector 描画制御 </summary>
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false; // Inspector上のGUI入力を無効化
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;　// GUIの有効状態を元に戻す
    }
}
