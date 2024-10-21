using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfEnumAttribute))]
public class ShowIfEnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var showIf = (ShowIfEnumAttribute)attribute;

        // 使用完整路径查找枚举字段
        var enumFieldPath = property.propertyPath.Replace(property.name, showIf.EnumFieldName);
        var enumField = property.serializedObject.FindProperty(enumFieldPath);

        if (enumField != null && enumField.propertyType == SerializedPropertyType.Enum)
        {
            // Debug.Log($"Enum Field Value: {enumField.enumValueIndex}, Expected: {showIf.EnumValue}");

            if (enumField.enumValueIndex == showIf.EnumValue)
                EditorGUI.PropertyField(position, property, label, true); // 正常绘制
        }
        else
        {
            Debug.LogWarning($"Enum Field {showIf.EnumFieldName} not found or not an enum at path: {enumFieldPath}");
            EditorGUI.PropertyField(position, property, label, true); // 正常绘制
        }
    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var showIf = (ShowIfEnumAttribute)attribute;

        // 使用完整路径查找枚举字段
        var enumFieldPath = property.propertyPath.Replace(property.name, showIf.EnumFieldName);
        var enumField = property.serializedObject.FindProperty(enumFieldPath);

        // 确保字段高度正确分配
        if (enumField != null && enumField.propertyType == SerializedPropertyType.Enum)
            if (enumField.enumValueIndex == showIf.EnumValue)
                // 如果条件匹配，返回字段的正常高度
                return EditorGUI.GetPropertyHeight(property, label, true);

        // 如果条件不匹配，返回 0 表示隐藏该字段
        return 0;
    }
}