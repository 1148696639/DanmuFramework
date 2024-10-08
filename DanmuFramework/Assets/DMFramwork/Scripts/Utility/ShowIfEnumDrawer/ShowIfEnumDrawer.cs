using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfEnumAttribute))]
public class ShowIfEnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 获取 ShowIfEnumAttribute 信息
        ShowIfEnumAttribute showIf = (ShowIfEnumAttribute)attribute;
        SerializedProperty enumField = property.serializedObject.FindProperty(showIf.EnumFieldName);

        if (enumField != null && enumField.propertyType == SerializedPropertyType.Enum)
        {
            // 检查枚举的值是否等于指定值
            if (enumField.enumValueIndex == showIf.EnumValue)
            {
                // 如果条件符合，显示属性
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
        else
        {
            // 如果找不到对应的枚举字段，显示错误信息
            EditorGUI.LabelField(position, label.text, "Error: Enum field not found or not an enum");
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfEnumAttribute showIf = (ShowIfEnumAttribute)attribute;
        SerializedProperty enumField = property.serializedObject.FindProperty(showIf.EnumFieldName);

        if (enumField != null && enumField.propertyType == SerializedPropertyType.Enum && enumField.enumValueIndex == showIf.EnumValue)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        return 0; // 不符合条件时隐藏字段
    }
}