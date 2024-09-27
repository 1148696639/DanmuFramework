using System;
using UnityEngine;

namespace DMFramework
{
    public static class DebugCtrl
    {
        public static bool LogIsOpen;

        /// <summary>
        ///     DebugCtrl.Log(mes)
        /// </summary>
        /// <param name="mes"></param>
        public static void Log(object mes)
        {
            // if (LogIsOpen)
            Debug.Log(DateTime.Now.ToString()+"----"+mes);
        }

        public static void LogWarning(object mes)
        {
            // if (LogIsOpen)
            Debug.LogWarning(DateTime.Now.ToString()+"----"+mes);
        }

        public static void LogError(object mes)
        {
            // if (LogIsOpen)
            Debug.LogError(DateTime.Now.ToString()+"----"+mes);
        }

    }
}