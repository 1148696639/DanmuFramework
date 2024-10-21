using System;
using UnityEngine;

public class TestUtility
{
    public static void InputA(Action action)
    {
        InputCheck(KeyCode.A, action);
    }

    public static void InputB(Action action)
    {
        InputCheck(KeyCode.B, action);
    }

    public static void InputC(Action action)
    {
        InputCheck(KeyCode.C, action);
    }

    public static void InputD(Action action)
    {
        InputCheck(KeyCode.D, action);
    }

    private static void InputCheck(KeyCode key, Action action)
    {
        if (Input.GetKeyDown(key)) action?.Invoke();
    }
}