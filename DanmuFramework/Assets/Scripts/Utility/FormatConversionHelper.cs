using System;
using UnityEngine;

namespace QFramework
{
    public class FormatConversionHelper
    {
        /// <summary>
        ///     将字符串转换为对应的枚举类型
        /// </summary>
        /// <param Name="value"></param>
        /// <typeparam Name="T"></typeparam>
        /// <returns></returns>
        public static T StringToEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        } 
        
        /// <summary>
        ///   将整数转换为对应的枚举类型
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T IntToEnum<T>(int value) where T : Enum
        {
            return (T)Enum.ToObject(typeof(T), value);
        }


        /// <summary>
        ///     将枚举转换为对应字符串
        /// </summary>
        /// <typeparam Name="T"></typeparam>
        /// <param Name="intValue"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ConvertIntToEnumString<T>(int intValue) where T : Enum
        {
            if (Enum.IsDefined(typeof(T), intValue))
                return Enum.GetName(typeof(T), intValue);
            throw new ArgumentException("所提供的整数值与任何枚举值都不对应。");
        }

        /// <summary>
        ///     将枚举转换为对应下标
        /// </summary>
        /// <param Name="enumValue"></param>
        /// <returns></returns>
        public static int EnumToInt(Enum enumValue)
        {
            return Convert.ToInt32(enumValue);
        }


        public static string ClassToJsonStr<T>(T t)
        {
            var jsonStr = JsonUtility.ToJson(t);
            return jsonStr;
        }

        /// <summary>
        ///     将base64字符串转换为对应的Texture2D类型
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static Texture2D DecodeBase64ToTexture(string base64String)
        {
            try
            {
                var imageBytes = Convert.FromBase64String(base64String);
                var texture = new Texture2D(1, 1);
                texture.LoadImage(imageBytes); // 自动解析图像格式并加载
                return texture;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error decoding base64 string to texture: " + ex.Message);
                return null;
            }
        }
    }
}