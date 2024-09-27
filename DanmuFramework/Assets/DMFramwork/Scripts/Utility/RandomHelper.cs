using UnityEngine;

public class RandomHelper
{
    // 返回一个在 -value 到 value 之间的随机浮点数
    public static float GetRandomValue(float value)
    {
        return Random.Range(-value, value);
    }

    // 返回一个在 0 到 value 之间的随机浮点数
    public static float GetRandomValue2(float value)
    {
        return Random.Range(0, value);
    }

    // 返回一个 Vector3，其每个分量都是在 -value 到 value 之间的随机浮点数
    public static Vector3 GetRandomVector(Vector3 value)
    {
        Vector3 result;
        result.x = GetRandomValue(value.x);
        result.y = GetRandomValue(value.y);
        result.z = GetRandomValue(value.z);
        return result;
    }

    // 返回一个 Vector3，其每个分量都是在 0 到 value 之间的随机浮点数
    public static Vector3 GetRandomVector2(Vector3 value)
    {
        Vector3 result;
        result.x = GetRandomValue2(value.x);
        result.y = GetRandomValue2(value.y);
        result.z = GetRandomValue2(value.z);
        return result;
    }
}