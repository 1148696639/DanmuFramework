using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ShowIfEnumAttribute : PropertyAttribute
{
    public string EnumFieldName { get; }
    public int EnumValue { get; }

    public ShowIfEnumAttribute(string enumFieldName, int enumValue)
    {
        EnumFieldName = enumFieldName;
        EnumValue = enumValue;
    }
}